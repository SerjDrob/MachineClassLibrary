using System;

namespace MachineClassLibrary.BehaviourTree
{
    public interface ISequence<T> where T : class
    {
        T Hire(WorkerBase worker);
        event Action<bool> Pulse;
        event Action<bool> Cancell;
    }
}
