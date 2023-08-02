using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MachineClassLibrary.Laser.Entities
{
    public class Curve : IShape
    {
        public Curve(IEnumerable<(double X, double Y, double Bulge)> vertices, bool isClosed)
        {
            Vertices = vertices;
            IsClosed = isClosed;
        }

        public IEnumerable<(double X, double Y, double Bulge)> Vertices { get; init; }
        public bool IsClosed { get; init; }

        public Rect Bounds { get; init; }

    }
}
