using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MachineClassLibrary.Miscellaneous;
using Microsoft.Extensions.Logging;


namespace MachineClassLibrary.SFC;

//public class MD520Q : ISpindle, IDisposable//TODO create handling errors: overcurrent etc.
//{
//    /// <summary>
//    ///     85 Hz = 5100 rpm
//    /// </summary>
//    private const ushort LOW_FREQ_LIMIT = 850;

//    /// <summary>
//    ///     1000 Hz = 60000 rpm
//    /// </summary>
//    private const ushort HIGH_FREQ_LIMIT = 10000;

//    private const ushort READ_AC_DRIVE_STATE_1 = 0x3000;

//    private const ushort READ_OUTPUT_CURRENT = 0x1004;
//    private const ushort READ_RUNNING_FREQ = 0x1001;
//    private const ushort READ_AC_DRIVE_STATE_2 = 0x7044;
//    private const ushort READ_AC_DRIVE_FAULT = 0x8000;

//    private const ushort WRITE_CONTROL_COMMAND_AC_DRIVE_2 = 0x2000;
//    private const ushort WRITE_FREQ_REF_SET_2 = 0x7310;

//    private const ushort STATE_1_RUNNING_FORWARD = 0x0001;
//    private const ushort STATE_1_RUNNING_REVERSE = 0x0002;
//    private const ushort STATE_1_STOPPED = 0x0003;
//    private const ushort COMMAND_AC_DRIVE_2_RUN_FORWARD = 0x0001;
//    private const ushort COMMAND_AC_DRIVE_2_DEC_STOP = 0x0006;

//    private readonly object _modbusLock = new();
//    private ModbusSerialMaster _client;
//    private SerialPort _serialPort;
//    private CancellationTokenSource _watchingStateCancellationTokenSource;
//    private readonly string _com;
//    private readonly int _baudRate;
//    private readonly ILogger<MD520Q> _logger;

//    // TODO wait or cancel in the end, NEVER forget Tasks
//    private Task _watchingStateTask;
//    private bool _hasStarted = false;
//    private ushort _freq;
//    private bool _onFreq;

//    public MD520Q(string com, int baudRate, ILogger<MD520Q> logger)
//    {
//        _com = com;
//        _baudRate = baudRate;
//        _logger = logger;
//    }
//    private bool CheckSpindleWorking()
//    {
//        lock (_modbusLock)
//        {
//            if (_client == null)
//            {
//                _logger.LogWarning("Attempted to check spindle state, but Modbus client is not initialized.");
//                return false;
//            }
//            try
//            {
//                var data = _client.ReadHoldingRegisters(1, READ_AC_DRIVE_STATE_1, 1);//when timeout throw an exception
//                return data[0] == STATE_1_RUNNING_FORWARD | data[0] == STATE_1_RUNNING_REVERSE;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Failed to {READ_AC_DRIVE_STATE_1}");
//                return false;
//            }
//        }
//    }
//    public event EventHandler<SpindleEventArgs> GetSpindleState;
//    public bool IsConnected { get; set; } = false;
//    public void SetSpeedAsync(ushort rpm)
//    {
//        if (!(rpm / 6 > LOW_FREQ_LIMIT && rpm / 6 < HIGH_FREQ_LIMIT))
//        {
//            throw new SpindleException($"{rpm}rpm is out of ({LOW_FREQ_LIMIT * 6},{HIGH_FREQ_LIMIT * 6}) rpm range");
//        }
//        rpm = (ushort)Math.Abs(rpm / 6);
//        lock (_modbusLock)
//        {
//            try
//            {
//                _client.WriteSingleRegister(1, WRITE_FREQ_REF_SET_2, rpm);
//                _logger.LogInformation($"Successfully {WRITE_FREQ_REF_SET_2}: {rpm} rpm");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Failed to {WRITE_FREQ_REF_SET_2}");
//                throw;
//            }
//        }
//    }
//    public void StartAsync()
//    {
//        lock (_modbusLock)
//        {
//            try
//            {
//                _client.WriteSingleRegister(1, WRITE_CONTROL_COMMAND_AC_DRIVE_2, COMMAND_AC_DRIVE_2_RUN_FORWARD);
//                _logger.LogInformation($"Successfully {WRITE_CONTROL_COMMAND_AC_DRIVE_2}: {COMMAND_AC_DRIVE_2_RUN_FORWARD}");
//                _hasStarted = true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Failed to {WRITE_CONTROL_COMMAND_AC_DRIVE_2}:{COMMAND_AC_DRIVE_2_RUN_FORWARD}");
//                throw;
//            }
//        }
//    }
//    public void Stop()
//    {
//        lock (_modbusLock)
//        {
//            try
//            {
//                _client.WriteSingleRegister(1, WRITE_CONTROL_COMMAND_AC_DRIVE_2, COMMAND_AC_DRIVE_2_DEC_STOP);
//                _logger.LogInformation($"Successfully {WRITE_CONTROL_COMMAND_AC_DRIVE_2}: {COMMAND_AC_DRIVE_2_DEC_STOP}");
//                _hasStarted = false;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Failed to {WRITE_CONTROL_COMMAND_AC_DRIVE_2}:{COMMAND_AC_DRIVE_2_DEC_STOP}");
//                throw;
//            }
//        }
//    }
//    private bool EstablishConnection()
//    {
//        _serialPort = new SerialPort
//        {
//            PortName = _com,
//            BaudRate = _baudRate,
//            Parity = Parity.None,
//            WriteTimeout = 1000,
//            ReadTimeout = 100,
//            DataBits = 8,
//            StopBits = StopBits.One
//        };
//        _logger.LogInformation("Attempting to open serial port: {PortName}", _com);
//        _serialPort.Open();
//        if (_serialPort.IsOpen)
//        {
//            _client = ModbusSerialMaster.CreateRtu(_serialPort);
//            _logger.LogInformation("Serial port {PortName} opened successfully. Modbus client created.", _com);
//        }
//        else
//        {
//            _logger.LogWarning("Failed to open serial port: {PortName}", _com);
//            return false;
//        }

//        return IsConnected = true;
//    }

//    private async Task WatchingStateAsync(CancellationToken token)
//    {
//        ushort[] data = default;
//        ushort tempFault = 0;
//        await Task.Delay(100).ConfigureAwait(false);
//        while (!token.IsCancellationRequested)
//        {
//            try
//            {
//                SpindleEventArgs args;
//                _serialPort?.DiscardInBuffer();
//                args = await ReadStateGetArgsAsync().ConfigureAwait(false);
//                data = await _client.ReadHoldingRegistersAsync(1, READ_AC_DRIVE_FAULT, 1).ConfigureAwait(false);

//                if (Enum.IsDefined(typeof(MD520FaultCode), data[0]) && tempFault != data[0])
//                {
//                    MD520FaultCode fault = (MD520FaultCode)data[0];
//                    string faultDescription = fault.GetDescription();

//                    _logger.LogError(
//                        "Spindle fault detected! Code: {FaultCode}, Description: {Description}",
//                        fault,
//                        faultDescription
//                    );
//                    args.IsOk = false;
//                    args.FaultDescription = faultDescription;
//                    args.FaultCode = (int)fault;
//                }
//                tempFault = data[0];
//                GetSpindleState?.Invoke(this, args);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Failed {nameof(WatchingStateAsync)}");//TODO count how many frequent
//            }
//            await Task.Delay(100).ConfigureAwait(false);
//        }

//    }

//    private async Task<SpindleEventArgs> ReadStateGetArgsAsync()
//    {
//        ushort[] data;
//        bool acc;
//        bool dec;
//        bool stop;
//        int current;
//        var CheckData = (ushort data, int bitNum) => (data & 1 << bitNum) != 0;

//        data = await _client.ReadHoldingRegistersAsync(1, READ_AC_DRIVE_STATE_2, 1).ConfigureAwait(false);
//        _onFreq = CheckData(data[0], 3);

//        data = await _client.ReadHoldingRegistersAsync(1, READ_AC_DRIVE_STATE_1, 1).ConfigureAwait(false);
//        stop = data[0] == STATE_1_STOPPED;

//        data = await _client.ReadHoldingRegistersAsync(1, READ_OUTPUT_CURRENT, 1).ConfigureAwait(false);
//        current = data[0];

//        data = await _client.ReadHoldingRegistersAsync(1, READ_RUNNING_FREQ, 1).ConfigureAwait(false);
//        acc = _onFreq ? false : data[0] > _freq;
//        dec = _onFreq ? false : data[0] < _freq;
//        _freq = data[0];
//        return new SpindleEventArgs(_freq * 6, (double)current / 100, _onFreq, acc, dec, stop);
//    }

//    private async Task<SpindleEventArgs> ReadStateGetArgs2_Async()
//    {
//        ushort[] data;
//        bool acc;
//        bool dec;
//        bool stop;
//        int current;
//        var CheckData = (ushort data, int bitNum) => (data & 1 << bitNum) != 0;
//        data = await _client.ReadHoldingRegistersAsync(1, READ_AC_DRIVE_STATE_2, 1).ConfigureAwait(false);
//        _onFreq = CheckData(data[0],3);
//        acc = _onFreq ? false : data[0] > _freq;
//        dec = _onFreq ? false : data[0] < _freq;

//        stop = data[0] == STATE_1_STOPPED;

//        data = await _client.ReadHoldingRegistersAsync(1, READ_OUTPUT_CURRENT, 1).ConfigureAwait(false);
//        current = data[0];

//        data = await _client.ReadHoldingRegistersAsync(1, READ_RUNNING_FREQ, 1).ConfigureAwait(false);

//        _freq = data[0];
//        return new SpindleEventArgs(_freq * 6, (double)current / 100, _onFreq, acc, dec, stop);
//    }

//    private bool SetParams()
//    {
//        throw new NotImplementedException();
//    }

//    public void Dispose()
//    {
//        _watchingStateCancellationTokenSource?.Cancel();
//        try
//        {
//            _watchingStateTask?.Wait(1000);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogWarning(ex, "Spindle state monitoring task did not stop gracefully.");
//        }
//        _serialPort?.Dispose();
//        _client?.Dispose();
//        _watchingStateCancellationTokenSource?.Dispose();
//    }

//    public bool Connect()
//    {
//        if (EstablishConnection())
//        {
//            _watchingStateCancellationTokenSource = new CancellationTokenSource();
//            _watchingStateTask = WatchingStateAsync(_watchingStateCancellationTokenSource.Token);
//            if (CheckSpindleWorking())
//            {
//                return true;
//            }
//            //if (!SetParams()) throw new SpindleException("SetParams is failed");
//        }
//        else
//        {
//            return false;
//        }
//        return true;
//    }
//    /*
//    public async Task<bool> ChangeSpeedAsync(ushort rpm, int delay)
//    {
//        if (!_hasStarted) return false;
//        if (rpm == _freq * 6) return true;
//        try
//        {
//            SetSpeed(rpm);
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
//    */
//    public Task<bool> ChangeSpeedAsync(ushort rpm, int delay)
//    {
//        if (!_hasStarted) return Task.FromResult(false);
//        if (rpm == _freq * 6) return Task.FromResult(true);
//        try
//        {
//            SetSpeedAsync(rpm);
//            var cts = new CancellationTokenSource(delay);
//            var tcs = new TaskCompletionSource<bool>();

//            void ReachedFreq(object sender, SpindleEventArgs args)
//            {
//                if (args.OnFreq)
//                {
//                    CleanupAndSetResult(true);
//                }
//                else if (!args.IsOk || args.Stop)
//                {
//                    CleanupAndSetResult(false);
//                }
//            }

//            void CleanupAndSetResult(bool result)
//            {
//                GetSpindleState -= ReachedFreq;
//                cts.Dispose();
//                tcs.TrySetResult(result);
//            }

//            GetSpindleState += ReachedFreq;

//            cts.Token.Register(() =>
//            {
//                GetSpindleState -= ReachedFreq;
//                tcs.TrySetResult(false); 
//            });

//            return tcs.Task;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Exception in ChangeSpeedAsync");
//            return Task.FromResult(false);
//        }
//    }
//}

/// <summary>
/// Перечисление кодов ошибок частотного преобразователя MD520.
/// Соответствует значениям, возвращаемым регистром Modbus 8000H.
/// </summary>
/// <summary>
/// Перечисление кодов ошибок частотного преобразователя MD520.
/// Соответствует значениям, возвращаемым регистром Modbus 8000H.
/// </summary>
public enum MD520FaultCode
{
    /// <summary>
    /// Нет ошибки / Неизвестная ошибка. Значение по умолчанию.
    /// </summary>
    [Description("Нет ошибки")]
    None = 0,

    [Description("Перегрузка по току")]
    Overcurrent = 2,

    [Description("Перенапряжение")]
    Overvoltage = 5,

    [Description("Ошибка цепи предварительного заряда")]
    PreChargePowerFault = 8,

    [Description("Пониженное напряжение")]
    Undervoltage = 9,

    [Description("Перегрузка преобразователя")]
    DriveOverload = 10,

    [Description("Перегрузка двигателя")]
    MotorOverload = 11,

    [Description("Потеря фазы на входе")]
    InputPhaseLoss = 12,

    [Description("Потеря фазы на выходе")]
    OutputPhaseLoss = 13,

    [Description("Перегрев")]
    Overheat = 14,

    [Description("Внешняя ошибка")]
    ExternalFault = 15,

    [Description("Исключение цепи предварительного заряда")]
    PreChargeCircuitException = 17,

    [Description("Ошибка измерения тока")]
    CurrentSamplingException = 18,

    [Description("Ошибка автонастройки двигателя")]
    MotorAutoTuningException = 19,

    [Description("Ошибка энкодера/PG карты")]
    EncoderPGCardException = 20,

    [Description("Ошибка EEPROM")]
    EEPROMFault = 21,

    [Description("PG карта не активирована")]
    EncoderCardNotActivated = 22,

    [Description("Короткое замыкание на землю на выходе")]
    OutputShortToGround = 23,

    [Description("Достигнута накопленная продолжительность работы")]
    AccumulativeRunningDurationReach = 26,

    [Description("Пользовательская ошибка")]
    UserDefinedFault = 27,

    [Description("Пользовательское предупреждение")]
    UserDefinedAlarm = 28,

    [Description("Достигнута накопленная продолжительность включения")]
    AccumulativePowerOnDurationReach = 29,

    [Description("Потеря нагрузки на выходе")]
    OutputLoadLoss = 30,

    [Description("Потеря PID обратной связи во время работы")]
    PIDFeedbackLossDuringRunning = 31,

    [Description("Ошибка параметра")]
    ParameterException = 32,

    [Description("Ошибка ограничения тока импульс за импульсом")]
    PulseByPulseCurrentLimitFault = 40,

    [Description("Чрезмерное отклонение скорости")]
    ExcessiveSpeedDeviation = 42,

    [Description("Превышение скорости двигателя")]
    MotorOverspeed = 43,

    [Description("Перегрев двигателя")]
    MotorOvertemperature = 45,

    [Description("Ошибка STO")]
    STOFault = 47,

    [Description("Ошибка автонастройки положения полюсов")]
    PolePositionAutoTuningError = 51,

    [Description("Ошибка управления ведущий-ведомый")]
    MasterSlaveControlFault = 55,

    [Description("Ошибка самодиагностики 1")]
    SelfCheckFault1 = 56,

    [Description("Ошибка самодиагностики 2")]
    SelfCheckFault2 = 57,

    [Description("Ошибка самодиагностики 3")]
    SelfCheckFault3 = 58,

    [Description("Ошибка самодиагностики 4")]
    SelfCheckFault4 = 59,

    [Description("Перегрузка тормоза")]
    BrakingOverload = 61,

    [Description("Ошибка транзистора тормоза")]
    BrakingTransistorFault = 62,

    [Description("Внешнее предупреждение")]
    ExternalAlarm = 63,

    [Description("Ошибка контактора предварительного заряда")]
    PreChargeContactorFault = 82,

    [Description("Ошибка синхронизации")]
    TimingFault = 85,

    [Description("Исключение управления двигателем 1")]
    MotorControlException1 = 93,

    [Description("Исключение управления двигателем 2")]
    MotorControlException2 = 94,

    [Description("Ошибка автоматического сброса")]
    AutoResetFault = 159,

    [Description("Таймаут Modbus")]
    ModbusTimeout = 160,

    [Description("Ошибка CANopen")]
    CANopenFault = 161,

    [Description("Ошибка CANlink")]
    CANlinkFault = 162,

    [Description("Ошибка платы расширения")]
    ExpansionCardFault = 164,

    [Description("Защита от исключения на входе")]
    InputExceptionProtection = 174
}

public class MD520Q : SpindleBase<MD520Q>
{
    private const ushort READ_AC_DRIVE_STATE_1 = 0x3000;

    private const ushort READ_OUTPUT_CURRENT = 0x1004;
    private const ushort READ_RUNNING_FREQ = 0x1001;
    private const ushort READ_AC_DRIVE_STATE_2 = 0x7044;
    private const ushort READ_AC_DRIVE_FAULT = 0x8000;

    private const ushort WRITE_CONTROL_COMMAND_AC_DRIVE_2 = 0x2000;
    private const ushort WRITE_FREQ_REF_SET_2 = 0x7310;

    private const ushort STATE_1_RUNNING_FORWARD = 0x0001;
    private const ushort STATE_1_RUNNING_REVERSE = 0x0002;
    private const ushort STATE_1_STOPPED = 0x0003;
    private const ushort COMMAND_AC_DRIVE_2_RUN_FORWARD = 0x0001;
    private const ushort COMMAND_AC_DRIVE_2_DEC_STOP = 0x0006;
    public MD520Q(SerialPortSettings serialPortSettings, ILogger<MD520Q> logger) : base(serialPortSettings, logger)
    {
    }

    protected override async Task<bool> CheckIfSpindleSpinningAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, READ_AC_DRIVE_STATE_1, 1).ConfigureAwait(false);//when timeout throw an exception
        return data[0] == STATE_1_RUNNING_FORWARD | data[0] == STATE_1_RUNNING_REVERSE;
    }

    protected override async Task<int> GetCurrentAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, READ_OUTPUT_CURRENT, 1).ConfigureAwait(false);
        return data[0];
    }

    protected override async Task<int> GetFrequencyAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, READ_RUNNING_FREQ, 1).ConfigureAwait(false);
        return data[0];
    }

    protected override async Task<SpinStatus> GetStatusAsync()
    {
        ushort[] data;
        bool acc;
        bool dec;
        bool stop;
        int current;
        var CheckData = (ushort data, int bitNum) => (data & 1 << bitNum) != 0;
        data = await _client.ReadHoldingRegistersAsync(1, READ_AC_DRIVE_STATE_2, 1).ConfigureAwait(false);
        var onFreq = CheckData(data[0], 3);
        acc = onFreq ? false : data[0] > _freq;
        dec = onFreq ? false : data[0] < _freq;
        stop = data[0] == STATE_1_STOPPED;
        data = await _client.ReadHoldingRegistersAsync(1, READ_OUTPUT_CURRENT, 1).ConfigureAwait(false);
        current = data[0];

        return new SpinStatus(onFreq, acc, dec, stop);
    }

    protected override async Task StartFWDCommandAsync() => await _client.WriteSingleRegisterAsync(1, WRITE_CONTROL_COMMAND_AC_DRIVE_2, COMMAND_AC_DRIVE_2_RUN_FORWARD).ConfigureAwait(false);

    protected override async Task StopCommandAsync() => await _client.WriteSingleRegisterAsync(1, WRITE_CONTROL_COMMAND_AC_DRIVE_2, COMMAND_AC_DRIVE_2_DEC_STOP).ConfigureAwait(false);

    protected override async Task WriteRPMAsync(ushort rpm) => await _client.WriteSingleRegisterAsync(1, WRITE_FREQ_REF_SET_2, rpm).ConfigureAwait(false);

    protected override Task WriteSettingsAsync() => Task.CompletedTask;

    protected override async Task<SpinFault> GetSpinFaultAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, READ_AC_DRIVE_FAULT, 1).ConfigureAwait(false);

        if (Enum.IsDefined(typeof(MD520FaultCode), data[0]) && _tempFault != data[0])
        {
            _tempFault = data[0];
            var fault = (MD520FaultCode)_tempFault;
            if (fault == MD520FaultCode.None) return await base.GetSpinFaultAsync().ConfigureAwait(false);
            string faultDescription = fault.GetDescription();

            _logger.LogError(
                "Spindle fault detected! Code: {FaultCode}, Description: {Description}",
                fault,
                faultDescription
            );

            var spinFault = new SpinFault(
            false,
            (int)fault,
            faultDescription);

            return spinFault;
        }
        else
        {
            return await base.GetSpinFaultAsync().ConfigureAwait(false);
        }
    }
}
