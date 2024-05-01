using System;
using System.Collections.Generic;

namespace MachineClassLibrary.Machine
{
    [Obsolete]
    public class ExceptionsAgregator
    {
        private readonly List<Action<string>> _messagesActions = new();

        public ExceptionsAgregator()
        {
        }

        // TODO add unregister to avoid delegate leak
        public void RegisterMessager(IMessager messager)
        {
            messager.ThrowMessage += Messager_ThrowMessage;
        }

        public void SetShowMethod(Action<string> method)
        {
            _messagesActions.Add(method);
            //ShowMessage = method;
        }

        //private Action<string> ShowMessage;
        private void Messager_ThrowMessage(string message, int methodNum)
        {
            if (methodNum < 0 | methodNum > _messagesActions.Count - 1) throw new Exception();
            _messagesActions[methodNum]?.Invoke(message);
            //ShowMessage?.Invoke(message);
        }
    }
}