using System;

namespace MachineClassLibrary.Laser.Entities
{
    public class PCircle : IProcObject<Circle>
    {
        public PCircle(double x, double y, double angle, Circle pObject, string layerName)
        {
            X = x;
            Y = y;
            Angle = angle;
            PObject = pObject;
            LayerName = layerName;
        }

        public double X { get; init; }
        public double Y { get; init; }
        public double Angle { get; init; }
        public Circle PObject { get; init; }
        public string LayerName { get; set; }

        public IProcObject<Circle> CloneWithPosition(double x, double y) => new PCircle(x, y, Angle, PObject, LayerName);

        public (double x, double y) GetSize()
        {
            throw new NotImplementedException();
        }
    }
}
