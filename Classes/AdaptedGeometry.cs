using System.Windows;
using System.Windows.Media;

namespace MachineClassLibrary.Classes
{
    public record AdaptedGeometry
    (
        System.Windows.Media.Geometry geometry,
        string LayerName,
        Brush LayerColor,
        Brush GeometryColor
    );   
}
