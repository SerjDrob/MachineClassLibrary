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

        public Task<bool> ChangeSpeedAsync(ushort rpm, int delay) => throw new NotImplementedException();

        public bool Connect()
        {
            return true;
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
