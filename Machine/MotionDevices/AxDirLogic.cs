using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Advantech.Motion;

namespace MachineClassLibrary.Machine.MotionDevices
{
    public enum AxDirLogic
    {
        DIR_ACT_LOW = (int)DirLogic.DIR_ACT_LOW,
        DIR_ACT_HIGH = (int)DirLogic.DIR_ACT_HIGH,
        NOT_SUPPORT = (int)DirLogic.NOT_SUPPORT
    }
}
