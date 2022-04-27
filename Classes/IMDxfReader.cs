using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
using MachineClassLibrary.Laser.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


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
    }
    public class IMDxfReader : IDxfReader
    {
        private const string TEMP_FILE_NAME = "tempcurve";
        private readonly string _fileName;
        private readonly DxfFile _document;
        private List<PDxfCurve> _tempDxfCurves = new();
        public IMDxfReader(string fileName)
        {
            _fileName = fileName;
            _document = DxfFile.Load(_fileName);
        }
        public IEnumerable<PCircle> GetCircles()
        {
            return _document.Entities.OfType<DxfCircle>()
                .Select(circle => new PCircle(circle.Center.X, circle.Center.Y, 0, new Circle() { Radius = circle.Radius }, circle.Layer, circle.Color.ToRGB()));
        }

        public IEnumerable<PLine> GetLines()
        {
            return _document.Entities.OfType<DxfLine>()
                .Select(line =>
                new PLine(line.P2.X - line.P1.X, line.P2.Y - line.P1.Y, 0, new Line()
                {
                    X1 = line.P1.X,
                    Y1 = line.P1.Y,
                    X2 = line.P2.X,
                    Y2 = line.P2.Y
                }, line.Layer, line.Color.ToRGB())
            );
        }
        public IEnumerable<PLine> GetAllSegments()
        {
            return _document.Entities.OfType<DxfLwPolyline>()
                 .Select(
                     polyline => polyline.AsSimpleEntities()
                     .OfType<DxfLine>().Select(dxfLine =>
                     new PLine(dxfLine.P2.X - dxfLine.P1.X, dxfLine.P2.Y - dxfLine.P1.Y, 0, new Line()
                     {
                         X1 = dxfLine.P1.X,
                         Y1 = dxfLine.P1.Y,
                         X2 = dxfLine.P2.X,
                         Y2 = dxfLine.P2.Y
                     }, polyline.Layer, dxfLine.Color.ToRGB())
                 )).SelectMany(x => x);
        }
        public IEnumerable<PCurve> GetAllCurves()
        {
            return _document.Entities.OfType<DxfLwPolyline>()
                .Select(polyline =>
                new PCurve(polyline.Vertices.GetPolylineCenter().x, polyline.Vertices.GetPolylineCenter().y, 0,
                new Curve { Vertices = polyline.Vertices.Select(vertex => (vertex.X, vertex.Y, vertex.Bulge)) },
                polyline.Layer, polyline.Color.ToRGB()));
        }
        /// <summary>
        /// Get and save in .dxf file all curves from the file
        /// </summary>
        /// <param name="folder">destination folder for curve file</param>
        /// <returns>DxfCurve containing filepath of the curve</returns>
        public IEnumerable<PDxfCurve> GetAllDxfCurves(string folder, string fromLayer)
        {
            var index = 0;
            PDxfCurve pdxfCurve;
            foreach (var polyline in _document.Entities.OfType<DxfLwPolyline>().Where(lw=>lw.Layer==fromLayer))
            {

                if (index + 1 < _tempDxfCurves.Count)
                {
                    pdxfCurve = _tempDxfCurves[index];
                }
                else
                {
                    var center = polyline.Vertices.GetPolylineCenter();
                    var vertices = polyline.Vertices.Select(vertex =>
                    {
                        var x = vertex.X - center.x;
                        var y = vertex.Y - center.y;
                        var vert = vertex;
                        vert.X = x;
                        vert.Y = y;
                        return vert;
                    });

                    var lw = new DxfLwPolyline(vertices);
                    lw.IsClosed = polyline.IsClosed;
                    var doc = new DxfFile();
                    doc.Header.Version = DxfAcadVersion.R14;
                    doc.Entities.Add(lw);
                    var filePostfix = Guid.NewGuid().ToString();
                    var fullPath = Path.Combine(folder, $"{TEMP_FILE_NAME}{filePostfix}.dxf");
                    doc.Save(fullPath);
                    pdxfCurve = new PDxfCurve(center.x,center.y,0, new DxfCurve(fullPath),polyline.Layer,polyline.Color.ToRGB());
                    _tempDxfCurves.Add(pdxfCurve);
                }

                index++;
                yield return pdxfCurve;
            }
        }

        public IEnumerable<PDxfCurve2> GetAllDxfCurves2(string folder, string fromLayer)
        {
            return _document.Entities.OfType<DxfLwPolyline>()
                .Where(p=>p.Layer==fromLayer)
                .Select(polyline => {
                    var centerX = polyline.Vertices.GetPolylineCenter().x;
                    var centerY = polyline.Vertices.GetPolylineCenter().y;
                    return new PDxfCurve2(centerX, centerY, 0,
                    new Curve { Vertices = polyline.Vertices.Select(vertex => (vertex.X - centerX, vertex.Y - centerY, vertex.Bulge)) },
                    polyline.Layer, polyline.Color.ToRGB(), polyline.IsClosed, this, folder);
                });
        }
        public void WriteCurveToFile(string filePath, Curve curve, bool isClosed)
        {
            var lw = new DxfLwPolyline(curve.Vertices.Select(v=>new DxfLwPolylineVertex{ X=v.X, Y=v.Y,Bulge=v.Bulge}));
            lw.ConstantWidth = 0.1d;
            lw.IsClosed = isClosed;
            var doc = new DxfFile();
            doc.Header.Version = DxfAcadVersion.Max;
            doc.Entities.Add(lw);
            doc.Save(filePath);
        }

        public IDictionary<string, int> GetLayers() => _document.Layers.ToDictionary(layer => layer.Name, layer => layer.Color.ToRGB());

        public IDictionary<string, IEnumerable<(string objType, int count)>> GetLayersStructure()
        {
            return _document.Layers
                .ToDictionary(layer => layer.Name, layer => _document.Entities
                    .Where(ent => ent.Layer == layer.Name)
                    .GroupBy(ent => ent.EntityTypeString)
                    .Select(group => (group.Key, group.Count())));
        }

        public (double width, double height) GetSize()
        {

            var w = _document.GetBoundingBox().Size.X;
            var h = _document.GetBoundingBox().Size.Y;
            return (w, h);
        }

        public IEnumerable<PPoint> GetPoints()
        {
            return _document.Entities.OfType<DxfModelPoint>()
                 .Select(point =>
                 new PPoint(point.Location.X, point.Location.Y, 0, new Laser.Entities.Point
                 {
                     X = point.Location.X,
                     Y = point.Location.Y,
                 }, point.Layer, point.Color.ToRGB())
             );
        }
    }
}
