using System;
using System.Runtime.Serialization;

namespace MachineClassLibrary.Machine.MotionDevices
{
    public class MotionException : Exception
    {
        public MotionException()
        {
        }

        public MotionException(string message) : base(message)
        {
        }

        public MotionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected MotionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}