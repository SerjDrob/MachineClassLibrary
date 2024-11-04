using MachineClassLibrary.Machine;

namespace MachineClassLibrary.Machine
{
    public record AxisStateView(double Position, double CmdPosition, bool NLmt, bool PLmt, bool MotionDone, bool MotionStart, bool EZ, bool ORG);
}
