using System;
using System.Linq;

namespace MachineClassLibrary.GeometryUtility
{
    public class CoeffLine
    {
        private double[] originPoints;
        private double[] derivedPoints;

        public CoeffLine(params (double orig, double derived)[] values)
        {
            if (values.Length < 2) throw new ArgumentException("points count must be no less than two");
            var res = values.OrderBy(val => val.orig);
            originPoints = res.Select(val => val.orig).ToArray();
            derivedPoints = res.Select(val => val.derived).ToArray();
        }

        public void AddPoint(double orig, double derived)
        {
            var tempArr = originPoints.Zip(derivedPoints)
                .Append((orig,derived))
                .OrderBy(o=>o.Item1);
            originPoints = tempArr.Select(val => val.Item1).ToArray();
            derivedPoints = tempArr.Select(val => val.Item2).ToArray();
        }
        public (double orig, double derived)[] GetValues() => originPoints.Zip(derivedPoints).ToArray();
        public double this[double par, bool rev = false]
        {
            get
            {
                var (opoints, dpoints) = rev ? (derivedPoints, originPoints) : (originPoints, derivedPoints);
                var range = GetRange(opoints, par);

                if(par == opoints[range.Start]) return dpoints[range.Start];
                if(par == opoints[range.End]) return dpoints[range.End];

                var y0 = dpoints[range.Start];
                var y1 = dpoints[range.End];
                var x0 = opoints[range.Start];
                var x1 = opoints[range.End];
                return (y1 - y0) * ((par - x0) / (x1 - x0)) + y0;
            }
        }

        private static Range GetRange(double[] points, double checkVal)
        {
            if (checkVal < points[0] || checkVal > points[^1] || points.Length == 2) return new Range(0, ^1);
            for (int i = 0; i < points.Length - 1; i++)
            {
                if (checkVal >= points[i] && checkVal <= points[i + 1]) return new Range(i, i + 1);
            }
            throw new InvalidOperationException("A range was not found");
        }
    }

}
