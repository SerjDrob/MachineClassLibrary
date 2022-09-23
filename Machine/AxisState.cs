namespace MachineClassLibrary.Machine
{
#if NOTTEST
    public struct AxisState
    {
        public double cmdPos;
        public double actPos;
        public int sensors;
        public int outs;
        public bool pLmt;
        public bool nLmt;
        public bool motionDone;
        public bool homeDone;
        public bool vhStart;
        public bool vhEnd;
    }
#endif
    readonly public struct AxisState
    {
        public AxisState(double cmdPos, double actPos, int sensors, int outs, bool pLmt, bool nLmt, bool motionDone, bool homeDone, bool vhStart, bool vhEnd)
        {
            this.cmdPos = cmdPos;
            this.actPos = actPos;
            this.sensors = sensors;
            this.outs = outs;
            this.pLmt = pLmt;
            this.nLmt = nLmt;
            this.motionDone = motionDone;
            this.homeDone = homeDone;
            this.vhStart = vhStart;
            this.vhEnd = vhEnd;
        }

        public double cmdPos { get; }
        public double actPos { get; }
        public int sensors { get; }
        public int outs { get; }
        public bool pLmt { get; }
        public bool nLmt { get; }
        public bool motionDone { get; }
        public bool homeDone { get; }
        public bool vhStart { get; }
        public bool vhEnd { get; }
    }
}