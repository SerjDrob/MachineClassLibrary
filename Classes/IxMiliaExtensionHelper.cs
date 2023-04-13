using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MachineClassLibrary.Classes
{
    static class IxMiliaExtensionHelper
    {
        public static (double x, double y) GetPolylineCenter(this IList<DxfLwPolylineVertex> vertices)
        {
            var xmax = vertices.Max(vertex => vertex.X);
            var ymax = vertices.Max(vertex => vertex.Y);
            var xmin = vertices.Min(vertex => vertex.X);
            var ymin = vertices.Min(vertex => vertex.Y);
            return ((xmax + xmin) / 2, (ymax + ymin) / 2);
        }

        public static Rect ToRect(this DxfBoundingBox? box)
        {
            return new Rect(box?.MinimumPoint.X ?? 0d, box?.MinimumPoint.Y ?? 0d, box?.Size.X ?? 0d, box?.Size.Y ?? 0d);
        }
    }

}
