using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MachineClassLibrary.SFC;

//public class MockSpindle : ISpindle
//{
//    public bool IsConnected { get; set; }

//    public event EventHandler<SpindleEventArgs> GetSpindleState;

//    public Task<bool> ChangeSpeedAsync(ushort rpm, int delay) => throw new NotImplementedException();

//    public bool Connect()
//    {
//        return true;
//    }

//    public void Dispose()
//    {
//        //throw new NotImplementedException();
//    }

//    public void SetSpeedAsync(ushort rpm)
//    {
//        //throw new NotImplementedException();
//    }

//    public void StartAsync()
//    {
//        //throw new NotImplementedException();
//    }

//    public void Stop()
//    {
//        //throw new NotImplementedException();
//    }
//}


public class MockSpindle : SpindleBase<MockSpindle>
{
    public MockSpindle(SerialPortSettings serialPortSettings, ILogger<MockSpindle> logger) : base(serialPortSettings, logger)
    {
    }

    protected override Task<bool> CheckIfSpindleSpinningAsync()
    {
        return Task.FromResult(true);
    }

    protected override Task<int> GetCurrentAsync()
    {
        return Task<int>.FromResult(0);
    }

    protected override Task<int> GetFrequencyAsync()
    {
        return Task.FromResult(0);
    }

    protected override Task<SpinStatus> GetStatusAsync()
    {
        return Task.FromResult(
            new SpinStatus(false, false, false, true)
            );
    }

    protected override Task StartFWDCommandAsync()
    {
        return Task.CompletedTask;
    }

    protected override Task StopCommandAsync()
    {
        return Task.CompletedTask;
    }

    protected override Task WriteRPMAsync(ushort rpm)
    {
        return Task.CompletedTask;
    }

    protected override Task WriteSettingsAsync()
    {
        return Task.CompletedTask;
    }
}
