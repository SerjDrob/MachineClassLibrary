using MachineClassLibrary.Classes;
using MachineClassLibrary.SFC;
using MachineClassLibrary.VideoCapture;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MachineClassLibrary.Machine
{

    //public delegate void AxisStateHandler(int axisNum, AxisState state);

    //public delegate void SensorStateHandler(Sensors sensor, bool state);

    //public delegate void ValveStateHandler(Valves valve, bool state);

    //public delegate void AxisMotioStateHandler(Ax axis, double position, bool nLmt, bool pLmt, bool motionDone,
    //    bool motionStart);
    public interface IHasLaser
    {
        public void TurnOnLaser();
        public void TurnOffLaser();
    }
}