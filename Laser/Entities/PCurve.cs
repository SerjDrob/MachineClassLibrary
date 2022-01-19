using System;

namespace MachineClassLibrary.Laser.Entities
{
    public class PCurve : IProcObject<Curve>
    {
        public PCurve(double x, double y, double angle, Curve pObject, string layerName)
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
        public Curve PObject { get; init; }
        public string LayerName { get; set; }

        public IProcObject<Curve> CloneWithPosition(double x, double y) => new PCurve(x, y, Angle, PObject, LayerName);

        public (double x, double y) GetSize()
        {
            throw new NotImplementedException();
        }
    }
}
