using System;

namespace MachineClassLibrary.Settings
{
    public interface ISettingsManager<TSettings>: IObservable<TSettings>, IDisposable
    {
        TSettings Settings { get; }
        void Save();
        void SetSettings(TSettings settings);
        void Load();
    }
}
