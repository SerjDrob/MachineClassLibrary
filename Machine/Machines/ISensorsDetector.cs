using MachineClassLibrary.Classes;

namespace MachineClassLibrary.Machine.Machines
{
    public interface ISensorsDetector
    {
        (LaserSensor, bool)[] GetSensorState(Ax ax, int ins);
    }
}
