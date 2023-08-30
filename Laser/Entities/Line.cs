using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MachineClassLibrary.Laser.Entities
{
    public class Line:IShape
    {
        public double X1 { get; set; }
        public double X2 { get; set; }
        public double Y1 { get; set; }
        public double Y2 { get; set; }

        public Rect Bounds
        {
            get;init;
        }//=> new Rect(X1, Y1, X2 - X1, Y2 - Y1);
    }
}
