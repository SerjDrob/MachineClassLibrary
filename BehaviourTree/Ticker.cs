using System;
using System.Threading.Tasks;

namespace MachineClassLibrary.BehaviourTree
{
    public class Ticker : WorkerBase, ISequence<Ticker>
    {
        private readonly int cycles;
        public Ticker() : base()
        {

        }
        public Ticker(int cycles)
        {
            this.cycles = cycles;
        }

        public event Action<bool> Pulse;
        public event Action<bool> Cancell;

        private WorkerBase _worker;

        public override async Task<bool> DoWork()
        {
            var loopCount = this.cycles;
            if (!_isCancelled)
            {
                base.DoWork();
                while (_notBlocked && !_isCancelled & (cycles > 0 ? loopCount-- > 0 : true))
                {
                    try
                    {
                        await _worker.DoWork();
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public Ticker Hire(WorkerBase worker)
        {
            worker.GiveMeName(false, $"{_name}.1");
            Pulse += worker.PulseAction;
            Cancell += worker.CancellAction;
            _worker = worker;
            return this;
        }
        public override void GiveMeName(bool ascribe, string name)
        {
            base.GiveMeName(ascribe, name);
            _worker?.GiveMeName(false, $"{_name}.1");
        }
        public override void PulseAction(bool info)
        {
            Pulse?.Invoke(info);
        }
        public override Ticker SetActionBeforeWork(Action action)
        {
            return (Ticker)base.SetActionBeforeWork(action);
        }

        public override void CancellAction(bool info)
        {
            _isCancelled = true;
            Cancell?.Invoke(info);
        }
    }
}
