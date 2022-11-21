using netDxf;
using System.Numerics;

namespace MachineClassLibrary.GeometryUtility
{
    internal static class MatrixConverter
    {
        public static Matrix3 ConvertMatrix(this Matrix3x2 initMatrix)
        {
            return new Matrix3(m11: initMatrix.M11, m12: initMatrix.M12, m13: initMatrix.M31,
                               m21: initMatrix.M21, m22: initMatrix.M22, m23: initMatrix.M32,
                               m31: 0, m32: 0, m33: 1);
        }

        public static Matrix3x2 ConvertMatrix(this Matrix3 initMatrix)
        {
            return new Matrix3x2(m11: (float)initMatrix.M11, m12: (float)initMatrix.M12,
                                 m21: (float)initMatrix.M21, m22: (float)initMatrix.M22,
                                 m31: (float)initMatrix.M13, m32: (float)initMatrix.M23);
        }
    }
}

