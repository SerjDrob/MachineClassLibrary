using System.Collections.Generic;
using System.Windows.Media;

namespace MachineClassLibrary.Classes
{
    public interface IGeometryAdapter
    {
        GeometryCollection Geometries { get; }

        IEnumerable<Geometry> GetGeometies();
        IEnumerable<AdaptedGeometry> GetGeometries();
    }
}
