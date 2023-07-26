using Microsoft.Toolkit.Diagnostics;
using netDxf;
using System;

namespace MachineClassLibrary.GeometryUtility
{
    public static class CoorSystemExtensions
    {
        public static double GetMatrixAngle(this ICoorSystem coorSystem)
        {
            var point1 = coorSystem.ToGlobal(0, 0);
            var point2 = coorSystem.ToGlobal(100, 0);
            return Math.Atan2(point2[1] - point1[1], point2[0] - point1[0]);
        }

        public static double GetMatrixAngle2(this ICoorSystem coorSystem)
        {
            var point1 = coorSystem.ToGlobal(0, 0);
            var point2 = coorSystem.ToGlobal(100, 0);
            return Math.Atan((point2[1] - point1[1])/(point2[0] - point1[0]));
        }

        public static Matrix3 GetMatrix3(this float[] elements)
        {
            Guard.HasSizeEqualTo<float>(elements, 6, "The array must have 6 elements");

            return new Matrix3(elements[0], elements[1],0,
                               elements[2], elements[3],0,
                               elements[4], elements[5],1);
        }
    }
}
