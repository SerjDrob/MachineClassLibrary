using System;
using System.Threading.Tasks;

namespace MachineClassLibrary.SFC
{
    public delegate void SpindleStateHandler(bool isConnected, double spinCurrent, double spindleFreq);

    public interface ISpindle : IDisposable
    {
        public bool IsConnected { get; set; }
        public Task SetSpeedAsync(ushort rpm);
        public Task StartAsync();
        public Task StopAsync();
        Task<bool> ConnectAsync();
        Task<bool> ChangeSpeedAsync(ushort rpm, int delay);

        /// <summary>
        ///     Gets frequency, current, spinning state
        /// </summary>

        public event EventHandler<SpindleEventArgs> GetSpindleState;
    }
}
