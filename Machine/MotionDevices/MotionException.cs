using System;
using System.Runtime.Serialization;

namespace MachineClassLibrary.Machine.MotionDevices
{
    public enum MotionExStatus
    {
        AccuracyNotReached,
        None
    }
    public class MotionException : Exception
    {
        public MotionException()
        {
        }

        public MotionException(string message) : base(message)
        {
        }

        public MotionException(string message, MotionExStatus status):base(message)
        {
            MotionExStatus = status;
        }

        public MotionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected MotionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public MotionExStatus MotionExStatus { get; } = MotionExStatus.None;
    }
}
