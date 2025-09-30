using MachineClassLibrary.SFC;
using System;
using System.Threading.Tasks;

namespace MachineClassLibrary.Machine
{
    public interface IHasSCF
    {
        public bool TryConnectSpindle();
        public void SetSpindleFreq(int frequency);
        public Task<bool> ChangeSpindleFreqOnFlyAsync(ushort rpm, TimeSpan delay);
        public void StartSpindle(params Sensors[] blockers);
        public void StopSpindle();
        public event EventHandler<SpindleEventArgs> OnSpindleStateChanging;
    }
}
