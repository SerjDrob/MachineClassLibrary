using System.Collections.Generic;
using System.Windows;

namespace MachineClassLibrary.Laser.Entities
{
    public class ContourRing : IShape
    {
        public IEnumerable<Curve> Curves { get; set; }
        public Rect Bounds { get; init; }
    }
}
