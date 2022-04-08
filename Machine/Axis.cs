using System.Collections.Generic;

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
        
        private bool _motionDone = true;
        public bool MotionDone
        {
            get => _motionDone;
            set
            {
                _motionDone = value ? value : _motionDone;
            }
        }
        public bool HomeDone { get; set; }
        private bool _vhStart;
        public bool VHStart
        {
            get => _vhStart;
            set
            {
                _vhStart = value;
                _motionDone = value ? false : _motionDone;
            }
        }
        public bool VHEnd { get; set; }
        public bool Compared { get; set; }
        public bool SetMotionStarted()
        {
            _motionDone = false;
            return true;
        }
        public bool SetMotionDone()
        {
            _motionDone = true;
            return true;
        }
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
