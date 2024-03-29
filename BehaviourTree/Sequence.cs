﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MachineClassLibrary.BehaviourTree
{
    public class Sequence : WorkerBase,ISequence<Sequence>
    {
        private List<WorkerBase> _workers = new();
        private int childrenCount = 0;

        public event Action<bool> Pulse;
        public event Action<bool> Cancell;

        public override async Task<bool> DoWorkAsync()
        {
            if (!_isCancelled)
            {
                await base.DoWorkAsync();
                if (_notBlocked)
                {
                    foreach (var worker in _workers)
                    {
                        if (_isCancelled) return true;
                        if (worker is Leaf) worker.PulseAction(true);
                        var res = await worker.DoWorkAsync();
                    }
                }
            }
            return true;
        }
        public Sequence Hire(WorkerBase worker)
        {
            childrenCount++;
            worker.GiveMeName(false, $"{_name}.{childrenCount}");
            Pulse += worker.PulseAction;
            Cancell += worker.CancellAction;
            _workers.Add(worker);
            return this;
        }
        public override void GiveMeName(bool ascribe, string name)
        {
            base.GiveMeName(ascribe, name);
            var i = 1;
            _workers.ForEach(worker =>
            {
                worker.GiveMeName(false, $"{_name}.{i++}");
            });
        }
        public override void PulseAction(bool info)
        {
            Pulse?.Invoke(info);
        }
        public override Sequence SetActionBeforeWork(Action action)
        {
            return (Sequence)base.SetActionBeforeWork(action);
        }
        public override Sequence SetBlock(Block block)
        {
            return (Sequence)base.SetBlock(block);
        }
        /// <summary>
        /// Cancells all hired work
        /// </summary>
        /// <param name="info">true value means to cancell</param>
        public override void CancellAction(bool info)
        {
            _isCancelled = true;
            Cancell?.Invoke(info);
        }
    }    
}
