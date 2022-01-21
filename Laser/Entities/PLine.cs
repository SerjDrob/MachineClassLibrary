using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineClassLibrary.Laser.Entities
{
    public class PLine : IProcObject<Line>
    {
        public PLine(double x, double y, double angle, Line pObject, string layerName, int rgbColor)
        {
            X = x;
            Y = y;
            Angle = angle;
            PObject = pObject;
            LayerName = layerName;
            ARGBColor = rgbColor;
        }

        public double X { get; init; }
        public double Y { get; init; }
        public double Angle { get; init; }
        public Line PObject { get; init; }
        public string LayerName { get; set; }
        public int ARGBColor { get; set; }

        public IProcObject<Line> CloneWithPosition(double x, double y) => new PLine(x, y, Angle, PObject, LayerName,ARGBColor);

        public (double x, double y) GetSize()
        {
            return (Math.Abs(PObject.X2 - PObject.X1), Math.Abs(PObject.Y2 - PObject.Y1));
        }
    }
}
