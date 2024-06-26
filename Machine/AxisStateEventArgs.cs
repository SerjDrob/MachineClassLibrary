﻿using MachineClassLibrary.Classes;
using System;

namespace MachineClassLibrary.Machine
{
    public class AxisStateEventArgs : EventArgs
    {
        public AxisStateEventArgs(Ax axis, double position, double cmdPosition, bool nLmt, bool pLmt, bool motionDone, bool motionStart, bool eZ, bool org)
        {
            Axis = axis;
            Position = position;
            CmdPosition = cmdPosition;
            NLmt = nLmt;
            PLmt = pLmt;
            MotionDone = motionDone;
            MotionStart = motionStart;
            EZ = eZ;
            ORG = org;
        }

        public Ax Axis { get; init; }
        public double Position { get; init; }
        public double CmdPosition { get; set; }
        public bool NLmt { get; init; }
        public bool PLmt { get; init; }
        public bool MotionDone { get; init; }
        public bool MotionStart { get; init; }
        public bool EZ { get; init; }
        public bool ORG { get; init; }
    }
}
