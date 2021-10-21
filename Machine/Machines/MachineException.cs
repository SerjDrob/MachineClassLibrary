using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineClassLibrary.Machine.Machines
{

    [Serializable]
    public class MachineException : Exception
    {
        public MachineException() { }
        public MachineException(string message) : base(message) { }
        public MachineException(string message, Exception inner) : base(message, inner) { }
        protected MachineException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
