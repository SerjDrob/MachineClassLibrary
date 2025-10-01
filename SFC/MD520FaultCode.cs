using System.ComponentModel;


namespace MachineClassLibrary.SFC;

/// <summary>
/// Перечисление кодов ошибок частотного преобразователя MD520.
/// Соответствует значениям, возвращаемым регистром Modbus 8000H.
/// </summary>
/// <summary>
/// Перечисление кодов ошибок частотного преобразователя MD520.
/// Соответствует значениям, возвращаемым регистром Modbus 8000H.
/// </summary>
public enum MD520FaultCode:ushort
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
