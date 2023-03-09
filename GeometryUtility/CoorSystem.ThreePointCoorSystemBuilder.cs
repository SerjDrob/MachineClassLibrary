using netDxf;
using System;
using System.Drawing;

namespace MachineClassLibrary.GeometryUtility
{
    public partial class CoorSystem<TPlaceEnum> where TPlaceEnum : Enum
    {
        public class ThreePointCoorSystemBuilder
        {
            private (PointF originPoint, PointF derivativePoint) _firstPair;
            private (PointF originPoint, PointF derivativePoint) _secondPair;
            private (PointF originPoint, PointF derivativePoint) _thirdPair;
            
            private Matrix3 _workMatrix = new Matrix3(m11: 1, m12: 0, m13: 0,
                                                      m21: 0, m22: 1, m23: 0,
                                                      m31: 0, m32: 0, m33: 1);
            
            private Matrix3 _pureMatrix = new Matrix3(m11: 1, m12: 0, m13: 0,
                                                      m21: 0, m22: 1, m23: 0,
                                                      m31: 0, m32: 0, m33: 1);

            public ThreePointCoorSystemBuilder SetFirstPointPair(PointF originPoint, PointF derivativePoint)
            {
                _firstPair = (originPoint, derivativePoint);
                return this;
            }
            public ThreePointCoorSystemBuilder SetSecondPointPair(PointF originPoint, PointF derivativePoint)
            {
                _secondPair = (originPoint, derivativePoint);
                return this;
            }
            public ThreePointCoorSystemBuilder SetThirdPointPair(PointF originPoint, PointF derivativePoint)
            {
                _thirdPair = (originPoint, derivativePoint);
                return this;
            }
            public ThreePointCoorSystemBuilder FormWorkMatrix(double xScaleMul, double yScaleMul)
            {
                var first = _firstPair;
                var second = _secondPair;
                var third = _thirdPair;

                var initialPoints = new Matrix3(m11: first.Item1.X, m12: first.Item1.Y, m13: 1,
                                                m21: second.Item1.X, m22: second.Item1.Y, m23: 1,
                                                m31: third.Item1.X, m32: third.Item1.Y, m33: 1);


                var transformedPoints = new Matrix3(m11: first.Item2.X, m12: first.Item2.Y, m13: 1,
                                                    m21: second.Item2.X, m22: second.Item2.Y, m23: 1,
                                                    m31: third.Item2.X, m32: third.Item2.Y, m33: 1);

                var invert = initialPoints.Inverse();

                var _mainTransformation = invert * transformedPoints;

                _mainTransformation = _mainTransformation.Transpose();


                _workMatrix = _mainTransformation;



                var point1 = _mainTransformation * (new netDxf.Vector3(1, 0, 1));
                var point0 = _mainTransformation * (new netDxf.Vector3(0, 0, 1));
                var point2 = _mainTransformation * (new netDxf.Vector3(0, 1, 1));

                var angle = Math.Atan2(point1.Y - point0.Y, point1.X - point0.X);

                var R = new Matrix3(m11: Math.Cos(angle), m12: -Math.Sin(angle), m13: 0,
                                    m21: Math.Sin(angle), m22: Math.Cos(angle), m23: 0,
                                    m31: 0, m32: 0, m33: 1);

                var deltaX = point0.X;
                var deltaY = point0.Y;

                var Translate = new Matrix3(m11: 1, m12: 0, m13: deltaX,
                                            m21: 0, m22: 1, m23: deltaY,
                                            m31: 0, m32: 0, m33: 1);


                var S = new Matrix3(m11: xScaleMul/*scaleX*/, m12: 0, m13: 0,
                                    m21: 0, m22: yScaleMul/*scaleY*/, m23: 0,
                                    m31: 0, m32: 0, m33: 1);

                var X = R.Inverse() * Translate.Inverse() * _mainTransformation * S.Inverse();
                _pureMatrix = X;
                return this;
            }
            public CoorSystem<TPlaceEnum> Build()
            {
                return new CoorSystem<TPlaceEnum>(_workMatrix);
            }
            public PureCoorSystem<TPlaceEnum> BuildPure()
            {
                return new PureCoorSystem<TPlaceEnum>(_pureMatrix);
            }
        }
    }
}

