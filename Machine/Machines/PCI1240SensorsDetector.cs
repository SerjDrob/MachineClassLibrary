using MachineClassLibrary.Classes;
using System.Collections.Generic;
using System.Linq;



namespace MachineClassLibrary.Machine.Machines
{
    public class PCI1240SensorsDetector : ISensorsDetector
    {
        private readonly Dictionary<(Ax,Di), LaserSensor> _sensors;

        private PCI1240SensorsDetector() { }
        private PCI1240SensorsDetector(Dictionary<(Ax,Di), LaserSensor> sensors)
        {
            _sensors = sensors;
        }
        public static SensorsDetectorBuilder GetSensorsDetectorBuilder() => new SensorsDetectorBuilder();
        public class SensorsDetectorBuilder
        {
            private Dictionary<(Ax,Di), LaserSensor> _sensors;

            public SensorsDetectorBuilder AddSensor(LaserSensor sensor, Ax ax, Di di)
            {
                _sensors ??= new();
                _sensors[(ax,di)] = sensor;
                return this;
            }
            public PCI1240SensorsDetector Build()
            {
                return new PCI1240SensorsDetector(_sensors);
            }
        }
        public (LaserSensor, bool)[] GetSensorState(Ax ax, int ins)
        {
            var arr = new List<(LaserSensor, bool)>();
            foreach (var item in _sensors.Keys.Where(k=>k.Item1 == ax))
            {
                var res = (ins & 1 << (int)item.Item2) != 0;
                var result = (_sensors[item], res);
                arr.Add(result);
            }
            return arr.ToArray();
        }
    }
}
