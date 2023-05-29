using System;

namespace MachineClassLibrary.SFC
{
    public class SpindleEventArgs : EventArgs
    {
        public SpindleEventArgs()
        {
            
        }
        public SpindleEventArgs(int rpm, double current, bool onFreq, bool accelerating, bool deccelarating, bool stop)
        {
            Rpm = rpm;
            Current = current;
            OnFreq = onFreq;
            Accelerating = accelerating;
            Deccelarating = deccelarating;
            Stop = stop;
        }

        public int Rpm { get; init; } //= 0;
        public double Current { get; init; }// = 0;
        public bool OnFreq { get; init; }// = false;
        public bool Accelerating { get; init; }// = false;
        public bool Deccelarating { get; init; }// = false;
        public bool Stop { get; init; } //= true;
        public bool IsOk { get; set; }
    }
}