using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MachineClassLibrary.SFC;

//public class CommanderSK : ISpindle, IDisposable
//{

//    private readonly ushort LowFreqLimit;
//    private readonly ushort HighFreqLimit;
//    private readonly object _modbusLock = new();
//    private readonly string _comPort;
//    private readonly int _baudRate;
//    private readonly SpindleParams _spindleParams;
//    private ModbusSerialMaster _client;
//    private SerialPort _serialPort;

//    // TODO wait or cancel in the end, NEVER forget Tasks
//    private Task _watchingStateTask;

//    public CommanderSK(string comPort, int baudRate, SpindleParams spindleParams)
//    {
//        _comPort = comPort;
//        _baudRate = baudRate;
//        _spindleParams = spindleParams;
//        LowFreqLimit = _spindleParams.MinFreq;
//        HighFreqLimit = _spindleParams.MaxFreq;
//    }

//    public bool Connect()
//    {
//        if (EstablishConnection(_comPort, _baudRate))
//        {
//            _watchingStateTask = WatchingStateAsync();
//            if (CheckSpindleWorking()) return true;
//            if (!SetParams()) throw new SpindleException("SetParams is failed");
//            return true;
//        }
//        else return false;
//    }
//    private bool CheckSpindleWorking()
//    {
//        lock (_modbusLock)
//        {
//            var data = _client.ReadHoldingRegisters(1, 0x43E9, 2);//Pr10.02
//            return data[1] != 0;
//        }
//    }


//    public event EventHandler<SpindleEventArgs> GetSpindleState;


//    public bool IsConnected { get; set; } = false;
//    /// <summary>
//    /// Set spindle's rpm
//    /// </summary>
//    /// <param name="rpm"></param>
//    /// <exception cref="SpindleException"></exception>
//    public void SetSpeedAsync(ushort rpm)
//    {
//        if (!(rpm / 60 > LowFreqLimit && rpm / 60 < HighFreqLimit))
//        {
//            throw new SpindleException($"{rpm} rpm is out of ({LowFreqLimit * 60},{HighFreqLimit * 60}) rpm range");
//        }
//        rpm = (ushort)Math.Abs(rpm / 6);
//        lock (_modbusLock)
//        {
//            var high = rpm;
//            var low = (ushort)(rpm - 5);
//            _client.WriteMultipleRegisters(1, 0x4069,
//                new ushort[] { 0, high, 0, low });//Pr01.06 - high limit of speed, Pr01.07 - low limit of speed
//            _onFreq = false;
//        }
//    }
//    /// <summary>
//    /// Changing spindle's rpm on fly and waiting it to settle
//    /// </summary>
//    /// <param name="rpm">spindle's new rpm</param>
//    /// <param name="delay">delay after settling the rpm</param>
//    /// <returns>true if success</returns>
//    public async Task<bool> ChangeSpeedAsync(ushort rpm, int delay)
//    {
//        if(!_hasStarted) return false;
//        if (rpm == _freq * 6) return true;
//        try
//        {
//            SetSpeedAsync(rpm);
//            if (_onFreq) return true;
//            await Task.Run(async () => { while (!_onFreq) await Task.Delay(50); });
//            await Task.Delay(delay);
//            return true;
//        }
//        catch 
//        {
//            return false;
//        }
//    }


//    public void StartAsync()
//    {
//        lock (_modbusLock)
//        {
//            _client.WriteMultipleRegisters(1, 0x4279, new ushort[] { 0, 1 });//Pr6.34
//            _hasStarted = true;
//        }
//    }

//    private bool _hasStarted = false;
//    private ushort _freq;
//    private ushort _current;
//    private bool _onFreq;
//    private bool _stop;
//    private bool _acc;
//    private bool _dec;
//    private bool _isOk;

//    public void Stop()
//    {
//        lock (_modbusLock)
//        {
//            _client.WriteMultipleRegisters(1, 0x4279, new ushort[] { 0, 0 });//Pr6.34
//            _hasStarted = false;
//        }
//    }

//    private bool EstablishConnection(string com, int baudRate)
//    {
//        _serialPort = new SerialPort
//        {
//            PortName = com,
//            BaudRate = baudRate,
//            Parity = Parity.None,
//            WriteTimeout = 1000,
//            ReadTimeout = 500
//        };

//        _serialPort.Open();
//        if (_serialPort.IsOpen)
//        {
//            _client = ModbusSerialMaster.CreateRtu(_serialPort);
//            return IsConnected = true;
//        }
//        else
//        {
//            return false;
//        }
//    }

//    private async Task WatchingStateAsync()
//    {
//        while (true)
//        {
//            try
//            {
//                lock (_modbusLock)
//                {
//                    _freq = _client.ReadHoldingRegisters(1, 0x41F4, 2)[1];//Pr5.01
//                    _current = _client.ReadHoldingRegisters(1, 0x4190, 2)[1];
//                    _onFreq = _client.ReadHoldingRegisters(1, 0x43ED, 2)[1] == 1;
//                    if(_onFreq) _hasStarted = true;
//                    _stop = _client.ReadHoldingRegisters(1, 0x43EA, 2)[1] == 1;
//                    _acc = !_stop & !_onFreq & _hasStarted;
//                    _dec = !_stop & !_onFreq & !_hasStarted;
//                    _isOk = _client.ReadHoldingRegisters(1, 0x43E8, 2)[1] == 1;

//                    GetSpindleState?.Invoke(this,
//                        new SpindleEventArgs
//                        {
//                            Rpm = _freq * 6,
//                            Current = _current / 100d,
//                            OnFreq = _onFreq,
//                            Accelerating = _acc,
//                            Deccelarating = _dec,
//                            Stop = _stop,
//                            IsOk = _isOk
//                        });
//                }
//            }
//            catch (ModbusException)
//            {
//                //throw;
//            }

//            await Task.Delay(100).ConfigureAwait(false);
//        }

//    }

//    private bool SetParams()
//    {
//        lock (_modbusLock)
//        {
//            _client.WriteMultipleRegisters(1, 0x40D2, new ushort[] { 0, (ushort)(10 * _spindleParams.Acc) });//Pr2.11
//            _client.WriteMultipleRegisters(1, 0x40DC, new ushort[] { 0, (ushort)(10 * _spindleParams.Dec) });//Pr2.21
//            _client.WriteMultipleRegisters(1, 0x41FC, new ushort[] { 0, _spindleParams.RatedVoltage });//Pr5.09
//            _client.WriteMultipleRegisters(1, 0x41FA, new ushort[] { 0, (ushort)(100 * _spindleParams.RatedCurrent) });//Pr5.07
//        }

//        return true;
//    }

//    public void Dispose()
//    {
//        _watchingStateTask.Dispose();
//        _serialPort.Dispose();
//        _client.Dispose();
//    }
//}


public class CommanderSK : SpindleBase<CommanderSK>
{
    private readonly SpindleParams _spindleParams;

    public CommanderSK(string com, int baudRate, ILogger<CommanderSK> logger, SpindleParams spindleParams) : base(com, baudRate, logger)
    {
        _spindleParams = spindleParams;
    }

    protected override async Task<bool> CheckIfSpindleSpinningAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, 0x43E9, 2).ConfigureAwait(false);//Pr10.02
        return data[1] != 0;
    }

    protected override async Task<int> GetCurrentAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, 0x4190, 2).ConfigureAwait(false);
        return data[1];
    }

    protected override async Task<int> GetFrequencyAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, 0x41F4, 2).ConfigureAwait(false);//Pr5.01
        return data[1];
    }

    protected override async Task<SpinStatus> GetStatusAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, 0x43ED, 2).ConfigureAwait(false);
        var onFreq = data[1] == 1;
        data = await _client.ReadHoldingRegistersAsync(1, 0x43EA, 2).ConfigureAwait(false);
        var stop = data[1] == 1;
        var acc = !stop & !onFreq;
        var dec = !stop & !onFreq;
        return new SpinStatus(onFreq, acc, dec, stop);
    }

    protected override async Task StartFWDCommandAsync() => await _client.WriteMultipleRegistersAsync(1, 0x4279, [0, 1]).ConfigureAwait(false);//Pr6.34

    protected override async Task StopCommandAsync() => await _client.WriteMultipleRegistersAsync(1, 0x4279, [0, 0]).ConfigureAwait(false);//Pr6.34

    protected override async Task WriteRPMAsync(ushort rpm)
    {
        var high = rpm;
        var low = (ushort)(rpm - 5);
        await _client.WriteMultipleRegistersAsync(1, 0x4069,
            [0, high, 0, low]).ConfigureAwait(false);//Pr01.06 - high limit of speed, Pr01.07 - low limit of speed
    }

    protected override async Task WriteSettingsAsync()
    {
        await _client.WriteMultipleRegistersAsync(1, 0x40D2, [0, (ushort)(10 * _spindleParams.Acc)]).ConfigureAwait(false);//Pr2.11
        await _client.WriteMultipleRegistersAsync(1, 0x40DC, [0, (ushort)(10 * _spindleParams.Dec)]).ConfigureAwait(false);//Pr2.21
        await _client.WriteMultipleRegistersAsync(1, 0x41FC, [0, _spindleParams.RatedVoltage]).ConfigureAwait(false);//Pr5.09
        await _client.WriteMultipleRegistersAsync(1, 0x41FA, [0, (ushort)(100 * _spindleParams.RatedCurrent)]).ConfigureAwait(false);//Pr5.07
    }
}
