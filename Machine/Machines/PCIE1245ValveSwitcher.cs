using MachineClassLibrary.Classes;
using MachineClassLibrary.Machine.MotionDevices;
using System;
using System.Collections.Generic;



namespace MachineClassLibrary.Machine.Machines
{
    public class PCIE1245ValveSwitcher : IValveSwitcher
    {
        private readonly Dictionary<Valves, uint> _valves;

        private PCIE1245ValveSwitcher() { }
        private PCIE1245ValveSwitcher(Dictionary<Valves, uint> valves)
        {
            _valves = valves;
        }
        public static SwitcherBuilder GetSwitcherBuilder() => new SwitcherBuilder();
        public class SwitcherBuilder
        {
            private Dictionary<Valves, uint> _valves;

            public SwitcherBuilder AddValve(Valves valve, uint channel)
            {
                _valves ??= new();
                _valves[valve] = channel;
                return this;
            }
            public PCIE1245ValveSwitcher Build()
            {
                return new PCIE1245ValveSwitcher(_valves);
            }
        }
        public void SwitchValve(IMotionDevicePCI1240U motionDevice, Valves valve, bool val, Func<Ax, int> getAxNum)
        {
            if (_valves.TryGetValue(valve, out var axDo))
            {
                motionDevice.SetAxisDout(0, (ushort)axDo, val);
            }
        }
    }
}
