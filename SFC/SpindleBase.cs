using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Modbus.Device;

namespace MachineClassLibrary.SFC;

public abstract class SpindleBase<T>: ISpindle, IDisposable
{
    /// <summary>
    ///     300 Hz = 18000 rpm
    /// </summary>
    public const ushort LOW_FREQ_LIMIT = 3000;

    /// <summary>
    ///     550 Hz = 33000 rpm
    /// </summary>
    public const ushort HIGH_FREQ_LIMIT = 5500;

    private readonly int _baudRate;
    private readonly string _com;
    protected readonly ILogger<T> _logger;

    private readonly object _modbusLock = new();
    protected ModbusSerialMaster _client;
    private SemaphoreSlim _semaphoreSlim = new(1,1);
    protected int _freq;
    private bool _hasStarted = false;
    private bool _onFreq;
    private SerialPort _serialPort;
    private CancellationTokenSource _watchingStateCancellationTokenSource;
    protected ushort _tempFault;

    // TODO wait o cancel in the end, NEVER forget Tasks
    private Task _watchingStateTask;

    protected SpindleBase(string com, int baudRate, ILogger<T> logger)
    {
        _com = com;
        _baudRate = baudRate;
        _logger = logger;
    }
    public bool IsConnected { get; set; } = false;

    public event EventHandler<SpindleEventArgs> GetSpindleState;

    public async Task<bool> ChangeSpeedAsync(ushort rpm, int delay)
    {
        //if (!_hasStarted) return Task.FromResult(false);
        if (rpm == _freq * 6) return true;// Task.FromResult(true);
        try
        {
            SetSpeedAsync(rpm);
            if (!_hasStarted)
            {
                StartAsync();
                await Task.Delay(3000).ConfigureAwait(false);
            }
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(delay));
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

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in ChangeSpeedAsync");
            return false;// Task.FromResult(false);
        }
    }
    /// <summary>
    /// Connects to the spindle
    /// </summary>
    /// <returns>true if success</returns>
    /// <exception cref="SpindleException"></exception>
    public async Task<bool> ConnectAsync()
    {
        if (EstablishConnection())
        {
            _watchingStateCancellationTokenSource = new CancellationTokenSource();
            _watchingStateTask = WatchingStateAsync(_watchingStateCancellationTokenSource.Token);
            var isWorking = await CheckSpindleWorkingAsync().ConfigureAwait(false);
            if (isWorking) return true;
            var paramsIsSet = await SetParamsAsync().ConfigureAwait(false);
            if (!paramsIsSet) throw new SpindleException("SetParams is failed");
        }
        else
        {
            return false;
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
        _client?.Dispose();
        _serialPort?.Close();
        _serialPort?.Dispose();
        _watchingStateCancellationTokenSource?.Dispose();
    }

    public async Task SetSpeedAsync(ushort rpm)
    {
        if (!(rpm / 6 > LOW_FREQ_LIMIT && rpm / 6 < HIGH_FREQ_LIMIT))
        {
            throw new SpindleException($"{rpm}rpm is out of ({LOW_FREQ_LIMIT * 6},{HIGH_FREQ_LIMIT * 6}) rpm range");
        }
        rpm = (ushort)Math.Abs(rpm / 6);
        //lock (_modbusLock)
        //{

        await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            await WriteRPMAsync(rpm).ConfigureAwait(false);
            _logger.LogInformation($"Successfully 0x0001: {rpm} rpm");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to 0x0001");
            throw new SpindleException($"Failed to set {rpm} rpm", ex);
        }
        finally
        {
            _semaphoreSlim?.Release();
        }
        //}
    }

    public async Task StartAsync()
    {
        //lock (_modbusLock)
        //{
        await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            await StartFWDCommandAsync().ConfigureAwait(false);
            _logger.LogInformation($"Successfully wrote started forward command");
            _hasStarted = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to write start forward command");
            throw new SpindleException($"Failed to start fwd", ex);
        }
        finally
        {
            _semaphoreSlim?.Release();
        }
        //}
    }

    public async Task StopAsync()
    {
        //lock (_modbusLock)
        //{
        await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            await StopCommandAsync().ConfigureAwait(false);
            _logger.LogInformation($"Successfully wrote stop command");
            _hasStarted = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to write stop command");
            throw;
        }
        finally
        {
            _semaphoreSlim?.Release();
        }
        //}
    }
    protected record SpinStatus(bool OnFreq, bool Acc, bool Dec, bool Stop);
    protected record SpinFault(bool IsOk,  int FaultCode, string FaultDescription);
    protected abstract Task<bool> CheckIfSpindleSpinningAsync();
    protected abstract Task<int> GetCurrentAsync();
    protected abstract Task<int> GetFrequencyAsync();
    protected abstract Task<SpinStatus> GetStatusAsync();
    protected abstract Task StartFWDCommandAsync();
    protected abstract Task StopCommandAsync();
    protected abstract Task WriteRPMAsync(ushort rpm);
    protected abstract Task WriteSettingsAsync();

    protected virtual Task<SpinFault> GetSpinFaultAsync() => Task.FromResult(new SpinFault(true, -1, string.Empty));


    private async Task<bool> CheckSpindleWorkingAsync()
    {
        //lock (_modbusLock)
        //{
        await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
        if (_client == null)
        {
            _logger.LogWarning("Attempted to check spindle state, but Modbus client is not initialized.");
            return false;
        }
        try
        {
            return await CheckIfSpindleSpinningAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to 0xD000");
            return false;
        }
        finally
        {
            _semaphoreSlim?.Release();
        }
        //}
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
    private async Task<bool> SetParamsAsync()
    {
        //lock (_modbusLock)
        //{
        await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            await WriteSettingsAsync().ConfigureAwait(false);
            _logger.LogInformation("Successfully set spindle params");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to set params");
            return false;   
        }
        finally 
        {
            _semaphoreSlim?.Release();
        }
        //}
        return true;
    }
    private async Task WatchingStateAsync(CancellationToken token)
    {
        await Task.Delay(100).ConfigureAwait(false);
        while (!token.IsCancellationRequested)
        {
            try
            {
                int current;
                _serialPort?.DiscardInBuffer();
                current = await GetCurrentAsync().ConfigureAwait(false);
                _freq = await GetFrequencyAsync().ConfigureAwait(false);

                var status = await GetStatusAsync().ConfigureAwait(false);
                var fault = await GetSpinFaultAsync().ConfigureAwait(false);
                _onFreq = status.OnFreq;
                _hasStarted = _onFreq || status.Acc;
                GetSpindleState?.Invoke(this, new SpindleEventArgs(_freq * 6, (double)current / 10, _onFreq, status.Acc, status.Dec, status.Stop) 
                {
                    IsOk = fault.IsOk,
                    FaultCode = fault.FaultCode,
                    FaultDescription = fault.FaultDescription
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed {nameof(WatchingStateAsync)}");//TODO count how many frequent
            }
            await Task.Delay(100).ConfigureAwait(false);
        }
    }
}
