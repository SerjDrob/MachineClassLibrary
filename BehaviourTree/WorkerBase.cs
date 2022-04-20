using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MachineClassLibrary.BehaviourTree
{
    public abstract class WorkerBase
    {
        protected string _name = "1";
        protected bool _isCancelled = false;
        public virtual void GiveMeName(bool ascribe, string name)
        {
            if (ascribe)
            {
                _name = $"{name}.{_name}";
            }
            else
            {
                _name = name;
            }

        }
        private event Action ActionBeforeWork;
        public virtual WorkerBase SetActionBeforeWork(Action action)
        {
            //if (!_isCancelled)
            //{
            //    ActionBeforeWork?.Invoke();
            //}

            ActionBeforeWork += action;

            return this;
        }

        public virtual Task<bool> DoWorkAsync()
        {
            ActionBeforeWork?.Invoke();
            return Task.FromResult(true);
        }
        public abstract void PulseAction(bool info);
        public abstract void CancellAction(bool info);

        public virtual WorkerBase SetBlock(Block block)
        {
            _blocks.Add(block);
            return this;
        }
        private List<Block> _blocks = new();
        protected bool _notBlocked => _blocks.All(b => b.NotBlocked);
    }
}
