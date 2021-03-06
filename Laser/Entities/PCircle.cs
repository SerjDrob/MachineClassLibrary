using System;

namespace MachineClassLibrary.Laser.Entities
{
    public class PCircle : IProcObject<Circle>
    {
        public PCircle(double x, double y, double angle, Circle pObject, string layerName, int rgbColor)
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
        private readonly Circle _circle;
        public Circle PObject { get=>GetTransformedCircle(); init { _circle = value; } }
        public string LayerName { get; set; }
        public int ARGBColor { get; set; }

        public double Scaling { get; private set; } = 1;

        public bool MirrorX { get; private set; } = false;

        public bool Turn90 { get; private set; } = false;
        public bool IsBeingProcessed { get; set; } = false;
        public bool IsProcessed { get; set; } = false;

        public void Scale(double scale)
        {
            Scaling = scale;
        }

        public void SetMirrorX(bool mirror)
        {
            MirrorX = mirror;
        }

        public void SetTurn90(bool turn)
        {
            Turn90 = turn;
        }
        public IProcObject<Circle> CloneWithPosition(double x, double y) => new PCircle(x, y, Angle, PObject, LayerName, ARGBColor);

        private Circle GetTransformedCircle() 
        {
            return new Circle { Radius = _circle.Radius * Scaling };
        }

        public (double x, double y) GetSize()
        {
            throw new NotImplementedException();
        }
        
    }
}
