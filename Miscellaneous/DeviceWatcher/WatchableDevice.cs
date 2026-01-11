using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace MachineClassLibrary.Miscellaneous;

public abstract class WatchableDevice : IWatchableDevice
{
    private ISubject<IDeviceInfo>? _subject;
    private readonly List<IDisposable> _subscriptions = new();
    private PlugMeWatcher _plugMeWatcher;
    public void HasHealthProblem(string message, Exception exception, object device = null)
    {
        _subject?.OnNext(new HealthProblem(message, exception, device));
    }
    public abstract void CureDevice();
    public abstract void AskHealth();
    public void DeviceOK() => _subject?.OnNext(new HealthOK());
    public void DeviceOK(object device) => _subject?.OnNext(new HealthOK(device));

    public void WatchMe(string vid, string pid, Action doWhenPlugged)
    {
        _plugMeWatcher ??= new(vid, pid);
        _plugMeWatcher.WaitAndPlugMe(doWhenPlugged);
    }

    public void Dispose()
    {
        _subscriptions?.ForEach(x => x.Dispose());
    }

    public IDisposable Subscribe(IObserver<IDeviceInfo> observer)
    {
        _subject ??= new Subject<IDeviceInfo>();
        var subscription = _subject.Subscribe(observer);
        _subscriptions.Add(subscription);
        return subscription;
    }
}
