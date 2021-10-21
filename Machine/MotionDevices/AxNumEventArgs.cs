using System;

namespace MachineClassLibrary.Machine.MotionDevices
{
    public class AxNumEventArgs : EventArgs
    {
        public AxNumEventArgs(int axisNum, AxisState axisState)
        {
            AxisNum = axisNum;
            AxisState = axisState;
        }

        public int AxisNum { get; init; }
        public AxisState AxisState { get; init; }
    }
}