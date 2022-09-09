using MachineClassLibrary.Classes;
using System;
using System.IO;

namespace MachineClassLibrary.Laser.Entities
{
    public class PDxfCurve2 : IProcObject<DxfCurve>
    {
        public PDxfCurve2(double x, double y, double angle, Curve pObject, 
            string layerName, int argBColor, bool isClosed, IDxfReader dxfReader, string folder)
        {
            Id = Guid.NewGuid();
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
        public Guid Id { get; private set; }
        public DxfCurve PObject { get => GetDxfCurve(); init => throw new NotImplementedException(); }
        public int ARGBColor { get => _pCurve.ARGBColor; set => throw new NotImplementedException(); }
        public string LayerName { get => _pCurve.LayerName; set => throw new NotImplementedException(); }
        public double X { get => _pCurve.X; init => throw new NotImplementedException(); }
        public double Y { get => _pCurve.Y; init => throw new NotImplementedException(); }
        public double Angle { get => _pCurve.Angle; init => throw new NotImplementedException(); }
        public bool IsBeingProcessed { get; set; } = false;
        public bool IsProcessed { get; set; } = false;
        public bool ToProcess { get; set; } = true;
        public double Scaling => _pCurve.Scaling;
        public bool MirrorX => _pCurve.MirrorX;
        public bool Turn90 => _pCurve.Turn90;

        private DxfCurve GetDxfCurve()
        {
            var trCurve = _pCurve.PObject;
            var filePostfix = Guid.NewGuid().ToString();
            var fullPath = Path.Combine(_folder, $"curve{filePostfix}.dxf");
            _dxfReader.WriteCurveToFile(fullPath, trCurve, _isClosed/*, MirrorX*/);
            return new DxfCurve(fullPath);
        }

        public IProcObject<DxfCurve> CloneWithPosition(double x, double y) => new PDxfCurve2(x, y, Angle, _initCurve, LayerName, ARGBColor, _isClosed, _dxfReader, _folder) { Id = this.Id };
        IProcObject IProcObject.CloneWithPosition(double x, double y) => CloneWithPosition(x, y);
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
