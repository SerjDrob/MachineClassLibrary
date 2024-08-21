using System;
using System.Collections.Generic;
using MachineClassLibrary.Classes;
using MachineClassLibrary.Machine.MotionDevices;



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

        public bool GetValveState(IMotionDevicePCI1240U motionDevice, Valves valve, Func<Ax, int> getAxNum)
        {
            if (_valves.TryGetValue(valve, out var ch))
            {
                return motionDevice.GetAxisDout(0, (ushort)ch);
            }
            throw new ArgumentException($"The valve {nameof(valve)} is not assigned");
        }
    }
}
