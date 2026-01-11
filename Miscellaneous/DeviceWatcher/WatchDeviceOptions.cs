using System;

namespace MachineClassLibrary.Miscellaneous;

public class WatchDeviceOptions
{
    public Action<bool>? IsDeviceOkAction { get; set; }
    public Action<string, Exception>? CureDeviceAction { get; set; }
}
