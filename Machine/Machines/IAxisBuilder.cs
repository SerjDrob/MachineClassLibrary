using MachineClassLibrary.Machine.MotionDevices;

namespace MachineClassLibrary.Machine.Machines
{
    public interface IAxisBuilder
    {
        void Build();
        PCI124XXMachine.AxisBuilder WithConfigs(MotionDeviceConfigs configs);
        PCI124XXMachine.AxisBuilder WithVelRegime(Velocity velocity, double value);
    }
}