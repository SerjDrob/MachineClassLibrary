using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MachineClassLibrary.Miscellaneous
{
    public class DeviceWatcher : IDisposable
    {
        private Dictionary<string, WatchableDevice> _devices;
        private bool disposedValue;

        public void AddDevice(IWatchableDevice watchableDevice, string deviceName, Action<WatchDeviceOptions> options)
        {
            _devices ??= new();
            if (_devices.Keys.Contains(deviceName)) throw new ArgumentException($"{deviceName} has already added.");
            var sbs = new List<IDisposable>();
            watchableDevice.OfType<HealthOK>()
                .Subscribe(ok =>
                {
                    var wdOptions = new WatchDeviceOptions();
                    options?.Invoke(wdOptions);
                    wdOptions.IsDeviceOkAction?.Invoke(true);
                }).AddSubscriptionTo(sbs);

            watchableDevice.OfType<HealthProblem>()
                .Subscribe(problem =>
                {
                    var wdOptions = new WatchDeviceOptions();
                    options?.Invoke(wdOptions);
                    wdOptions.CureDeviceAction?.Invoke(problem.Message, problem.Exception);
                }).AddSubscriptionTo(sbs);

            _devices[deviceName] = new WatchableDevice(watchableDevice, sbs);
        }

        public IWatchableDevice this[string name] => _devices[name].Device;

        private record WatchableDevice(IWatchableDevice Device, List<IDisposable> Subscriptions);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)

                    foreach (var item in _devices.Values)
                    {
                        item.Subscriptions.ForEach(s => s.Dispose());
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DeviceWatcher()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class WatchDeviceOptions
    {
        public Action<bool>? IsDeviceOkAction { get; set; }
        public Action<string, Exception>? CureDeviceAction { get; set; }
    }

    public interface IDeviceInfo { }
    public record HealthProblem(string Message, Exception Exception, object Device = null) : IDeviceInfo;
    public record HealthOK(object Device = null) : IDeviceInfo;

    public interface IWatchableDevice : IObservable<IDeviceInfo>, IDisposable
    {
        void AskHealth();
        void CureDevice();
        void DeviceOK(object device);
        void HasHealthProblem(string message, Exception exception, object deviceType);
    }

    public abstract class WatchableDevice : IWatchableDevice
    {
        private ISubject<IDeviceInfo>? _subject;
        private List<IDisposable>? _subscriptions;
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
            _subscriptions ??= new List<IDisposable>();
            var subscription = _subject.Subscribe(observer);
            _subscriptions.Add(subscription);
            return subscription;
        }
    }

    internal static class WatchDeviceExtensions
    {
        internal static void AddSubscriptionTo(this IDisposable subscription, IList<IDisposable> subscriptions) => subscriptions?.Add(subscription);
    }
}
