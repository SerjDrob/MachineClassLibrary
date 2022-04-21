using System;
using System.Collections.Generic;

namespace MachineClassLibrary.BehaviourTree
{

    public class ActionTree
    {
        private readonly Action _myAction;
        public void DoAction() => _myAction.Invoke();
        public Action GetAction() => _myAction;
        protected ActionTree(Action action)
        {
            _myAction = action;
        }
        public static ActionTree SetAction(Action action) => new ActionTree(action);
        public static MakeLoop StartLoop(int count) => new MakeLoop(count);
        public class MakeLoop
        {
            public MakeLoop(int count)
            {
                _count = count;
            }
            private List<ActionTree> _children = new();
            private readonly int _count;

            public MakeLoop AddChild(ActionTree child)
            {
                _children.Add(child);
                return this;
            }
            public ActionTree EndLoop
            {
                get => new ActionTree(() =>
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
