using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace MachineClassLibrary.Miscellaneous;

//public class DeviceWatcher : IDisposable
//{
//    private Dictionary<string, WatchableDevice> _devices;
//    private bool disposedValue;

//    public void AddDevice(IWatchableDevice watchableDevice, string deviceName, Action<WatchDeviceOptions> options)
//    {
//        _devices ??= new();
//        if (_devices.Keys.Contains(deviceName)) throw new ArgumentException($"{deviceName} has already added.");
//        var sbs = new List<IDisposable>();
//        watchableDevice.OfType<HealthOK>()
//            .Subscribe(ok =>
//            {
//                var wdOptions = new WatchDeviceOptions();
//                options?.Invoke(wdOptions);
//                wdOptions.IsDeviceOkAction?.Invoke(true);
//            }).AddSubscriptionTo(sbs);

//        watchableDevice.OfType<HealthProblem>()
//            .Subscribe(problem =>
//            {
//                var wdOptions = new WatchDeviceOptions();
//                options?.Invoke(wdOptions);
//                wdOptions.CureDeviceAction?.Invoke(problem.Message, problem.Exception);
//            }).AddSubscriptionTo(sbs);

//        _devices[deviceName] = new WatchableDevice(watchableDevice, sbs);
//    }

//    public IWatchableDevice this[string name] => _devices[name].Device;

//    private record WatchableDevice(IWatchableDevice Device, List<IDisposable> Subscriptions);

//    protected virtual void Dispose(bool disposing)
//    {
//        if (!disposedValue)
//        {
//            if (disposing)
//            {
//                // TODO: dispose managed state (managed objects)

//                foreach (var item in _devices.Values)
//                {
//                    item.Subscriptions.ForEach(s => s.Dispose());
//                }
//            }

//            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
//            // TODO: set large fields to null
//            disposedValue = true;
//        }
//    }

//    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
//    // ~DeviceWatcher()
//    // {
//    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
//    //     Dispose(disposing: false);
//    // }

//    public void Dispose()
//    {
//        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
//        Dispose(disposing: true);
//        GC.SuppressFinalize(this);
//    }
//}
