using MachineClassLibrary.Classes;
using System;

namespace MachineClassLibrary.Machine
{
    public class AxisStateEventArgs : EventArgs
    {
        public AxisStateEventArgs(Ax axis, double position, double cmdPosition, bool nLmt, bool pLmt, bool motionDone, bool motionStart)
        {
            Axis = axis;
            Position = position;
            CmdPosition = cmdPosition;
            NLmt = nLmt;
            PLmt = pLmt;
            MotionDone = motionDone;
            MotionStart = motionStart;
        }

        public Ax Axis { get; init; }
        public double Position { get; init; }
        public double CmdPosition { get; set; }
        public bool NLmt { get; init; }
        public bool PLmt { get; init; }
        public bool MotionDone { get; init; }
        public bool MotionStart { get; init; }
    }
}