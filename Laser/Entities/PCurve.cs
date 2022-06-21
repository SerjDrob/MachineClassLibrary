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
            X = x;
            Y = y;
            Angle = angle;
            PObject = pObject;
            LayerName = layerName;
            ARGBColor = argBColor;
        }

        public double X { get; init; }
        public double Y { get; init; }
        public double Angle { get; init; }
        public Curve PObject { get => GetTransformedCurve(); init { _curve = value; } }
        public string LayerName { get; set; }
        public int ARGBColor { get; set; }
        private readonly Curve _curve;
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
        public IProcObject<Curve> CloneWithPosition(double x, double y) => new PCurve(x, y, Angle, PObject, LayerName, ARGBColor);

        public (double x, double y) GetSize()
        {
            throw new NotImplementedException();
        }

        private Curve GetTransformedCurve()
        {
            var transformation = Matrix3x2.Identity;
            var mirror = Matrix3x2.CreateScale(MirrorX ? -1 : 1, 1);
            var scaling = Matrix3x2.CreateScale((float)Scaling);
            var rotation = Matrix3x2.CreateRotation(Turn90 ? MathF.PI * 90 / 180 : 0);
            transformation *= scaling * mirror * rotation;
            var matrix = new Matrix(transformation/*scaling*/);

            var points = _curve.Vertices.Select(vertex => new PointF((float)vertex.X, (float)vertex.Y)).ToArray();
            matrix.TransformPoints(points);

            var result = points.Zip(_curve.Vertices, (p, v) => ((double)p.X, (double)p.Y, MirrorX ? -v.Bulge : v.Bulge));
            return new Curve { Vertices = result };
        }
        
    }
}
