using System.Windows;

namespace MachineClassLibrary.Laser.Entities
{
    public class Ring : IShape
    {
        public double Radius1 { get; set;}
        public double Radius2 { get; set; }
        public Rect Bounds { get; init; }
    }
}
