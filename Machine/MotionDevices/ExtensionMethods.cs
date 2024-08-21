using Advantech.Motion;
using System;
using System.Text;

namespace MachineClassLibrary.Machine.MotionDevices
{

    internal static class ExtensionMethods
    {
        public static void CheckResult(this uint result, int axisNum = -1)
        {
            if (result != 0)
            {
                var sb = new StringBuilder(50);
                Motion.mAcm_GetErrorMessage(result, sb, 50);
                var axisName = axisNum != -1 ? $"in axis number {axisNum}" : string.Empty;
                
                throw new MotionException($"{sb} Error Code: [0x{result:X}] {axisName}");
            }
        }

        public static void CheckResult2(this uint result, int axisNum = -1)
        {
            if (result != 0)
            {
                var sb = new StringBuilder(50);
                uint state = default;
                Motion2.mAcm2_AxGetState(0,AXIS_STATUS_TYPE.AXIS_STATE, ref state);
                var xstate = (Advantech.Motion.AxisState)state;
                Motion2.mAcm2_AxGetState(1, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
                var ystate = (Advantech.Motion.AxisState)state;
                Motion2.mAcm2_AxGetState(2, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
                var zstate = (Advantech.Motion.AxisState)state;
                Motion2.mAcm2_GetErrorMessage(result, sb, 50);
                var axisName = axisNum != -1 ? $"in axis number {axisNum}" : string.Empty;

               throw new MotionException($"{sb} Error Code: [0x{result:X}] {axisName}");
            }
        }

        public static void CheckResult(this uint result, IntPtr handle)
        {
            if (result != 0 && result != 2147483690)//TODO fix this error result
            {
                ushort state = 0;
                Motion.mAcm_AxGetState(handle, ref state).CheckResult();

                var sb = new StringBuilder(50);
                Motion.mAcm_GetErrorMessage(result, sb, 50);

                var axisName = $"in axis number {handle}";
                throw new MotionException($"{state} Error Code: [0x{result:X}] {axisName}");
            }
        }
    }
}
