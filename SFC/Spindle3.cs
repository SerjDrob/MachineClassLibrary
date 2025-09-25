using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Modbus.Device;

namespace MachineClassLibrary.SFC
{
    public class Spindle3 : ISpindle, IDisposable
    {
        /// <summary>
        ///     300 Hz = 18000 rpm
        /// </summary>
        private const ushort LOW_FREQ_LIMIT = 3000;

        /// <summary>
        ///     550 Hz = 33000 rpm
        /// </summary>
        private const ushort HIGH_FREQ_LIMIT = 5500;
        private const ushort READ_FREQ_CURRENT = 0xD000;

        private const ushort READ_STATE = 0x2000;

        private const ushort WRITE_COMMAND = 0x1001;

        private const ushort COMMAND_STOP = 0x0003;
        private const ushort COMMAND_START_FWD = 0x0001;

        private const ushort STATE_ON_FREQ_FWD = 0x0001;
        private const ushort STATE_ON_FREQ_REV = 0x0002;
        private const ushort STATE_ACC_FWD = 0x0011;
        private const ushort STATE_ACC_REV = 0x0012;
        private const ushort STATE_DEC_FWD = 0x0014;
        private const ushort STATE_DEC_REV = 0x0015;
        private const ushort STATE_STOP = 0x0003;

        private readonly object _modbusLock = new();
        private readonly string _com;
        private readonly int _baudRate;
        private readonly ILogger<Spindle3> _logger;
        private ModbusSerialMaster _client;
        private SerialPort _serialPort;
        private bool _hasStarted = false;
        private int _freq;
        private bool _onFreq;
        private CancellationTokenSource _watchingStateCancellationTokenSource;

        // TODO wait o cancel in the end, NEVER forget Tasks
        private Task _watchingStateTask;

        public Spindle3(string com, int baudRate, ILogger<Spindle3> logger)
        {
            _com = com;
            _baudRate = baudRate;
            _logger = logger;
        }
        public event EventHandler<SpindleEventArgs> GetSpindleState;
        public bool IsConnected { get; set; } = false;
        private bool CheckSpindleWorking()
        {
            lock (_modbusLock)
            {
                if (_client == null)
                {
                    _logger.LogWarning("Attempted to check spindle state, but Modbus client is not initialized.");
                    return false;
                }
                try
                {
                    var data = _client.ReadHoldingRegisters(1, 0xD000, 1);
                    return data[0] != 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to 0xD000");
                    return false;
                }
            }
        }

        public void SetSpeed(ushort rpm)
        {
            if (!(rpm / 6 > LOW_FREQ_LIMIT && rpm / 6 < HIGH_FREQ_LIMIT))
            {
                throw new SpindleException($"{rpm}rpm is out of ({LOW_FREQ_LIMIT * 6},{HIGH_FREQ_LIMIT * 6}) rpm range");
            }
            rpm = (ushort)Math.Abs(rpm / 6);
            lock (_modbusLock)
            {
                try
                {
                    _client.WriteSingleRegister(1, 0x0001, rpm);
                    _logger.LogInformation($"Successfully 0x0001: {rpm} rpm");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to 0x0001");
                    throw;
                }
            }
        }

        public void Start()
        {
            lock (_modbusLock)
            {
                try
                {
                    _client.WriteSingleRegister(1, WRITE_COMMAND, COMMAND_START_FWD);
                    _logger.LogInformation($"Successfully {WRITE_COMMAND}: {COMMAND_START_FWD}");
                    _hasStarted = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to {WRITE_COMMAND}: {COMMAND_START_FWD}");
                    throw;
                }
            }
        }
        public void Stop()
        {
            lock (_modbusLock)
            {
                try
                {
                    _client.WriteSingleRegister(1, WRITE_COMMAND, COMMAND_STOP);
                    _logger.LogInformation($"Successfully {WRITE_COMMAND}: {COMMAND_STOP}");
                    _hasStarted = false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to {WRITE_COMMAND}: {COMMAND_STOP}");
                    throw;
                }
            }
        }
        private bool EstablishConnection()
        {
            _serialPort = new SerialPort
            {
                PortName = _com,
                BaudRate = _baudRate,
                Parity = Parity.Even,
                WriteTimeout = 1000,
                ReadTimeout = 100
            };
            _logger.LogInformation("Attempting to open serial port: {PortName}", _com);
            _serialPort.Open();
            if (_serialPort.IsOpen)
            {
                _client = ModbusSerialMaster.CreateRtu(_serialPort);
                _logger.LogInformation("Serial port {PortName} opened successfully. Modbus client created.", _com);
            }
            else
            {
                _logger.LogWarning("Failed to open serial port: {PortName}", _com);
                return false;
            }

            return IsConnected = true;
        }
        private async Task WatchingStateAsync(CancellationToken token)
        {
            ushort[] data = default;
            bool acc = false;
            bool dec = false;
            bool stop = false;
            await Task.Delay(100).ConfigureAwait(false);
            while (!token.IsCancellationRequested)
            {
                try
                {
                    int current;
                    _serialPort?.DiscardInBuffer();
                    data = await _client.ReadHoldingRegistersAsync(1, READ_FREQ_CURRENT, 2).ConfigureAwait(false);
                    current = data[1];
                    _freq = data[0];
                    data = await _client.ReadHoldingRegistersAsync(1, READ_STATE, 1).ConfigureAwait(false);
                    _onFreq = data[0] == STATE_ON_FREQ_FWD | data[0] == STATE_ON_FREQ_REV;
                    acc = data[0] == STATE_ACC_FWD | data[0] == STATE_ACC_REV;
                    dec = data[0] == STATE_DEC_FWD | data[0] == STATE_DEC_REV;
                    stop = data[0] == STATE_STOP;
                    _hasStarted = _onFreq || acc;
                    GetSpindleState?.Invoke(this, new SpindleEventArgs(_freq * 6, (double)current / 10, _onFreq, acc, dec, stop));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed {nameof(WatchingStateAsync)}");//TODO count how many frequent
                }
                await Task.Delay(100).ConfigureAwait(false);
            }
        }
        private bool SetParams()
        {
            lock (_modbusLock)
            {
                _client.WriteMultipleRegisters(1, 0x0000,
                [
                    0,
                    5000,
                    2,
                    LOW_FREQ_LIMIT, //500,//lower limiting frequency/10
                    HIGH_FREQ_LIMIT, //upper limiting frequency/10
                    900 //acceleration time/10
                ]);

                _client.WriteMultipleRegisters(1, 0x000B,
                [
                    60, //torque boost/10, 0.0 - 20.0%
                    5200, //basic running frequency/10
                    50 //maximum output voltage 50 - 500V
                ]);

                _client.WriteMultipleRegisters(1, 0x020F,
                [
                    4999, //f3/10
                    30 //V3
                ]);

                _client.WriteMultipleRegisters(1, 0x020D,
                [
                    1200, //f2/10
                    20 //V2
                ]);

                _client.WriteMultipleRegisters(1, 0x020B,
                [
                    800, //f1/10
                    10 //V1
                ]);
            }
            return true;
        }
        public void Dispose()
        {
            _watchingStateCancellationTokenSource?.Cancel();
            try
            {
                _watchingStateTask?.Wait(1000);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Spindle state monitoring task did not stop gracefully.");
            }
            _serialPort?.Dispose();
            _client?.Dispose();
            _watchingStateCancellationTokenSource?.Dispose();
        }
        /// <summary>
        /// Connects to the spindle
        /// </summary>
        /// <returns>true if success</returns>
        /// <exception cref="SpindleException"></exception>
        public bool Connect()
        {
            if (EstablishConnection())
            {
                _watchingStateCancellationTokenSource = new CancellationTokenSource();
                _watchingStateTask = WatchingStateAsync(_watchingStateCancellationTokenSource.Token);
                if (CheckSpindleWorking())
                {
                    return true;
                }
                if (!SetParams()) throw new SpindleException("SetParams is failed");
            }
            else
            {
                return false;
            }
            return true;
        }
        public Task<bool> ChangeSpeedAsync(ushort rpm, int delay)
        {
            if (!_hasStarted) return Task.FromResult(false);
            if (rpm == _freq * 6) return Task.FromResult(true);
            try
            {
                SetSpeed(rpm);
                var cts = new CancellationTokenSource(delay);
                var tcs = new TaskCompletionSource<bool>();

                void ReachedFreq(object sender, SpindleEventArgs args)
                {
                    if (args.OnFreq)
                    {
                        CleanupAndSetResult(true);
                    }
                    else if (!args.IsOk || args.Stop)
                    {
                        CleanupAndSetResult(false);
                    }
                }

                void CleanupAndSetResult(bool result)
                {
                    GetSpindleState -= ReachedFreq;
                    cts.Dispose();
                    tcs.TrySetResult(result);
                }

                GetSpindleState += ReachedFreq;

                cts.Token.Register(() =>
                {
                    GetSpindleState -= ReachedFreq;
                    tcs.TrySetResult(false);
                });

                return tcs.Task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in ChangeSpeedAsync");
                return Task.FromResult(false);
            }
        }
    }
}
