using MachineClassLibrary.Classes;
using System;
using System.Collections.Generic;

namespace MachineClassLibrary.Machine
{
    public interface IHasValves
    {        
        public event EventHandler<ValveEventArgs> OnValveStateChanged;
        public void ConfigureValves(Dictionary<Valves, (Ax, Do)> valves);
        public void SwitchOnValve(Valves valve);
        public void SwitchOffValve(Valves valve);
        public bool GetValveState(Valves valve);
    }
}