using System;

namespace MachineClassLibrary.Laser.Entities
{
    public class PDxfCurve : IProcObject<DxfCurve>
    {
        public PDxfCurve(double x, double y, double angle, DxfCurve pObject, string layerName, int rgbColor)
        {
            Id = Guid.NewGuid();
            X = x;
            Y = y;
            Angle = angle;
            PObject = pObject;
            LayerName = layerName;
            ARGBColor = rgbColor;
        }
        public Guid Id { get; private set; }
        public int ARGBColor { get; set; }
        public string LayerName { get; set; }
        public double X { get; init; }
        public double Y { get; init; }
        public double Angle { get; init; }
        public DxfCurve PObject { get; init; }
        public double Scaling { get; private set; } = 1;

        public bool MirrorX { get; private set; } = false;

        public bool Turn90 { get; private set; } = false;
        public bool IsBeingProcessed { get; set; } = false;
        public bool IsProcessed { get; set; } = false;
        public bool ToProcess { get; set; } = true;
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
        public IProcObject<DxfCurve> CloneWithPosition(double x, double y) => new PDxfCurve(x, y, Angle, PObject, LayerName, ARGBColor) { Id = this.Id };
        IProcObject IProcObject.CloneWithPosition(double x, double y) => CloneWithPosition(x, y);
        public (double x, double y) GetSize()
        {
            throw new NotImplementedException();
        }
       
    }
}
