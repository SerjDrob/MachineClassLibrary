using System;

namespace MachineClassLibrary.Machine
{
    public interface IMessager
    {
        public event Action<string, int> ThrowMessage;
    }
}