using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
using MachineClassLibrary.Laser.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MachineClassLibrary.Classes
{
    public class IMDxfReader : IDxfReader
    {
        private const string TEMP_FILE_NAME = "tempcurve";
        private readonly string _fileName;
        private readonly DxfFile _document;
        private List<PDxfCurve> _tempDxfCurves = new();
        public IMDxfReader(string fileName)
        {
            _fileName = fileName;
            try
            {
                _document = DxfFile.Load(_fileName);
            }
            catch (DxfReadException ex)
            {
                throw new DxfReaderException("Ошибка чтения файла",ex);
            }
        }
        public IEnumerable<PCircle> GetCircles(string fromLayer = null)
        {
            return _document.Entities.OfType<DxfCircle>()
                .Where(lw => fromLayer is null ? true : lw.Layer == fromLayer)
                .Select(circle => new PCircle(circle.Center.X, circle.Center.Y, 0,
                new Circle() { Bounds = circle.GetBoundingBox().ToRect(), Radius = circle.Radius }, circle.Layer, circle.Color.ToRGB()));
        }
        public IEnumerable<IProcObject> GetObjectsFromLayer<TObject>(string layerName) where TObject : IProcObject
        {
            if (typeof(TObject) == typeof(PCurve)) return GetAllCurves(layerName);
            if (typeof(TObject) == typeof(PCircle)) return GetCircles(layerName);
            return null;
        }
        public IEnumerable<IProcObject> GetSelectedObjectsFromLayer(Rect selection, string layerName)
        {
            var lines = _document.Entities.OfType<DxfLwPolyline>()
                .Where(lw => lw.Layer == layerName && selection.Contains(lw.GetBoundingBox().ToRect()))
                .ToArray()
                .Select(l => (IProcObject)ConvertPolyline(l));

            var circles = _document.Entities.OfType<DxfCircle>()
                .Where(lw => lw.Layer == layerName && selection.Contains(lw.GetBoundingBox().ToRect()))
                .ToArray()
                .Select(c => (IProcObject)ConvertCircle(c));

            var file = RemoveFromDocumentBySelection(selection, layerName);


            return lines.Concat(circles);
        }

        private IEnumerable<DxfEntity> GetUnselectedEntitiesFromLayer(Rect selection, string layerName)
        {
            return _document.Entities
                .Where(e => e.Layer == layerName && !selection.Contains(e.GetBoundingBox().ToRect()))
                .ToArray();
        }

        private DxfFile RemoveFromDocumentBySelection(Rect selection, string layerName)
        {
            var file = new DxfFile();
            foreach (var entity in _document.Entities.Where(e => e.Layer != layerName))
            {
                file.Entities.Add(entity);
            }

            foreach (var entity in GetUnselectedEntitiesFromLayer(selection, layerName))
            {
                file.Entities.Add(entity);
            }

            return file;
        }

        private PCircle ConvertCircle(DxfCircle circle)
        {
            return new PCircle(circle.Center.X, circle.Center.Y, 0,
                new Circle() { Radius = circle.Radius }, circle.Layer, circle.Color.ToRGB());
        }
        private PCurve ConvertPolyline(DxfLwPolyline polyline)
        {
            var center = polyline.Vertices.GetPolylineCenter();

            return new PCurve(center.x, center.y, 0,
                    new Curve(polyline.Vertices.Select(vertex => (vertex.X - center.x, vertex.Y - center.y, vertex.Bulge)),
                    polyline.IsClosed),
                    polyline.Layer, polyline.Color.ToRGB());
        }

        public IEnumerable<PLine> GetLines()
        {
            return _document.Entities.OfType<DxfLine>()
                .Select(line =>
                new PLine(line.P2.X - line.P1.X, line.P2.Y - line.P1.Y, 0, new Line()
                {
                    Bounds = line.GetBoundingBox().ToRect(),
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
                         Bounds = dxfLine.GetBoundingBox().ToRect(),
                         X1 = dxfLine.P1.X,
                         Y1 = dxfLine.P1.Y,
                         X2 = dxfLine.P2.X,
                         Y2 = dxfLine.P2.Y
                     }, polyline.Layer, dxfLine.Color.ToRGB())
                 )).SelectMany(x => x);
        }
        public IEnumerable<PCurve> GetAllCurves(string fromLayer = null)
        {
            return _document.Entities.OfType<DxfLwPolyline>()
                .Where(lw => fromLayer is null ? true : lw.Layer == fromLayer)
                .Select(polyline =>
                {
                    var center = polyline.Vertices.GetPolylineCenter();
                    return new PCurve(center.x, center.y, 0,
                    new Curve(polyline.Vertices.Select(vertex => (vertex.X - center.x, vertex.Y - center.y, vertex.Bulge)),
                    polyline.IsClosed)
                    { Bounds = polyline.GetBoundingBox().ToRect() },
                    polyline.Layer, polyline.Color.ToRGB());
                });
        }
        /// <summary>
        /// Get and save in .dxf file all curves from the file
        /// </summary>
        /// <param name="folder">destination folder for curve file</param>
        /// <returns>DxfCurve containing filepath of the curve</returns>
        public void WriteCurveToFile(string filePath, Curve curve, bool isClosed)
        {
            var lw = new DxfLwPolyline(curve.Vertices.Select(v => new DxfLwPolylineVertex { X = v.X, Y = v.Y, Bulge = v.Bulge }));
            lw.ConstantWidth = 0.1d;
            lw.IsClosed = isClosed;
            var doc = new DxfFile();
            doc.Header.Version = DxfAcadVersion.Max;
            doc.Entities.Add(lw);
            doc.Save(filePath);
        }
        //TODO make function WriteShapesToFile(string filePath, params IShape[] shapes); 
        public void WriteCircleToFile(string filePath, Circle circle)
        {
            var c = new DxfCircle(new DxfPoint(0, 0, 0), circle.Radius);
            c.Thickness = 0.1d;
            var doc = new DxfFile();
            doc.Header.Version = DxfAcadVersion.Max;
            doc.Entities.Add(c);
            doc.Save(filePath);
        }

        public void WriteShapesToFile(string filePath, params IShape[] shapes)
        {
            var doc = new DxfFile();
            doc.Header.Version = DxfAcadVersion.Max;
            foreach (var shape in shapes)
            {
                var ent = shape switch
                {
                    Circle circle => new DxfCircle(new DxfPoint(circle.CenterX, circle.CenterY, 0), circle.Radius),
                    Curve curve => (DxfEntity) new DxfLwPolyline(curve.Vertices.Select(v => new DxfLwPolylineVertex { X = v.X, Y = v.Y, Bulge = v.Bulge })){ IsClosed = true, ConstantWidth = 0.1d }
                };
                doc.Entities.Add(ent);
            }
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
            var boundingBox = _document.GetBoundingBox();
            var w = boundingBox.Size.X;
            var h = boundingBox.Size.Y;
            return (w, h);
        }
        public IEnumerable<PPoint> GetPoints()
        {
            return _document.Entities.OfType<DxfModelPoint>()
                 .Select(point =>
                 new PPoint(point.Location.X, point.Location.Y, 0, new Laser.Entities.Point
                 {
                     Bounds = point.GetBoundingBox().ToRect(),
                     X = point.Location.X,
                     Y = point.Location.Y,
                 }, point.Layer, point.Color.ToRGB())
             );
        }

        
    }

}
