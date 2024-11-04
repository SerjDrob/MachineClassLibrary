using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;

namespace MachineClassLibrary.Laser.Entities
{
    public class PCurve : IProcObject<Curve>
    {
        public PCurve(double x, double y, double angle, Curve pObject, string layerName, int argBColor)
        {
            Id = Guid.NewGuid();
            X = x;
            Y = y;
            Angle = angle;
            PObject = pObject;
            LayerName = layerName;
            ARGBColor = argBColor;
        }
        private PCurve(double x, double y, double angle, Curve pObject, string layerName, int argBColor, Guid id)
        {
            Id = id;
            X = x;
            Y = y;
            Angle = angle;
            PObject = pObject;
            LayerName = layerName;
            ARGBColor = argBColor;
        }
        public Guid Id { get; init; }
        public double X { get; init; }
        public double Y { get; init; }
        public double Angle { get; init; }
        public Curve PObject
        {
            get => GetTransformedCurve(); init => _curve = value;
        }
        public string LayerName { get; set; }
        public int ARGBColor { get; set; }
        private readonly Curve _curve;
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
        public IProcObject<Curve> CloneWithPosition(double x, double y) => new PCurve(x, y, Angle, PObject, LayerName, ARGBColor, Id);
        IProcObject IProcObject.CloneWithPosition(double x, double y) => CloneWithPosition(x, y);
        public override string ToString() => $"{GetType().Name} X:{X}, Y:{Y} Id = {Id}";

        
        public (double x, double y) GetSize()
        {
            return (PObject.Bounds.Width, PObject.Bounds.Height);
        }

        private Curve GetTransformedCurve()
        {
            var transformation = Matrix3x2.Identity;
            var mirror = Matrix3x2.CreateScale(MirrorX ? -1 : 1, 1);
            var scaling = Matrix3x2.CreateScale((float)Scaling);
            var rotation = Matrix3x2.CreateRotation(Turn90 ? MathF.PI * 90 / 180 : 0);
            transformation *= scaling * mirror * rotation;
            var matrix = new Matrix(transformation);

            var points = _curve.Vertices.Select(vertex => new PointF((float)vertex.X, (float)vertex.Y)).ToArray();
            matrix.TransformPoints(points);

            var result = points.Zip(_curve.Vertices, (p, v) => ((double)p.X, (double)p.Y, MirrorX ? -v.Bulge : v.Bulge));
            return new Curve(result, _curve.IsClosed) { Bounds = _curve.Bounds };
        }
        
    }
}
