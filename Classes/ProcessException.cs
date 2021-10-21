using System;
using System.Runtime.Serialization;

namespace MachineClassLibrary.Classes
{
    class ProcessException : Exception
    {
        public ProcessException()
        {
        }

        public ProcessException(string message) : base(message)
        {
        }

        public ProcessException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ProcessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
