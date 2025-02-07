using MachineClassLibrary.Classes;

namespace MachineClassLibrary.Machine
{
    public interface ISensorBuilder
    {
        void Configure();
        IValveBuilder AddSensor(Sensors sensor, Ax axis, Di di, bool inverted);
    }
}
