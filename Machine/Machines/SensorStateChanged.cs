namespace MachineClassLibrary.Machine.Machines
{
    public record SensorStateChanged(LaserSensor Sensor, bool state):IDeviceStateChanged;
}
