using System;
using System.Runtime.Serialization;

namespace MachineClassLibrary.SFC;

class SpindleException : Exception
{
    public SpindleException()
    {
    }

    public SpindleException(string message) : base(message)
    {
    }

    public SpindleException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected SpindleException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
