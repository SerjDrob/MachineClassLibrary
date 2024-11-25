using System.CodeDom;
using System.Windows;

namespace MachineClassLibrary.Laser.Entities
{
    public class Circle:IShape
    {
        public double Radius { get; set; }
        public double CenterX
        {
            get;
            set;
        }
        public double CenterY
        {
            get;
            set;
        }
        public Rect Bounds
        {
            get; init;
        }//=> new(new System.Windows.Point(CenterX - Radius,CenterY - Radius),new Size(Radius*2,Radius*2));

        public void Deconstruct(out IShape[] primaryShape, out int num)
        {
            primaryShape = [this];
            num = 1;
        }

        public static bool operator ==(Circle a, Circle b) => a.Radius == b.Radius;
        public static bool operator !=(Circle a, Circle b) => !(a == b);
    }
}
