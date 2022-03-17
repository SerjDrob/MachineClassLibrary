using Advantech.Motion;
using System.Text;

namespace MachineClassLibrary.Machine.MotionDevices
{
    internal static class ExtensionMethods
    {
        public static void CheckResult(this uint result)
        {
            if (result != 0)
            {
                var sb = new StringBuilder();
                Motion.mAcm_GetErrorMessage(result, sb, 50);
                throw new MotionException($"{sb} Error Code: [0x{result:X}]");
            }
        }
    }
}
