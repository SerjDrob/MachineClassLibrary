using MachineClassLibrary.Classes;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
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

            var result = points.Zip(_curve.Vertices, (p, v) => ((double)p.X, (double)p.Y, v.Bulge));
            return new Curve { Vertices = result };
        }
        
    }

    public class PDxfCurve2 : IProcObject<DxfCurve>
    {
        public PDxfCurve2(double x, double y, double angle, Curve pObject, 
            string layerName, int argBColor, bool isClosed, IDxfReader dxfReader, string folder)
        {           
            _pCurve = new PCurve(x, y, angle, pObject, layerName, argBColor);
            _isClosed = isClosed;
            _dxfReader = dxfReader;
            _folder = folder;
            _initCurve = pObject;
        }

        private readonly Curve _initCurve;
        private readonly string _folder;
        private readonly PCurve _pCurve;
        private readonly IDxfReader _dxfReader;
        private readonly bool _isClosed;
        public DxfCurve PObject { get => GetDxfCurve(); init => throw new NotImplementedException(); }
        public int ARGBColor { get => _pCurve.ARGBColor; set => throw new NotImplementedException(); }
        public string LayerName { get => _pCurve.LayerName; set => throw new NotImplementedException(); }
        public double X { get => _pCurve.X; init => throw new NotImplementedException(); }
        public double Y { get => _pCurve.Y; init => throw new NotImplementedException(); }
        public double Angle { get => _pCurve.Angle; init => throw new NotImplementedException(); }

        public double Scaling => _pCurve.Scaling;
        public bool MirrorX => _pCurve.MirrorX;
        public bool Turn90 => _pCurve.Turn90;

        private DxfCurve GetDxfCurve()
        {
            var trCurve = _pCurve.PObject;
            var filePostfix = Guid.NewGuid().ToString();
            var fullPath = Path.Combine(_folder, $"curve{filePostfix}.dxf");
            _dxfReader.WriteCurveToFile(fullPath, trCurve, _isClosed);
            return new DxfCurve(fullPath);
        }

        public IProcObject<DxfCurve> CloneWithPosition(double x, double y) => new PDxfCurve2(x, y, Angle, _initCurve, LayerName, ARGBColor,_isClosed,_dxfReader,_folder);

        public (double x, double y) GetSize()
        {
            throw new NotImplementedException();
        }

        public void Scale(double scale)
        {
            _pCurve.Scale(scale);
        }

        public void SetMirrorX(bool mirror)
        {
            _pCurve.SetMirrorX(mirror);
        }

        public void SetTurn90(bool turn)
        {
            _pCurve.SetTurn90(turn);
        }
    }
}
