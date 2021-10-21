using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineClassLibrary.Machine
{
    public class Axis : IAxis
    {
        public Axis(double lineCoefficient, int axisNum)
        {
            LineCoefficient = lineCoefficient;
            AxisNum = axisNum;
        }
        public int AxisNum { get; }

        public double LineCoefficient { get; }

        public bool LmtP { get; set; }
        public bool LmtN { get; set; }
        public double CmdPosition { get; set; }
        public double ActualPosition { get; set; }
        public int Ppu { get; set; }
        public bool MotionDone { get; set; }
        public bool HomeDone { get; set; }
        public bool Compared { get; set; }
        public int DIs { get; set; }
        public int DOs { get; set; }
        public Dictionary<Velocity, double> VelRegimes { get; set; }
        public bool GetDi(Di din)
        {
            var res = (DIs & 1 << (int)din) != 0;
            return res;
        }

        public bool GetDo(Do dout)
        {
            return (DOs & 1 << (int)dout) != 0;
        }

        public bool Busy { get; set; } = false;
    }
}
