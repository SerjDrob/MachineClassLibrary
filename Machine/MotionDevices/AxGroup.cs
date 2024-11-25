using Advantech.Motion;

namespace MachineClassLibrary.Machine.MotionDevices
{
    struct AxGroup
    {
        public AxGroup(uint id, uint[] axes):this()
        {
            ID = id;
            AxesID = axes;
        }

        public readonly uint ID;
        public readonly uint[] AxesID;
        public SPEED_PROFILE_PRM GpVel;
        public void SetSpeedProfile(SPEED_PROFILE_PRM profile) => GpVel = profile;
        public void ChangeSpeed(double low, double high)
        {
            GpVel.FL = low;
            GpVel.FH = high;
        }
    }

}

