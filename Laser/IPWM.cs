using System.Threading.Tasks;

namespace MachineClassLibrary.Laser
{
    public interface IPWM
    {
        Task<bool> ClosePWM();
        Task<bool> FindOpen();
        bool OpenPort(string port);
        void SetBaudRate(int baudRate);
        Task<bool> SetPWM(int freq, int dutyCycle1, int modFreq, int dutyCycle2);
        Task<bool> StopPWM();
    }
}