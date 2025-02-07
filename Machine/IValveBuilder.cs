using MachineClassLibrary.Classes;

namespace MachineClassLibrary.Machine
{
    public interface IValveBuilder
    {
        void Configure();
        IValveBuilder AddValve(Valves valve, Ax axis, Do @do);
    }
}
