using System;
using System.IO.Ports;
using System.Threading.Tasks;
using MachineClassLibrary.SFC;
using NModbus;
using NModbus.Serial;


namespace MachineClassLibrary.SFC;

public class MD520Q : ISpindle
{
    private IModbusSerialMaster _modbusMaster;
    private SerialPort _serialPort;
    private const byte DeviceAddress = 1; // Modbus address of the spindle drive
    private bool _isDisposed = false;

    public event EventHandler<SpindleEventArgs> GetSpindleState;

    public bool IsConnected { get; set; }

    public void Connect()
    {
        if (_serialPort == null)
        {
            _serialPort = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One);
            _serialPort.Open();

            var factory = new ModbusFactory();
            _modbusMaster = factory.CreateRtuMaster(_serialPort);
        }

        IsConnected = true;
    }

    public void SetSpeed(ushort rpm)
    {
        if (!IsConnected) throw new InvalidOperationException("Not connected to the spindle.");

        // Assuming 7310H is the address to set the frequency in Hz.
        // Convert RPM to frequency in Hz (this may need adjustment based on the motor specs)
        double frequencyHz = rpm / 60.0; // Example conversion, adjust as necessary
        ushort frequencyValue = (ushort)(frequencyHz * 100); // Assuming 2 decimal places

        _modbusMaster.WriteSingleRegister(DeviceAddress, 0x7310, frequencyValue);
    }

    public void Start()
    {
        if (!IsConnected) throw new InvalidOperationException("Not connected to the spindle.");

        // 7311H is used to start the motor
        _modbusMaster.WriteSingleRegister(DeviceAddress, 0x7311, 1); // 1 for forward direction run
    }

    public void Stop()
    {
        if (!IsConnected) throw new InvalidOperationException("Not connected to the spindle.");

        // 7311H is used to stop the motor
        _modbusMaster.WriteSingleRegister(DeviceAddress, 0x7311, 0); // 0 for stop
    }

    public async Task<bool> ChangeSpeedAsync(ushort rpm, int delay)
    {
        SetSpeed(rpm);
        await Task.Delay(delay);
        return true;
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _serialPort?.Close();
            _serialPort?.Dispose();
            _modbusMaster?.Dispose();
            _isDisposed = true;
        }
    }

    private void UpdateSpindleState()
    {
        if (!IsConnected) return;

        // Read state from Modbus registers (e.g., 3000H for state, 1004H for current)
        ushort[] stateData = _modbusMaster.ReadHoldingRegisters(DeviceAddress, 0x3000, 1);
        ushort[] currentData = _modbusMaster.ReadHoldingRegisters(DeviceAddress, 0x1004, 1);

        double spinCurrent = currentData[0] / 100.0; // Example conversion, adjust as needed
        double spindleFreq = stateData[0]; // Example, depending on what 3000H returns

        var state = new SpindleEventArgs();


        //GetSpindleState?.Invoke(this, new SpindleEventArgs
        //{
        //    IsConnected = IsConnected,
        //    SpinCurrent = spinCurrent,
        //    SpindleFreq = spindleFreq
        //});
    }
}
