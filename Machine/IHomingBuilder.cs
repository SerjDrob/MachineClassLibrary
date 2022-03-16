using MachineClassLibrary.Machine.MotionDevices;

namespace MachineClassLibrary.Machine
{
    public interface IHomingBuilder
    {
        void Configure();
        IHomingBuilder SetHomingDirection(AxDir direction);
        IHomingBuilder SetHomingMode(HmMode hmMode);
        IHomingBuilder SetHomingReset(HomeRst homeRst);
        IHomingBuilder SetHomingVelocity(double velocity);
        IHomingBuilder SetPositionAfterHoming(double position);
    }
}