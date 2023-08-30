using System;

namespace MachineClassLibrary.Laser.Entities
{
    public class PCircle : IProcObject<Circle>
    {
        public PCircle(double x, double y, double angle, Circle pObject, string layerName, int rgbColor)
        {
            Id = Guid.NewGuid();
            X = x;
            Y = y;
            Angle = angle;
            PObject = pObject;
            LayerName = layerName;
            ARGBColor = rgbColor;
        }
        private PCircle(double x, double y, double angle, Circle pObject, bool mirrorX, bool turn90, double scaling, string layerName, int rgbColor, Guid id)
        {
            Id = id;
            X = x;
            Y = y;
            Angle = angle;
            PObject = pObject;
            LayerName = layerName;
            ARGBColor = rgbColor;
            MirrorX = mirrorX;
            Turn90 = turn90;
            Scaling = scaling;
        }
        public double X { get; init; }
        public double Y { get; init; }
        public double Angle { get; init; }
        private readonly Circle _pobject;
        public Circle PObject
        {
            get => GetTransformedCircle(); init => _pobject = value;
        }
        public string LayerName { get; set; }
        public int ARGBColor { get; set; }

        public double Scaling { get; private set; } = 1;

        public bool MirrorX { get; private set; } = false;

        public bool Turn90 { get; private set; } = false;
        public bool IsBeingProcessed { get; set; } = false;
        public bool IsProcessed { get; set; } = false;
        public bool ToProcess { get; set; } = true;

        public Guid Id { get; init; }

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
        public IProcObject<Circle> CloneWithPosition(double x, double y) => new PCircle(x, y, Angle, _pobject, MirrorX, Turn90, Scaling, LayerName, ARGBColor, Id);
        public override string ToString() => $"{GetType().Name} X:{X}, Y:{Y} Id = {Id}";
        private Circle GetTransformedCircle() 
        {
            return new Circle { Radius = _pobject.Radius * Scaling };
        }

        public (double x, double y) GetSize()
        {
            return (PObject.Bounds.Width, PObject.Bounds.Height);
        }

        IProcObject IProcObject.CloneWithPosition(double x, double y) => CloneWithPosition(x, y);
    }
}
