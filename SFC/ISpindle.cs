using System;
using System.Threading.Tasks;

namespace MachineClassLibrary.SFC
{
    public delegate void SpindleStateHandler(bool isConnected, double spinCurrent, double spindleFreq);

    public interface ISpindle : IDisposable
    {
        public bool IsConnected { get; set; }
        public void SetSpeed(ushort rpm);
        public void Start();
        public void Stop();
        void Connect();
        Task<bool> ChangeSpeedAsync(ushort rpm, int delay);

        /// <summary>
        ///     Gets frequency, current, spinning state
        /// </summary>

        public event EventHandler<SpindleEventArgs> GetSpindleState;
    }
}