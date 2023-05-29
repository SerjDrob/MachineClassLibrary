using MachineClassLibrary.SFC;
using System;

namespace MachineClassLibrary.Machine
{
    public interface IHasSCF
    {
        public bool TryConnectSpindle();
        public void SetSpindleFreq(int frequency);
        public void StartSpindle(params Sensors[] blockers);
        public void StopSpindle();
        public event EventHandler<SpindleEventArgs> OnSpindleStateChanging;
    }
}