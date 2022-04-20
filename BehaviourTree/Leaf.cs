using Microsoft.VisualStudio.Workspace;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MachineClassLibrary.BehaviourTree
{
    public class Leaf : WorkerBase
    {

        private readonly Func<Task> _myWork;
        private bool isPausedAfterWork = false;
        private CancellationTokenSource cancellationTokenSource = new();
        private int pauseCount = 0;
        private int resumeCount = 0;

        private object _lock = new object();
        public Leaf(Func<Task> myWork)
        {
            _myWork = myWork;
        }
        private PauseTokenSource _pauseTokenAfterWork = new();
        private bool _waitMeAfterWorkDone = false;
        public override async Task<bool> DoWorkAsync()
        {
            if (!_isCancelled)
            {
                await base.DoWorkAsync();
                try
                {
                    if (_notBlocked)
                    {
                        //var task = new Task(_myWork, cancellationTokenSource.Token);
                        //task.Start();
                        //await task;


                        var task = _myWork?.Invoke();
                        //task.Start();
                        await task;
                        if (_waitMeAfterWorkDone)
                            await _pauseTokenAfterWork.Token.WaitWhilePausedAsync().ContinueWith(t => { isPausedAfterWork = false; });
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }
        public Leaf WaitForMe()
        {
            _waitMeAfterWorkDone = true;
            return this;
        }
        public override void PulseAction(bool info)
        {
            lock (_lock)
            {
                if (info & _waitMeAfterWorkDone)
                {
                    if (!isPausedAfterWork)
                    {
                        isPausedAfterWork = true;
                        pauseCount++;
                        _pauseTokenAfterWork.Pause();
                    }

                }
                else
                {
                    if (isPausedAfterWork)
                    {
                        resumeCount++;
                        _pauseTokenAfterWork.Resume();
                    }
                }
            }
        }
        public override void CancellAction(bool info)
        {
            if (info)
            {
                cancellationTokenSource.Cancel();
            }
        }
        public override Leaf SetActionBeforeWork(Action action)
        {
            return (Leaf)base.SetActionBeforeWork(action);
        }
        public override Leaf SetBlock(Block block)
        {
            return (Leaf)base.SetBlock(block);
        }
    }
}
