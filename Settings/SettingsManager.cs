using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using MachineClassLibrary.Miscellaneous;

namespace MachineClassLibrary.Settings
{
    public class SettingsManager<T> : ISettingsManager<T>
    {
        private readonly string _settingsPath;
        private ISubject<T> _subject;
        private List<IDisposable> _subscriptions;
        private bool disposedValue;

        public T? Settings
        {
            get;
            private set;
        }
        public SettingsManager(string settingsPath) => _settingsPath = settingsPath;
        public void Load()
        {
            try
            {
                var deserializer = new JsonDeserializer<T>();
                Settings = deserializer.DeserializeFromFile(_settingsPath);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void Save()
        {
            try
            {
                Settings?.SerializeObject(_settingsPath);
                _subject?.OnNext(Settings!);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void SetSettings(T settings)
        {
            Settings = settings;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            _subject ??= new Subject<T>();
            _subscriptions ??= new();
            var subscription = _subject.Subscribe(observer);
            _subscriptions.Add(subscription);
            return subscription;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _subscriptions.ForEach(s => s.Dispose());
                    _subscriptions.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SettingsManager()
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
}
