using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineClassLibrary.Machine
{
    public interface IAxis
    {
        public int AxisNum { get; }
        public double LineCoefficient { get; }
        public Dictionary<Velocity, double> VelRegimes { get; set; }
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
        public bool GetDi(Di din);
        public bool GetDo(Do dout);
        public bool Busy { get; set; }
        bool VHStart { get; set; }
        bool VHEnd { get; set; }
        bool SetMotionStarted();
        bool SetMotionDone();
    }
}
