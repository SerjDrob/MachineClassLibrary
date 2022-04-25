using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineClassLibrary.Laser.Entities
{
    public class DxfCurve : IShape
    {
        public double Scaling { get; private set; }
        public bool Turn90 { get; private set; }
        public bool MirrorX { get; private set; }
        public void Scale(double scale)
        {
            Scaling = scale;
        }
    }
}
