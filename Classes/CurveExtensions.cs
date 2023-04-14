using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
using MachineClassLibrary.Laser.Entities;
using netDxf.Entities;

namespace MachineClassLibrary.Classes;
internal static class CurveExtensions
{
    static IEnumerable<IEnumerable<PointD>> InflatePath(IEnumerable<double> path, double delta)
    {
        var subj = new PathsD();
        var mainPath = Clipper.MakePath(path.ToArray());
        subj.Add(mainPath);
        var solution = Clipper.InflatePaths(subj, delta, JoinType.Round, EndType.Polygon);
        var result = solution.Select(x => x.Select(point => point));
        return result;
    }

    public static IEnumerable<Curve> InflateCurve(this Curve curve, double delta)
    {
        var lwVertices = curve.Vertices.Select(v => new LwPolylineVertex(v.X, v.Y, v.Bulge));
        var lwPolyline = new LwPolyline(lwVertices);
        var curvePath = lwPolyline.PolygonalVertexes(0, 0.001, 0.001).Aggregate(new List<double>(), (acc, prev) =>
        {
            acc.Add(prev.X);
            acc.Add(prev.Y);
            return acc;
        }, acc => acc.ToArray()); 

        var paths = InflatePath(curvePath, delta);
        var curves = paths.Select(x => x.Select(point => (point.x, point.y, 0d)))
            .Select(vert => new Curve(vert, true));
        return curves;
    }
}


