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

    public struct MockAxisState
    {
        public double cmdPos { get; set; }
        public double actPos { get; set; }
        public int sensors { get; set; }
        public int outs { get; set; }
        public bool pLmt { get; set; }
        public bool nLmt { get; set; }
        public bool motionDone { get; set; }
        public bool homeDone { get; set; }
        public bool vhStart { get; set; }
        public bool vhEnd { get; set; }
        public AxisState GetAxisState => new AxisState(cmdPos, actPos, sensors, outs, pLmt, nLmt, motionDone, homeDone, vhStart,vhEnd);
    }
}
