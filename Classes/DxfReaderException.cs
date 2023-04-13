using System;

namespace MachineClassLibrary.Classes
{
    [Serializable]
    public class DxfReaderException : Exception
    {
        public DxfReaderException() { }
        public DxfReaderException(string message) : base(message) { }
        public DxfReaderException(string message, Exception inner) : base(message, inner) { }
        protected DxfReaderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
