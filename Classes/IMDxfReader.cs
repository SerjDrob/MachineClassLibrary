using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
using MachineClassLibrary.Laser.Entities;
using System.Collections.Generic;
using System.Linq;


namespace MachineClassLibrary.Classes
{
    public class IMDxfReader : IDxfReader
    {
        private readonly string _fileName;
        private readonly DxfFile _document;
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
    }
}
