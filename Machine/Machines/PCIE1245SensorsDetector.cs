using MachineClassLibrary.Classes;
using System.Collections.Generic;



namespace MachineClassLibrary.Machine.Machines
{
    public class PCIE1245SensorsDetector : ISensorsDetector
    {
        private readonly Dictionary<uint, LaserSensor> _sensors;

        private PCIE1245SensorsDetector() { }
        private PCIE1245SensorsDetector(Dictionary<uint, LaserSensor> sensors)
        {
            _sensors = sensors;
        }
        public static SensorsDetectorBuilder GetSensorsDetectorBuilder() => new SensorsDetectorBuilder();
        public class SensorsDetectorBuilder
        {
            private Dictionary<uint, LaserSensor> _sensors;

            public SensorsDetectorBuilder AddSensor(LaserSensor sensor, uint channel)
            {
                _sensors ??= new();
                _sensors[channel] = sensor;
                return this;
            }
            public PCIE1245SensorsDetector Build()
            {
                return new PCIE1245SensorsDetector(_sensors);
            }
        }
        public (LaserSensor, bool)[] GetSensorState(Ax ax, int ins)
        {
            var arr = new List<(LaserSensor, bool)>();
            foreach (var item in _sensors.Keys)
            {
                var res = (ins & 1 << (int)item) > 0;
                var result = (_sensors[item], res);
                arr.Add(result);
            }
            return arr.ToArray();
        }
    }
}
