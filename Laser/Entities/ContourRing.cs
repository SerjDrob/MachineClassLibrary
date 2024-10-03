using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MachineClassLibrary.Laser.Entities
{
    public class ContourRing : IShape
    {
        public IEnumerable<Curve> Curves { get; set; }
        public Rect Bounds => Curves.Aggregate(new Rect(),(acc, cur) =>
        {
            acc.Union(cur.Bounds);
            return acc;
        });

        public void Deconstruct(out IShape[] primaryShape, out int num)
        {
            primaryShape = Curves.ToArray();
            num = Curves.Count();
        }
    }
}
