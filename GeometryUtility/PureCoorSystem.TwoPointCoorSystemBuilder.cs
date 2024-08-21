using netDxf;
using System;
using System.Drawing;

namespace MachineClassLibrary.GeometryUtility
{



    public partial class PureCoorSystem<TPlace> where TPlace : Enum
    {
        public class TwoPointCoorSystemBuilder
        {
            private (PointF originPoint, PointF derivativePoint) _firstPair;
            private (PointF originPoint, PointF derivativePoint) _secondPair;
            private Matrix3 _workMatrix = new(m11: 1, m12: 0, m13: 0,
                                                     m21: 0, m22: 1, m23: 0,
                                                     m31: 0, m32: 0, m33: 1);
            private CoeffLine _xLine;
            private bool _useXLine;
            private CoeffLine _yLine;
            private bool _useYLine;
            private readonly Matrix3 _workTransformation;

            public TwoPointCoorSystemBuilder(Matrix3 workTransformation)
            {
                _workTransformation = workTransformation;
            }

            public TwoPointCoorSystemBuilder SetFirstPointPair(PointF originPoint, PointF derivativePoint)
            {
                _firstPair = (originPoint, derivativePoint);
                return this;
            }
            public TwoPointCoorSystemBuilder SetSecondPointPair(PointF originPoint, PointF derivativePoint)
            {
                _secondPair = (originPoint, derivativePoint);
                return this;
            }
            public TwoPointCoorSystemBuilder UseXCoeffLine(CoeffLine xLine)
            {
                _xLine = xLine;
                _useXLine = true;
                return this;
            }
            public TwoPointCoorSystemBuilder UseYCoeffLine(CoeffLine yLine)
            {
                _yLine = yLine;
                _useYLine= true;
                return this;
            }
            public TwoPointCoorSystemBuilder FormWorkMatrix(double xScaleMul, double yScaleMul)
            {
                var point0 = _workTransformation * new Vector3(_firstPair.originPoint.X, _firstPair.originPoint.Y, 1);
                var point1 = _workTransformation * new Vector3(_secondPair.originPoint.X, _secondPair.originPoint.Y, 1);

                var S = new Matrix3(m11: xScaleMul, m12: 0, m13: 0,
                                           m21: 0, m22: yScaleMul, m23: 0,
                                           m31: 0, m32: 0, m33: 1);

                
                var fderX = _useXLine ? _xLine[_firstPair.derivativePoint.X, true] : _firstPair.derivativePoint.X;
                var fderY = _useYLine ? _yLine[_firstPair.derivativePoint.Y, true] : _firstPair.derivativePoint.Y;
                var sderX = _useXLine ? _xLine[_secondPair.derivativePoint.X, true] : _secondPair.derivativePoint.X;
                var sderY = _useYLine ? _yLine[_secondPair.derivativePoint.Y, true] : _secondPair.derivativePoint.Y;

                var k1 = (sderY - fderY) / (sderX - fderX);
                var k2 = (point1.Y - point0.Y) / (point1.X - point0.X);

                var angle = Math.Atan((k1 - k2) / (1 + k1 * k2));

                var R = new Matrix3(m11: Math.Cos(angle), m12: -Math.Sin(angle), m13: 0,
                                           m21: Math.Sin(angle), m22: Math.Cos(angle), m23: 0,
                                           m31: 0,               m32: 0,               m33: 1);

                var m1 = R * _workTransformation * S;

                var p1 = m1 * new Vector3(_firstPair.originPoint.X, _firstPair.originPoint.Y, 1);
                var deltaX = fderX - p1.X;
                var deltaY = fderY - p1.Y;

                var Translate = new Matrix3(m11: 1, m12: 0, m13: deltaX,
                                                   m21: 0, m22: 1, m23: deltaY,
                                                   m31: 0, m32: 0, m33: 1);

                _workMatrix = Translate * R * _workTransformation * S;
                //_workMatrix = R.Inverse() * Translate.Inverse() * _workTransformation * S.Inverse();
                //R.Inverse() * Translate.Inverse() * _mainTransformation * S.Inverse();
                return this;
            }
            public ICoorSystem Build()
            {
                return CoorSystem<TPlace>.GetWorkMatrixSystemBuilder()
                    .SetWorkMatrix(_workMatrix.ConvertMatrix())
                    .Build();
            }
        }
    }
}

