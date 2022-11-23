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
    }
}
