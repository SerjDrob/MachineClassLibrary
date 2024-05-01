using System;

namespace MachineClassLibrary.Machine
{
    [Flags]
    public enum Sensors
    {
        Air = 2,
        ChuckVacuum = 4,
        Coolant = 8,
        SpindleCoolant = 16
    }
}
