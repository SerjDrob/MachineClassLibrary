using MachineClassLibrary.Laser.Entities;
using netDxf;
using System.Collections.Generic;

namespace MachineClassLibrary.Classes
{

    public class DxfReader
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
                yield return new PCircle(circle.Center.X, circle.Center.Y, 0, new Laser.Entities.Circle() { Radius = circle.Radius }, circle.Layer.Name);
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
                }, line.Layer.Name);
            }
        }
    }
}
