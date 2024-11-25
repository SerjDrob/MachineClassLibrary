using Advantech.Motion;

namespace MachineClassLibrary.Machine.MotionDevices
{
    struct Axis
    {
        public Axis(uint id):this()
        {
            ID = id;
        }
        public readonly uint ID;
        public SPEED_PROFILE_PRM AxVel;
        public void SetSpeedProfile(SPEED_PROFILE_PRM profile) => AxVel = profile;
        public void ChangeSpeed(double low, double high)
        {
            AxVel.FL = low;
            AxVel.FH = high;
        }
    }
}
