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
    }
}
