using System;
using System.Collections.Generic;
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
        public static FuncTree SetFunc(Func<Task> func) => new FuncTree(func);
        public static MakeLoop StartLoop(int count) => new MakeLoop(count);
        public class MakeLoop
        {
            public MakeLoop(int count)
            {
                _count = count;
            }
            private List<FuncTree> _children = new();
            private readonly int _count;

            public MakeLoop AddChild(FuncTree child)
            {
                _children.Add(child);
                return this;
            }
            public FuncTree EndLoop
            {
                get => new FuncTree(async () =>
                {
                    for (int i = 0; i < _count; i++)
                    {
                        foreach (var item in _children)
                        {
                            await item.DoFuncAsync();
                        }
                    }
                });
            }
        }
    }    
}
