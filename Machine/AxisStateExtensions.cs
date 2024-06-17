namespace MachineClassLibrary.Machine
{
    public static class AxisStateExtensions
    {
        public static AxisState AlterVHStart(this AxisState state, bool newVal) =>  new AxisState
        (
            state.cmdPos,
            state.actPos,
            state.sensors,
            state.outs,
            state.pLmt,
            state.nLmt,
            state.motionDone,
            state.homeDone,
            newVal,
            state.vhEnd,
            state.ez,
            state.org
        );
        public static AxisState AlterVHEnd(this AxisState state, bool newVal) => new AxisState
        (
            state.cmdPos,
            state.actPos,
            state.sensors,
            state.outs,
            state.pLmt,
            state.nLmt,
            state.motionDone,
            state.homeDone,
            state.vhStart,
            newVal,
            state.ez, 
            state.org
        );
        public static AxisState AlterMotDone(this AxisState state, bool newVal)
        {
            return new AxisState
        (
            state.cmdPos,
            state.actPos,
            state.sensors,
            state.outs,
            state.pLmt,
            state.nLmt,
            newVal,
            state.homeDone,
            state.vhStart,
            state.vhEnd,
            state.ez,
            state.org
        );
        }
    }
}
