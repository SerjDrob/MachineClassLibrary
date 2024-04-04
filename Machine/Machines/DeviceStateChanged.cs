namespace MachineClassLibrary.Machine.Machines
{
    public interface IDeviceStateChanged { };
    public record DeviceStateChanged():IDeviceStateChanged;
    public record SensorStateChanged(Sensors Sensor, bool state):IDeviceStateChanged;
}
