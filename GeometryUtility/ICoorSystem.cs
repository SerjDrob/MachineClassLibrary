using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;

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

    public interface ICoorSystem: ICloneable
    {
        double[] FromGlobal(double x, double y);
        float[] GetMainMatrixElements();
        double[] ToGlobal(double x, double y);
    }
}