using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
using MachineClassLibrary.Laser.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MachineClassLibrary.Classes
{
    public class IMDxfReader : IDxfReader
    {
        private readonly string _fileName;
        public readonly DxfFile Document;
        public IMDxfReader(string fileName)
        {
            _fileName = fileName;
            Document = DxfFile.Load(_fileName);
        }
        public IEnumerable<PCircle> GetCircles()
        {
            foreach (var circle in Document.Entities.Where(entity => entity.EntityType is DxfEntityType.Circle))
            {
                yield return new PCircle(((DxfCircle)circle).Center.X, ((DxfCircle)circle).Center.Y, 0, new Laser.Entities.Circle() { Radius = ((DxfCircle)circle).Radius }, ((DxfCircle)circle).Layer, ((DxfCircle)circle).Color.ToRGB());
            }
        }

        public IEnumerable<PLine> GetLines()
        {
            foreach (var line in Document.Entities.Where(entity => entity.EntityType is DxfEntityType.Line))
            {
                var xCenter = ((DxfLine)line).P2.X - ((DxfLine)line).P1.X;
                var yCenter = ((DxfLine)line).P2.Y - ((DxfLine)line).P1.Y;
                yield return new PLine(xCenter, yCenter, 0, new Laser.Entities.Line()
                {
                    X1 = ((DxfLine)line).P1.X,
                    Y1 = ((DxfLine)line).P1.Y,
                    X2 = ((DxfLine)line).P2.X,
                    Y2 = ((DxfLine)line).P2.Y
                }, ((DxfLine)line).Layer, ((DxfLine)line).Color.ToRGB());
            }
        }
        public IEnumerable<PLine> GetAllSegments()
        {            
            foreach (var polyline in Document.Entities.Where(entity=>entity.EntityType is DxfEntityType.LwPolyline))
            {

                var figures = ((DxfLwPolyline)polyline).AsSimpleEntities();
                
                foreach (var newEntity in figures)
                {
                    if (newEntity is DxfLine dxfLine)
                    {
                        var xCenter = dxfLine.P2.X - dxfLine.P1.X;
                        var yCenter = dxfLine.P2.Y - dxfLine.P1.Y;
                        yield return new PLine(xCenter, yCenter, 0, new Laser.Entities.Line()
                        {
                            X1 = dxfLine.P1.X,
                            Y1 = dxfLine.P1.Y,
                            X2 = dxfLine.P2.X,
                            Y2 = dxfLine.P2.Y
                        }, dxfLine.Layer, dxfLine.Color.ToRGB());
                    }                    
                }

            }
        }
        public IDictionary<string, int> GetLayers()
        {
            var result  = new Dictionary<string, int>();    
            foreach (var layer in Document.Layers)
            {
                result.TryAdd(layer.Name,layer.Color.ToRGB());
            }
            return result;
        }
    }
}
