using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MachineClassLibrary.SFC
{
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
}
