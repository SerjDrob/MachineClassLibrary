namespace MachineClassLibrary.Machine
{
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
    }
}