using System;

namespace MachineClassLibrary.Machine
{
    public class ValveEventArgs : EventArgs
    {
        public ValveEventArgs(Valves valve, bool state)
        {
            Valve = valve;
            State = state;
        }

        public Valves Valve { get; init; }
        public bool State { get; init; }
    }
}