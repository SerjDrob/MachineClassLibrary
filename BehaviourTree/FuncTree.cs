using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MachineClassLibrary.BehaviourTree
{
    public class FuncTree
    {
        private readonly Func<Task> _myTask;
        public async Task DoFuncAsync() => await _myTask.Invoke().ConfigureAwait(false);
        public Func<Task> GetFunc() => _myTask;
        protected FuncTree(Func<Task> func)
        {
            _myTask = func;
        }
        public static FuncTree SetFunc(Func<Task> func) => new(func);
        public static MakeLoop StartLoop(int count, CancellationToken cancellationToken = new()) => new(count, cancellationToken);
        public class MakeLoop
        {
            private readonly List<FuncTree> _children = new();
            private readonly int _count;
            private readonly CancellationToken _cancellationToken;
            public MakeLoop(int count, CancellationToken cancellationToken)
            {
                _cancellationToken = cancellationToken;
                _count = count;
            }
            public MakeLoop AddChild(FuncTree child)
            {
                _children.Add(child);
                return this;
            }
            public FuncTree EndLoop => new(async () =>
            {
                if (_cancellationToken.IsCancellationRequested) return;
                for (var i = 0; i < _count; i++)
                {
                    if (_cancellationToken.IsCancellationRequested) return;
                    foreach (var item in _children)
                    {
                        if (_cancellationToken.IsCancellationRequested) return;
                        await item.DoFuncAsync();
                    }
                }
            });
        }
    }    
}
