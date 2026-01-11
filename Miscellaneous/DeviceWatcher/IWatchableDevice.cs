using System;

namespace MachineClassLibrary.Miscellaneous;

public interface IWatchableDevice : IObservable<IDeviceInfo>, IDisposable
{
    void AskHealth();
    void CureDevice();
    void DeviceOK(object device);
    void HasHealthProblem(string message, Exception exception, object deviceType);
}
