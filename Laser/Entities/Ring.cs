using System.Collections;
using System.Windows;

namespace MachineClassLibrary.Laser.Entities
{
    public class Ring : IShape
    {
        public double Radius1 { get; set;}
        public double Radius2 { get; set; }
        public double CenterX
        {
            get; set;
        }
        public double CenterY
        {
            get;
            set;
        }
        public Rect Bounds => new Rect(CenterX - _biggestR, CenterY - _biggestR, _biggestR * 2, _biggestR * 2);
        private double _biggestR => Radius1 > Radius2 ? Radius1 : Radius2;

        public void Deconstruct(out IShape[] primaryShape, out int num)
        {
            var circle1 = new Circle { Radius = Radius1, CenterX = this.CenterX, CenterY = this.CenterY };
            var circle2 = new Circle { Radius = Radius2, CenterX = this.CenterX, CenterY = this.CenterY };
            primaryShape = new Circle[] { circle1, circle2 };
            num = 2;
        }
    }
}
