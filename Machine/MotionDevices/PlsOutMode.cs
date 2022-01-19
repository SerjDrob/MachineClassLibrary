using Advantech.Motion;

namespace MachineClassLibrary.Machine.MotionDevices
{
    public enum PlsOutMode
    {
        OUT_DIR = (int)PulseOutMode.OUT_DIR,
        OUT_DIR_OUT_NEG = (int)PulseOutMode.OUT_DIR_OUT_NEG,
        OUT_DIR_DIR_NEG = (int)PulseOutMode.OUT_DIR_DIR_NEG,
        OUT_DIR_ALL_NEG = (int)PulseOutMode.OUT_DIR_ALL_NEG,
        O_CW_CCW = (int)PulseOutMode.O_CW_CCW,
        CW_CCW_ALL_NEG = (int)PulseOutMode.CW_CCW_ALL_NEG,
        AB_PHASE = (int)PulseOutMode.AB_PHASE,
        BA_PHASE = (int)PulseOutMode.BA_PHASE,
        CW_CCW_OUT_NEG = (int)PulseOutMode.CW_CCW_OUT_NEG,
        CW_CCW_DIR_NEG = (int)PulseOutMode.CW_CCW_DIR_NEG,
        NOT_SUPPORT = (int)PulseOutMode.NOT_SUPPORT
    }
}
