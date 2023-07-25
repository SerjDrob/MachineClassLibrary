using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Microsoft.Toolkit.Diagnostics;
using netDxf;

namespace MachineClassLibrary.GeometryUtility
{
    public partial class CoorSystemBase<TPlaceEnum> : ICoorSystem<TPlaceEnum> where TPlaceEnum : Enum
    {
        protected readonly Matrix3 _workTransformation;
        protected CoorSystemBase(Matrix3 mainMatrix)
        {
            _workTransformation = mainMatrix;
        }

        protected Dictionary<TPlaceEnum, ICoorSystem<TPlaceEnum>> _subSystems = new();
        protected bool _axisXNeg = false;
        protected bool _axisYNeg = false;
        public object Clone()
        {
            var copy = new CoorSystemBase<TPlaceEnum>(_workTransformation);
            copy._axisXNeg = _axisXNeg;
            copy._axisYNeg = _axisYNeg;
            copy._axisXNeg = _axisXNeg;
            copy._subSystems = _subSystems;

            return copy;
        }
        public ICoorSystem<TPlaceEnum> ExtractSubSystem(TPlaceEnum from) => (ICoorSystem<TPlaceEnum>)_subSystems[from].Clone();
        public double[] FromGlobal(double x, double y)
        {
            try
            {
                var vector = new netDxf.Vector3(x, y, 1);
                var result = _workTransformation.Inverse() * vector;

                var _axisXSign = _axisXNeg ? -1 : 1;
                var _axisYSign = _axisYNeg ? -1 : 1;

                var resX = _axisXSign * result.X;
                var resY = _axisYSign * result.Y;

                _axisXNeg = false;
                _axisYNeg = false;

                return new double[2] { resX, resY };
            }
            catch (Exception)
            {
                throw new Exception("System matrix is not invertible");
            }

        }
        public double[] FromSub(TPlaceEnum from, double x, double y)
        {
            Guard.IsNotNull(_subSystems, nameof(_subSystems));
            var point = _subSystems.ContainsKey(from) ? _subSystems[from].WithAxes(_axisXNeg, _axisYNeg).FromGlobal(x, y) : throw new KeyNotFoundException($"Subsystem {from} is not set");
            _axisXNeg = false;
            _axisYNeg = false;
            return point;
        }
        public float[] GetMainMatrixElements()
        {
            return new float[]{ (float) _workTransformation.M11, (float) _workTransformation.M12,
                                (float) _workTransformation.M21, (float) _workTransformation.M22,
                                (float) _workTransformation.M13, (float) _workTransformation.M23 };
        }
        public void SetRelatedSystem(TPlaceEnum name, double offsetX, double offsetY)
        {
            var translate = new Matrix3(m11: 1, m12: 0, m13: offsetX,
                                        m21: 0, m22: 1, m23: offsetY,
                                        m31: 0, m32: 0, m33: 1);

            var matrix = translate * _workTransformation;
            var sub = new CoorSystemBase<TPlaceEnum>(matrix);
            _subSystems[name] = sub;
        }
        public void SetRelatedSystem(TPlaceEnum name, Matrix3x2 matrix)
        {
            var sub = new CoorSystemBase<TPlaceEnum>(matrix.ConvertMatrix());
            _subSystems[name] = sub;
        }
        public double[] ToGlobal(double x, double y)
        {
            var vector = new netDxf.Vector3(x, y, 1);
            var result = _workTransformation * vector;
            var points = new PointF[] { new((float)result.X, (float)result.Y) };
            return new double[2] { points[0].X, points[0].Y };
        }
        public double[] ToSub(TPlaceEnum to, double x, double y)
        {
            Guard.IsNotNull(_subSystems, nameof(_subSystems));
            return _subSystems.ContainsKey(to) ? _subSystems[to].ToGlobal(x, y) : throw new KeyNotFoundException($"Subsystem {to} is not set");
        }
        public ICoorSystem<TPlaceEnum> WithAxes(bool negX, bool negY)
        {
            _axisXNeg = negX;
            _axisYNeg = negY;
            return this;
        }
        public RelatedSystemBuilder<TPlaceEnum> BuildRelatedSystem() => new RelatedSystemBuilder<TPlaceEnum>(_workTransformation.ConvertMatrix(), this);
    }
}