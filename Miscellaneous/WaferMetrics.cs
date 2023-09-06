using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MachineClassLibrary.Laser.Entities;

namespace MachineClassLibrary.Miscellaneous
{
    public class WaferMetrics
    {
        private readonly IEnumerable<IProcObject> _wafer;

        public WaferMetrics(IEnumerable<IProcObject> procObjects)
        {
            _wafer = procObjects;
        }
        public void CreateProcessingChain()
        {
            var vx = 1;
            var vy = 2;
            var result = new Dictionary<Guid, double>();
            IProcObject tempElement = _wafer.First();

            foreach (var element in _wafer)
            {
                result[element.Id] = Time(tempElement, element, vx, vy);
                tempElement = element;
            }

            var result2 = _wafer.GroupBy(o => o, p => p.Id, new ProcObjectByPObjEqComparer())
              .ToLookup(g => g.Key.Id);


            double Time(IProcObject first, IProcObject second, double velx, double vely) =>
                Math.Max(Math.Abs(second.X - first.X) / velx, Math.Abs(second.Y - first.Y) / vely);
        }
    }
    internal static class ProcObjectExtensions
    {

    }

    internal class MultiKeyDictionary<K, E>
    {
        private readonly Dictionary<K, E> _mainElements = new();
        private readonly IEnumerable<IGrouping<K, K>> _keyGroups;

        public MultiKeyDictionary(IEnumerable<IGrouping<K, K>> keyGroups)
        {
            _keyGroups = keyGroups;
        }

        public E this[K someKey]
        {
            get
            {
                var key = GetKey(someKey);
                return _mainElements[key];
            }
            set
            {
                var key = GetKey(someKey);
                _mainElements[key] = value;
            }
        }

        private K GetKey(K someKey) => _keyGroups.Single(g => g.Any(e => e.Equals(someKey))).Key;
    }

    public class ProcObjectByPObjEqComparer : EqualityComparer<IProcObject>
    {
        public override bool Equals(IProcObject x, IProcObject y)
        {
            if (x == null || y == null || x.GetType() != y.GetType())
            {
                return false;
            }

            return x switch
            {
                PCircle circle1 when y is PCircle circle2 => circle1.PObject == circle2.PObject,
                PCurve curve1 when y is PCurve curve2 => curve1.PObject == curve2.PObject,
                _ => false
            };
        }
        public override int GetHashCode([DisallowNull] IProcObject obj) => GetHashCode();
    }
}