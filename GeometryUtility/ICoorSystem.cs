using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using MachineClassLibrary.Classes;
using netDxf;

namespace MachineClassLibrary.GeometryUtility
{
    public interface ICoorSystem<TPlaceEnum> : ICoorSystem where TPlaceEnum : Enum
    {
        double[] FromSub(TPlaceEnum from, double x, double y);
        void SetRelatedSystem(TPlaceEnum name, double offsetX, double offsetY);
        void SetRelatedSystem(TPlaceEnum name, Matrix3x2 matrix);
        double[] ToSub(TPlaceEnum to, double x, double y);
        ICoorSystem<TPlaceEnum> ExtractSubSystem(TPlaceEnum from);
        ICoorSystem<TPlaceEnum> WithAxes(bool negX, bool negY);
    }

    public interface ICoorSystem : ICloneable
    {
        double[] FromGlobal(double x, double y);
        float[] GetMainMatrixElements();
        double[] ToGlobal(double x, double y);
    }



    public class VariableCoorSystem : ICoorSystem<LMPlace>
    {
        private readonly ICoorSystem<LMPlace> _coorSystem;
        private readonly CoeffLine _xLine;
        private readonly CoeffLine _yLine;

        public VariableCoorSystem(ICoorSystem<LMPlace> coorSystem,
                                    (double orig, double derived)[] xTransPoints,
                                    (double orig, double derived)[] yTransPoints)
        {
            _coorSystem = coorSystem;
            _xLine = new CoeffLine(xTransPoints);
            _yLine = new CoeffLine(yTransPoints);
        }

        public object Clone() => _coorSystem.Clone();
        public ICoorSystem<LMPlace> ExtractSubSystem(LMPlace from) => _coorSystem.ExtractSubSystem(from);
        public double[] FromGlobal(double x, double y) => _coorSystem.FromGlobal(x,y);
        public double[] FromSub(LMPlace from, double x, double y) => _coorSystem.FromSub(from,x,y);
        public float[] GetMainMatrixElements() => _coorSystem.GetMainMatrixElements();
        public void SetRelatedSystem(LMPlace name, double offsetX, double offsetY) => _coorSystem.SetRelatedSystem(name,offsetX,offsetY);
        public void SetRelatedSystem(LMPlace name, Matrix3x2 matrix) => _coorSystem.SetRelatedSystem(name,matrix);
        public double[] ToGlobal(double x, double y)
        {
            var result = _coorSystem.ToGlobal(x,y);
            var xres = _xLine[result[0]];
            var yres = _yLine[result[1]];
            return new double[] { xres, yres };
        }
        public double[] ToSub(LMPlace to, double x, double y) => _coorSystem.ToSub(to,x,y);
        public ICoorSystem<LMPlace> WithAxes(bool negX, bool negY) => _coorSystem.WithAxes(negX,negY);



        class CoeffLine
        {
            private readonly double[] originPoints;
            private readonly double[] derivedPoints;

            public CoeffLine(params (double orig, double derived)[] values)
            {
                if (values.Length < 2) throw new ArgumentException("points count must be no less than two");
                var res = values.OrderBy(val => val.orig);
                originPoints = res.Select(val => val.orig).ToArray();
                derivedPoints = res.Select(val => val.derived).ToArray();
            }

            public double this[double par]
            {
                get
                {
                    var range = GetRange(originPoints, par);
                    var y0 = derivedPoints[range.Start];
                    var y1 = derivedPoints[range.End];
                    var x0 = originPoints[range.Start];
                    var x1 = originPoints[range.End];
                    return (y1 - y0) * ((par - x0) / (x1 - x0)) + y0;
                }
            }

            private static Range GetRange(double[] points, double checkVal)
            {
                if (checkVal < points[0] || checkVal > points[^1] || points.Length == 2) return new Range(0, ^1);
                for (int i = 0; i < points.Length - 1; i++)
                {
                    if (checkVal >= points[i] && checkVal < points[i + 1]) return new Range(i, i + 1);
                }
                throw new InvalidOperationException("A range was not found");
            }
        }

    }
}