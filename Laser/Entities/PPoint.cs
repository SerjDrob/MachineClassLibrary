using System;

namespace MachineClassLibrary.Laser.Entities
{
    public class PPoint : IProcObject<Point>
    {
        public PPoint(double x, double y, double angle, Point pObject, string layerName, int aRGBColor)
        {
            X = x;
            Y = y;
            Angle = angle;
            PObject = pObject;
            LayerName = layerName;
            ARGBColor = aRGBColor;
        }
        public int ARGBColor { get; set; }
        public string LayerName { get; set; }
        public double X { get; init; }
        public double Y { get; init; }
        public double Angle { get; init; }
        public Point PObject { get; init; }

        public IProcObject<Point> CloneWithPosition(double x, double y) => new PPoint(x, y, Angle, PObject, LayerName, ARGBColor);
        
        public (double x, double y) GetSize()
        {
            throw new NotImplementedException();
        }
    }
}
