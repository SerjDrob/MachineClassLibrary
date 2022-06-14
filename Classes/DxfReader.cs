using MachineClassLibrary.Laser.Entities;
using netDxf;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MachineClassLibrary.Classes
{

    public class DxfReader : IDxfReader
    {
        private readonly string _fileName;
        public readonly DxfDocument Document;
        public DxfReader(string fileName)
        {
            _fileName = fileName;
            Document = DxfDocument.Load(_fileName);
        }
        public IEnumerable<PCircle> GetCircles()
        {
            foreach (var circle in Document.Circles)
            {
                yield return new PCircle(circle.Center.X, circle.Center.Y, 0, new Laser.Entities.Circle() { Radius = circle.Radius }, circle.Layer.Name, circle.Color.ToColor().ToArgb());
            }
        }
        public IEnumerable<PLine> GetLines()
        {
            foreach (var line in Document.Lines)
            {
                var xCenter = line.EndPoint.X - line.StartPoint.X;
                var yCenter = line.EndPoint.Y - line.StartPoint.Y;
                yield return new PLine(xCenter, yCenter, 0, new Laser.Entities.Line()
                {
                    X1 = line.StartPoint.X,
                    Y1 = line.StartPoint.Y,
                    X2 = line.EndPoint.X,
                    Y2 = line.EndPoint.Y
                }, line.Layer.Name, line.Color.ToColor().ToArgb());
            }
        }

        public IDictionary<string, int> GetLayers()
        {
            var result = new Dictionary<string, int>();
            foreach (var layer in Document.Layers)
            {
                result.TryAdd(layer.Name,layer.Color.ToColor().ToArgb());
            }
            return result;
        }

        public IEnumerable<PLine> GetAllSegments()
        {
            foreach (var polyline in Document.LwPolylines)
            {
                var figures = polyline.Explode();
                foreach (var newEntity in figures)
                {
                    if (newEntity is netDxf.Entities.Line dxfLine)
                    {
                        var xCenter = dxfLine.EndPoint.X - dxfLine.StartPoint.X;
                        var yCenter = dxfLine.EndPoint.Y - dxfLine.StartPoint.Y;
                        yield return new PLine(xCenter, yCenter, 0, new Laser.Entities.Line()
                        {
                            X1 = dxfLine.StartPoint.X,
                            Y1 = dxfLine.StartPoint.Y,
                            X2 = dxfLine.EndPoint.X,
                            Y2 = dxfLine.EndPoint.Y
                        }, dxfLine.Layer.Name, dxfLine.Color.ToColor().ToArgb());
                    }                    
                }
            }
        }

        public IDictionary<string, IEnumerable<(string objType, int count)>> GetLayersStructure()
        {
            throw new System.NotImplementedException();
        }

        public (double width, double height) GetSize()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<PPoint> GetPoints()
        {
            throw new System.NotImplementedException();
            //return Document.Points.Select(p => new PointF((float)p.Position.X, (float)p.Position.Y));
        }

        public IEnumerable<PCurve> GetAllCurves()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<PDxfCurve> GetAllDxfCurves(string folder, string fromLayer)
        {
            throw new System.NotImplementedException();
        }

        public void WriteCurveToFile(string filePath, Curve curve, bool isClosed)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<PDxfCurve2> GetAllDxfCurves2(string folder, string fromLayer)
        {
            throw new System.NotImplementedException();
        }

        public void WriteCircleToFile(string filePath, Circle circle)
        {
            throw new System.NotImplementedException();
        }
    }
}
