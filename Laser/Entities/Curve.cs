using System.Collections.Generic;
using System.Linq;

namespace MachineClassLibrary.Laser.Entities
{
    public class Curve : IShape
    {
        public IEnumerable<(double X, double Y, double Bulge)> Vertices { get; set; }

        public void Scale(double scale)
        {
            Vertices = Vertices.Select(vertex=> (vertex.X * scale, vertex.Y * scale, vertex.Bulge));
        }
    }
}
