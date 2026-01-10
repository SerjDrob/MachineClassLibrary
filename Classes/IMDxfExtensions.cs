using System.Windows;
using System.Windows.Media;
using IxMilia.Dxf;
using IxMilia.Dxf.Entities;

namespace MachineClassLibrary.Classes;

internal static class IMDxfExtensions
{
    public static DxfEllipse ToEllipse(this DxfCircle dxfCircle)
    {
        return new DxfEllipse(dxfCircle.Center, new DxfVector(dxfCircle.Radius, 0, 0), 1)
        {
            Color = dxfCircle.Color,
            Color24Bit = dxfCircle.Color24Bit,  
            ColorName = dxfCircle.ColorName,
            Layer = dxfCircle.Layer
        };
    }

    public static Point ToPoint(this DxfPoint dxfPoint) => new Point(dxfPoint.X, dxfPoint.Y);

    public static EllipseGeometry ToEllipseGeometry(this DxfCircle dxfCircle) => 
        new EllipseGeometry(dxfCircle.Center.ToPoint(), dxfCircle.Radius, dxfCircle.Radius);
}
