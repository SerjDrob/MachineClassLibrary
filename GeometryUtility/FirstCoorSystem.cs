using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineClassLibrary.GeometryUtility
{
    public class FirstCoorSystem<TPlaceEnum> : IDisposable, ICoorSystem<TPlaceEnum> where TPlaceEnum : Enum
    {
        private Dictionary<TPlaceEnum, CoorSystem<TPlaceEnum>> _subSystems = new();
        private readonly Matrix _mainMatrix;
        private Matrix _skewMatrix;
        private Matrix3 _matrix3;
        public CoorSystem(Matrix mainMatrix)
        {
            _mainMatrix = mainMatrix;
        }
        /// <summary>
        /// Instantiates a new object of CoorSystem by accordance of three pairs of points.
        /// </summary>
        /// <param name="first">First pair of points. Item1 and Item2 are "point from" and "point to" respectively</param>
        /// <param name="second">Second pair of points. Item1 and Item2 are "point from" and "point to" respectively</param>
        /// <param name="third">Third pair of points. Item1 and Item2 are "point from" and "point to" respectively</param>
        public CoorSystem((PointF, PointF) first, (PointF, PointF) second, (PointF, PointF) third)
        {
            //var list = new List<(PointF, PointF)> { first, second, third };

            //var width = list.MaxBy(p => p.Item1.X).Item1.X - list.MinBy(p => p.Item1.X).Item1.X;
            //var height = list.MaxBy(p => p.Item1.Y).Item1.Y - list.MinBy(p => p.Item1.Y).Item1.Y;


            //var size = new SizeF(width, -height);

            //_mainMatrix = new Matrix(new RectangleF(list[0].Item1, size), new[] { list[0].Item2, list[1].Item2, list[2].Item2 });



            var initialPoints = new Matrix3(m11: first.Item1.X, m12: first.Item1.Y, m13: 1,
                                            m21: second.Item1.X, m22: second.Item1.Y, m23: 1,
                                            m31: third.Item1.X, m32: third.Item1.Y, m33: 1);


            var transformedPoints = new Matrix3(m11: first.Item2.X, m12: first.Item2.Y, m13: 1,
                                                m21: second.Item2.X, m22: second.Item2.Y, m23: 1,
                                                m31: third.Item2.X, m32: third.Item2.Y, m33: 1);

            var invert = initialPoints.Inverse();
            var transformation = invert * transformedPoints;

            transformation = transformation.Transpose();
            _matrix3 = transformation;
            var matrix = new Matrix3x2(m11: (float)transformation.M11, m12: (float)transformation.M12,
                                       m21: (float)transformation.M21, m22: (float)transformation.M22,
                                       m31: (float)transformation.M13, m32: (float)transformation.M23);

            _mainMatrix = new Matrix(matrix);


            var scaleX = Math.Sqrt(Math.Pow(transformation.M11, 2) + Math.Pow(transformation.M12, 2));
            var scaleY = -Math.Sqrt(transformation.M11 * transformation.M22 - transformation.M12 * transformation.M21) / scaleX;
            var shearY = Math.Atan2(transformation.M11 * transformation.M21 + transformation.M12 * transformation.M22, transformation.M11 * transformation.M11 + transformation.M12 * transformation.M12);
            var rotating = Math.Atan2(transformation.M12, transformation.M11);
            var translationX = transformation.M13;
            var translationY = transformation.M23;
            _skewMatrix = new Matrix(new Matrix3x2());
        }

        public record Transformations(Matrix3x2 Transformation, Matrix3x2 Skew, Matrix3x2 Rotation, Matrix3x2 Translation);
        public Transformations GetTransform((PointF, PointF) first, (PointF, PointF) second, (PointF, PointF) third)
        {
            var m1 = new Matrix3(first.Item1.X, first.Item1.Y, 1, second.Item1.X, second.Item1.Y, 1, third.Item1.X, third.Item1.Y, 1);
            var m2 = new Matrix3(first.Item2.X, first.Item2.Y, 1, second.Item2.X, second.Item2.Y, 1, third.Item2.X, third.Item2.Y, 1);
            var invert = m1.Inverse();
            var transformation = invert * m2;
            transformation = transformation.Transpose();
            var matrix = new Matrix3x2((float)transformation.M11, (float)transformation.M12, (float)transformation.M21, (float)transformation.M22, (float)transformation.M13, (float)transformation.M23);
            var mainTrans = new Matrix(matrix);


            var scaleX = Math.Sqrt(Math.Pow(transformation.M11, 2) + Math.Pow(transformation.M12, 2));
            var scaleY = -Math.Sqrt(transformation.M11 * transformation.M22 - transformation.M12 * transformation.M21) / scaleX;
            var shearY = Math.Atan2(transformation.M11 * transformation.M21 + transformation.M12 * transformation.M22, transformation.M11 * transformation.M11 + transformation.M12 * transformation.M12);
            var rotating = Math.Atan2(transformation.M12, transformation.M11);
            var translationX = transformation.M13;
            var translationY = transformation.M23;
            var skewTrans = new Matrix(new Matrix3x2());
            throw new NotImplementedException();
        }
        public Matrix3x2 GetMainMatrix() => _mainMatrix.MatrixElements;
        public void SetRelatedSystem(TPlaceEnum name, Matrix matrix)
        {
            matrix.Multiply(_mainMatrix, MatrixOrder.Append);
            var sub = new CoorSystem<TPlaceEnum>(matrix);
            //_subSystems = new();
            if (!_subSystems.TryAdd(name, sub))
            {
                _subSystems[name] = sub;
            }
        }
        public void SetRelatedSystem(TPlaceEnum name, double offsetX, double offsetY)
        {
            var matrix = _mainMatrix.Clone();
            matrix.Translate((float)offsetX, (float)offsetY);
            var sub = new CoorSystem<TPlaceEnum>(matrix);
            //_subSystems = new();
            if (!_subSystems.TryAdd(name, sub))
            {
                _subSystems[name] = sub;
            }
        }

        public double[] ToGlobal(double x, double y)
        {
            var points = new PointF[] { new((float)x, (float)y) };
            _mainMatrix.TransformPoints(points);
            return new double[2] { points[0].X, points[0].Y };
        }
        public double[] ToGlobal1(double x, double y)
        {
            var vector = new netDxf.Vector3(x, y, 1);
            var result = _matrix3 * vector;
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
            if (_mainMatrix.IsInvertible)
            {
                var points = new PointF[] { new((float)x, (float)y) };
                var matrix = _mainMatrix.Clone();
                matrix.Invert();
                matrix.TransformPoints(points);
                return new double[2] { points[0].X, points[0].Y };
            }
            else
            {
                throw new Exception("System matrix is not invertible");
            }

        }
        public double[] FromSub(TPlaceEnum from, double x, double y)
        {
            Guard.IsNotNull(_subSystems, nameof(_subSystems));
            return _subSystems.ContainsKey(from) ? _subSystems[from].FromGlobal(x, y) : throw new KeyNotFoundException($"Subsystem {from} is not set");
        }
        public float[] GetMainMatrixElements() => _mainMatrix.Elements;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_mainMatrix != null)
                {
                    _mainMatrix.Dispose();
                    //_mainMatrix = null;
                }
            }
        }
    }
}
