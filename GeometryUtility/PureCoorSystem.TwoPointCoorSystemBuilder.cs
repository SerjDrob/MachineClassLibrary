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
            private Matrix3 _workMatrix = new Matrix3(m11: 1, m12: 0, m13: 0,
                                                      m21: 0, m22: 1, m23: 0,
                                                      m31: 0, m32: 0, m33: 1);
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
            public TwoPointCoorSystemBuilder FormWorkMatrix(double xScaleMul, double yScaleMul)
            {
                var point0 = _workTransformation * new netDxf.Vector3(_firstPair.originPoint.X, _firstPair.originPoint.Y, 1);
                var point1 = _workTransformation * new netDxf.Vector3(_secondPair.originPoint.X, _secondPair.originPoint.Y, 1);

                //var derivativeLength = Math.Sqrt(Math.Pow(point1.X - point0.X,2) + Math.Pow(point1.Y - point0.Y,2));
                //var originLength = Math.Sqrt(Math.Pow(_secondPair.originPoint.X - _firstPair.originPoint.X, 2) + Math.Pow(_secondPair.originPoint.Y - _firstPair.originPoint.Y, 2));

                //var scale = derivativeLength / originLength;
                
                var S = new Matrix3(m11: xScaleMul/*scale*/, m12: 0, m13: 0,
                                           m21: 0, m22: yScaleMul/*scale*/, m23: 0,
                                           m31: 0, m32: 0, m33: 1);

                var k1 = (_secondPair.derivativePoint.Y - _firstPair.derivativePoint.Y) / (_secondPair.derivativePoint.X - _firstPair.derivativePoint.X);
                var k2 = (point1.Y - point0.Y) / (point1.X - point0.X);

                var angle = Math.Atan((k1 - k2) / (1 + k1 * k2));


                var R = new Matrix3(m11: Math.Cos(angle), m12: -Math.Sin(angle), m13: 0,
                                            m21: Math.Sin(angle), m22: Math.Cos(angle), m23: 0,
                                            m31: 0, m32: 0, m33: 1);


                


                var m1 = R * _workTransformation * S;


                var v1 = new Vector3(20000, 10000, 1);

                

                var p1 = m1 * new netDxf.Vector3(_firstPair.originPoint.X, _firstPair.originPoint.Y, 1);
                var deltaX = _firstPair.derivativePoint.X - p1.X;
                var deltaY = _firstPair.derivativePoint.Y - p1.Y;


                var Translate = new Matrix3(m11: 1, m12: 0, m13: deltaX,
                                                                    m21: 0, m22: 1, m23: deltaY,
                                                                    m31: 0, m32: 0, m33: 1);



                _workMatrix = Translate * R * _workTransformation * S;


                var v2 = _workMatrix * v1;

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

