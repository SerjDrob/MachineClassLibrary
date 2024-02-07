using System;

namespace MachineClassLibrary.Machine
{
    [Obsolete]
    public interface IMessager
    {
        public event Action<string, int> ThrowMessage;
    }
}