using System;

namespace MachineClassLibrary.Machine
{
    [Flags]
    public enum Sensors
    {
        ChuckVacuum = 4,
        Air = 2,
        Coolant = 8,
        SpindleCoolant = 16
    }
}