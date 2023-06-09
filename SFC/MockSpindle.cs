using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineClassLibrary.SFC
{
    public class MockSpindle : ISpindle
    {
        public bool IsConnected { get; set; }

        public event EventHandler<SpindleEventArgs> GetSpindleState;

        public void Connect()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void SetSpeed(ushort rpm)
        {
            //throw new NotImplementedException();
        }

        public void Start()
        {
            //throw new NotImplementedException();
        }

        public void Stop()
        {
            //throw new NotImplementedException();
        }
    }
}
