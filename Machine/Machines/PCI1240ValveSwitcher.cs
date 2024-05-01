using MachineClassLibrary.Classes;
using MachineClassLibrary.Machine.MotionDevices;
using System;
using System.Collections.Generic;



namespace MachineClassLibrary.Machine.Machines
{
    public class PCI1240ValveSwitcher : IValveSwitcher
    {
        private readonly Dictionary<Valves, (Ax, Do)> _valves;

        private PCI1240ValveSwitcher( ) { }
        private PCI1240ValveSwitcher(Dictionary<Valves, (Ax, Do)> valves)
        {
            _valves = valves;
        }
        public static SwitcherBuilder GetSwitcherBuilder() => new SwitcherBuilder();
        public class SwitcherBuilder
        {
            private Dictionary<Valves, (Ax axis,Do dout)> _valves;

            public SwitcherBuilder AddValve(Valves valve, Ax ax, Do @do) 
            {
                _valves ??= new();
                _valves[valve] = (ax, @do);
                return this;    
            }
            public PCI1240ValveSwitcher Build()
            {
                return new PCI1240ValveSwitcher(_valves);
            }
        }
        public void SwitchValve(IMotionDevicePCI1240U motionDevice, Valves valve, bool val, Func<Ax, int> getAxNum)
        {
            if (_valves.TryGetValue(valve, out var axDo))
            {
                motionDevice.SetAxisDout(getAxNum?.Invoke(axDo.Item1) ?? throw new ArgumentException($"{nameof(getAxNum)} is not assigned"), (ushort)axDo.Item2, val);
            }
        }
        public bool GetValveState(IMotionDevicePCI1240U motionDevice, Valves valve, Func<Ax, int> getAxNum)
        {
            if (_valves.TryGetValue(valve, out var val))
            {
                return motionDevice.GetAxisDout(getAxNum?.Invoke(val.Item1) ?? throw new ArgumentException($"The valve {nameof(getAxNum)} is not assigned"), (ushort)val.Item2); 
            }
            throw new ArgumentException($"The valve {nameof(valve)} is not assigned");
        }
    }
}
