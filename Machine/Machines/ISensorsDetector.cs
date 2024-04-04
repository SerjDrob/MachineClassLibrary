using MachineClassLibrary.Classes;

namespace MachineClassLibrary.Machine.Machines
{
    public interface ISensorsDetector
    {
        (Sensors, bool)[] GetSensorState(Ax ax, int ins);
    }
}