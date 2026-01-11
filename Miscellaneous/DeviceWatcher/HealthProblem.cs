using System;

namespace MachineClassLibrary.Miscellaneous;

public record HealthProblem(string Message, Exception Exception, object Device = null) : IDeviceInfo;
