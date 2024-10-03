using System.Windows;

namespace MachineClassLibrary.Laser.Entities
{
    public class Point:IShape
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Rect Bounds { get; init; }

        public void Deconstruct(out IShape[] primaryShape, out int num)
        {
            primaryShape = [this];
            num = 1;
        }
    }
}
