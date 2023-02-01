using Microsoft.Toolkit.Diagnostics;
//using netDxf;
using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
//using System.Numerics;
//using System.Drawing.Drawing2D;
using MachineClassLibrary.Classes;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
//using netDxf;
//using System.Numerics;

namespace MachineClassLibrary.GeometryUtility
{
    public class CoorSystem1<TPlaceEnum> : ICoorSystem<TPlaceEnum>, ICloneable where TPlaceEnum : Enum
    {
        private Dictionary<TPlaceEnum, CoorSystem<TPlaceEnum>> _subSystems = new();

        private readonly Matrix<double> _workTransformation;

        private bool _axisXNeg = false;
        private bool _axisYNeg = false;

        public CoorSystem1()
        {
            _workTransformation = Matrix<double>.Build.DenseIdentity(3,3);
        }

        private CoorSystem(Matrix3 mainMatrix)
        {
            _workTransformation = mainMatrix;
        }
        public void SetRelatedSystem(TPlaceEnum name, Matrix3x2 matrix)
        {
            var sub = new CoorSystem<TPlaceEnum>(matrix.ConvertMatrix());
            if (!_subSystems.TryAdd(name, sub))
            {
                _subSystems[name] = sub;
            }
        }
        public void SetRelatedSystem(TPlaceEnum name, double offsetX, double offsetY)
        {
            var translate = new Matrix3(m11: 1, m12: 0, m13: offsetX,
                                        m21: 0, m22: 1, m23: offsetY,
                                        m31: 0, m32: 0, m33: 1);

            var matrix = translate * _workTransformation;
            var sub = new CoorSystem<TPlaceEnum>(matrix);
            if (!_subSystems.TryAdd(name, sub))
            {
                _subSystems[name] = sub;
            }
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
        public static ThreePointCoorSystemBuilder<TPlaceEnum> GetThreePointSystemBuilder() => new ThreePointCoorSystemBuilder<TPlaceEnum>();
        public static WorkMatrixCoorSystemBuilder<TPlaceEnum> GetWorkMatrixSystemBuilder() => new WorkMatrixCoorSystemBuilder<TPlaceEnum>();
        public RelatedSystemBuilder<TPlaceEnum> BuildRelatedSystem()
        {
            return new RelatedSystemBuilder<TPlaceEnum>(_workTransformation.ConvertMatrix(), this);
        }
        public class ThreePointCoorSystemBuilder<TPlace> where TPlace : Enum
        {
            private (PointF originPoint, PointF derivativePoint) _firstPair;
            private (PointF originPoint, PointF derivativePoint) _secondPair;
            private (PointF originPoint, PointF derivativePoint) _thirdPair;
            private Matrix3 _workMatrix = new Matrix3(m11: 1, m12: 0, m13: 0,
                                                      m21: 0, m22: 1, m23: 0,
                                                      m31: 0, m32: 0, m33: 1);

            private bool _isWorkMatrixFormed = false;

            public ThreePointCoorSystemBuilder<TPlace> SetFirstPointPair(PointF originPoint, PointF derivativePoint)
            {
                _firstPair = (originPoint, derivativePoint);
                return this;
            }
            public ThreePointCoorSystemBuilder<TPlace> SetSecondPointPair(PointF originPoint, PointF derivativePoint)
            {
                _secondPair = (originPoint, derivativePoint);
                return this;
            }
            public ThreePointCoorSystemBuilder<TPlace> SetThirdPointPair(PointF originPoint, PointF derivativePoint)
            {
                _thirdPair = (originPoint, derivativePoint);
                return this;
            }
            public ThreePointCoorSystemBuilder<TPlace> FormWorkMatrix(double xScaleMul, double yScaleMul, bool pureDeformation)
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

                if (!pureDeformation)
                {
                    _workMatrix = _mainTransformation;
                }
                else
                {
                    var pairs = new[] { first, second, third };

                    var zeroPointPairs =
                        pairs.Where(pair => pair.originPoint == new PointF(0, 0));

                    Guard.IsTrue(zeroPointPairs.Count() == 1, nameof(zeroPointPairs), "Origin points must have one (0,0) point");


                    var anglePoints = pairs.Where(point => point.originPoint.Y == 0)
                                           .OrderBy(point => Math.Abs(point.originPoint.X))
                                           .ToArray();

                    var hasAmgle = anglePoints.Count() == 2;

                    Guard.IsTrue(hasAmgle, nameof(hasAmgle), "Origin points must have two points with same ordinate");

                    var zeroPointPair = zeroPointPairs.Single();

                    var angle = Math.Atan2(anglePoints[1].derivativePoint.Y - anglePoints[0].derivativePoint.Y, anglePoints[1].derivativePoint.X - anglePoints[0].derivativePoint.X);

                    var R = new Matrix3(m11: Math.Cos(angle), m12: -Math.Sin(angle), m13: 0,
                                        m21: Math.Sin(angle), m22: Math.Cos(angle), m23: 0,
                                        m31: 0, m32: 0, m33: 1);

                    var deltaX = zeroPointPair.derivativePoint.X;
                    var deltaY = zeroPointPair.derivativePoint.Y;

                    var Translate = new Matrix3(m11: 1, m12: 0, m13: deltaX,
                                                m21: 0, m22: 1, m23: deltaY,
                                                m31: 0, m32: 0, m33: 1);

                    var S = new Matrix3(m11: xScaleMul, m12: 0, m13: 0,
                                        m21: 0, m22: yScaleMul, m23: 0,
                                        m31: 0, m32: 0, m33: 1);

                    var X = R.Inverse() * Translate.Inverse() * _mainTransformation * S.Inverse();
                    _workMatrix = X;
                }
                _isWorkMatrixFormed = true;
                return this;
            }

            public CoorSystem<TPlace> Build()
            {
                return new CoorSystem<TPlace>(_workMatrix);
            }
        }
        public class WorkMatrixCoorSystemBuilder<TPlace> where TPlace : Enum
        {
            private Matrix3 _workMatrix;

            public WorkMatrixCoorSystemBuilder<TPlace> SetWorkMatrix(Matrix3x2 workMatrix)
            {

                _workMatrix = workMatrix.ConvertMatrix();
                return this;
            }
            public CoorSystem<TPlace> Build()
            {
                return new CoorSystem<TPlace>(_workMatrix);
            }
        }
        public class RelatedSystemBuilder<TPlace> where TPlace : Enum
        {
            private Matrix3 _mainMatrix;
            private readonly CoorSystem<TPlace> _parentSystem;

            public RelatedSystemBuilder(Matrix3x2 mainMatrix, CoorSystem<TPlace> parentSystem)
            {
                _mainMatrix = mainMatrix.ConvertMatrix();
                _parentSystem = parentSystem;
            }
            /// <summary>
            /// Rotate initial matrix by the angle
            /// </summary>
            /// <param name="angle">Rotation angle in radian</param>
            /// <returns>RelatedSystemBuilder</returns>
            public RelatedSystemBuilder<TPlace> Rotate(double angle)
            {
                var R = new Matrix3(m11: Math.Cos(angle), m12: -Math.Sin(angle), m13: 0,
                                    m21: Math.Sin(angle), m22: Math.Cos(angle), m23: 0,
                                    m31: 0, m32: 0, m33: 1);
                _mainMatrix = R * _mainMatrix;
                return this;
            }
            public RelatedSystemBuilder<TPlace> Translate(double offsetX, double offsetY)
            {
                var Translate = new Matrix3(m11: 1, m12: 0, m13: offsetX,
                                            m21: 0, m22: 1, m23: offsetY,
                                            m31: 0, m32: 0, m33: 1);
                _mainMatrix = Translate * _mainMatrix;
                return this;
            }
            public RelatedSystemBuilder<TPlace> Scale(double scale)
            {
                var Translate = new Matrix3(m11: scale, m12: 0, m13: 0,
                                                  m21: 0, m22: scale, m23: 0,
                                                  m31: 0, m32: 0, m33: 1);
                _mainMatrix = Translate * _mainMatrix;
                return this;
            }
            public void Build(TPlace place)
            {
                _parentSystem.SetRelatedSystem(place, _mainMatrix.ConvertMatrix());
            }
        }
        public CoorSystem<TPlaceEnum> WithAxes(bool negX, bool negY)
        {
            _axisXNeg = negX;
            _axisYNeg = negY;
            return this;
        }
        public ICoorSystem<TPlaceEnum> ExtractSubSystem(TPlaceEnum from) => (CoorSystem<TPlaceEnum>)_subSystems[from].Clone();
        public object Clone()
        {
            var copy = new CoorSystem<TPlaceEnum>(_workTransformation);
            copy._axisXNeg = _axisXNeg;
            copy._axisYNeg = _axisYNeg;
            copy._axisXNeg = _axisXNeg;
            copy._subSystems = _subSystems;

            return copy;
        }
    }
}

