using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineClassLibrary.Laser.Entities
{
    public class DxfCurve : IShape
    {
        private readonly string _fullPath;
        public string FilePath { get => _fullPath; }
        public DxfCurve(string fullPath)
        {
            _fullPath = fullPath;
        }

        //public double Scaling { get; private set; } = 1;
        //public bool Turn90 { get; private set; } = false;
        //public bool MirrorX { get; private set; } = false;
        //public void Scale(double scale) => Scaling = scale;       
        //public void SetTurn90(bool turn) => Turn90 = turn;
        //public void SetMirrorX(bool mirror) => MirrorX = mirror;

    }
}
