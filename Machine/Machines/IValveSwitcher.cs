using MachineClassLibrary.Classes;
using MachineClassLibrary.Machine.MotionDevices;
using System;



namespace MachineClassLibrary.Machine.Machines
{
    public interface IValveSwitcher
    {
        bool GetValveState(IMotionDevicePCI1240U motionDevice, Valves valve, Func<Ax, int> getAxNum);
        void SwitchValve(IMotionDevicePCI1240U motionDevice, Valves valve, bool val, Func<Ax,int> getAxNum);
    }
}
