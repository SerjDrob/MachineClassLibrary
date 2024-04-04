namespace MachineClassLibrary.Machine.Machines
{
    public interface IDeviceStateChanged { };
    public record DeviceStateChanged():IDeviceStateChanged;
    public record SensorStateChanged(LaserSensor Sensor, bool state):IDeviceStateChanged;
}
