using System;
using System.Collections.Generic;

namespace MachineClassLibrary.BehaviourTree
{
    public class Tree
    {
        private readonly Action _myAction;
        public void DoAction() => _myAction.Invoke();
        protected Tree(Action action)
        {
            _myAction = action;
        }
        public Tree SetAction(Action action) => new Tree(action);
        public MakeLoop StartLoop(int count) => new MakeLoop(count);
        public class MakeLoop
        {
            public MakeLoop(int count)
            {
                _count = count;
            }
            private List<Tree> _children = new();
            private readonly int _count;

            public MakeLoop AddChild(Tree child)
            {
                _children.Add(child);
                return this;
            }
            public Tree EndLoop
            {
                get => new Tree(() =>
                {
                    for (int i = 0; i < _count; i++)
                    {
                        foreach (var item in _children)
                        {
                            item.DoAction();
                        }
                    }
                });
            }
        }
    }
}
