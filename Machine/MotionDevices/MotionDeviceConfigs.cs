
using Advantech.Motion;

namespace MachineClassLibrary.Machine.MotionDevices
{
    public struct MotionDeviceConfigs
    {
        public double maxAcc;
        public double maxDec;
        public double maxVel;
        public int axDirLogic; // (int)DirLogic.DIR_ACT_HIGH;
        public int plsInLogic; // (int)PulseInLogic.NOT_SUPPORT;
        public int plsInMde; // (int)PulseInMode.AB_4X;
        public int plsInSrc; // (int)PulseInSource.NOT_SUPPORT;
        public int plsOutMde; // (int)PulseOutMode.OUT_DIR;
        public int reset; // (int)HomeReset.HOME_RESET_EN;
        public uint hLmtLogic;// (uint) HLmtLogic.HLMT_ACT_LOW
        public double acc;
        public double dec;
        public int jerk;
        public int ppu;
        public uint cmpEna; // (uint)CmpEnable.CMP_EN;
        public uint cmpMethod; // (uint)CmpMethod.MTD_GREATER_POSITION;
        public uint cmpSrcAct; // (uint)CmpSource.SRC_ACTUAL_POSITION;
        public uint cmpSrcCmd; // (uint)CmpSource.SRC_COMMAND_POSITION;
        public double homeVelLow;
        public double homeVelHigh;
        public int denominator;
        public double ratio;
        public double lineDiscrete;
        //public MotionDeviceConfigs()
        //{
        //}
    }
}
