using System;

namespace MachineClassLibrary.Machine
{
    public class SensorsEventArgs : EventArgs
    {
        public SensorsEventArgs(Sensors sensor, bool state)
        {
            Sensor = sensor;
            State = state;
        }

        public Sensors Sensor { get; init; }
        public bool State { get; init; }
    }
}