using MachineClassLibrary.SFC;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MachineClassLibrary.Machine;

public interface IHasSCF
{
    bool TryConnectSpindle();
    void SetSpindleFreq(int frequency);
    Task<bool> ChangeSpindleFreqOnFlyAsync(ushort rpm, TimeSpan delay);
    Task StartSpindleAsync();
    void SetSpindleStartBlocker(Func<(bool canStart, IEnumerable<string> absentSensors)> blocker);
    Task StopSpindleAsync();
    event EventHandler<SpindleEventArgs> OnSpindleStateChanging;
}
