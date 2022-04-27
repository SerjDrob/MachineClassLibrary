using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineClassLibrary.Laser.Entities
{
    public class Line:IShape
    {
        public double X1 { get; set; }
        public double X2 { get; set; }
        public double Y1 { get; set; }
        public double Y2 { get; set; }

        //public void Scale(double scale)
        //{
        //    X1 *= scale;
        //    X2 *= scale;
        //    Y1 *= scale;
        //    Y2 *= scale;
        //}

        //public void SetMirrorX(bool mirror)
        //{
        //   // throw new NotImplementedException();
        //}

        //public void SetTurn90(bool turn)
        //{
        //   // throw new NotImplementedException();
        //}
    }
}
