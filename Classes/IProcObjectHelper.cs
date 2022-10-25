using MachineClassLibrary.Laser.Entities;
using System.Windows;

namespace MachineClassLibrary.Classes
{
    internal static class IProcObjectHelper
    {
        public static Rect GetBoundingBox(this IProcObject procObject)
        {
            var size = procObject.GetSize();
            return new Rect(
                procObject.X - size.x / 2,
                procObject.Y + size.y / 2,
                size.x,
                size.y);
        }
    }

}
