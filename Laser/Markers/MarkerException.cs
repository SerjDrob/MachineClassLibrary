using System;
using System.Runtime.Serialization;

namespace MachineClassLibrary.Laser.Markers
{
    public class MarkerException : Exception
    {
        public MarkerException()
        {
        }

        public MarkerException(string message) : base(message)
        {
        }

        public MarkerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MarkerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}