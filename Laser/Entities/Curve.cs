using System.CodeDom;
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
            //var minX = vertices.Min(x => x.X);
            //var minY = vertices.Min(y => y.Y);
            //var maxX = vertices.Max(x => x.X);
            //var maxY = vertices.Max(y => y.Y);
            //Bounds = new Rect(new System.Windows.Point { X = minX, Y = minY }, new System.Windows.Point { X = maxX, Y = maxY });
        }

        public IEnumerable<(double X, double Y, double Bulge)> Vertices { get; init; }
        public bool IsClosed { get; init; }
        public Rect Bounds { get; init; }

        public void Deconstruct(out IShape[] primaryShapes, out int num)
        {
            primaryShapes = new Curve[] { this };
            num = 1;
        }

        public static bool operator == (Curve left, Curve right) => left.Bounds.Width == right.Bounds.Width && left.Bounds.Height == right.Bounds.Height;
        public static bool operator != (Curve left, Curve right) => !(left == right);

    }
}
