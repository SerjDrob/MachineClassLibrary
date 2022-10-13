using System;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;

namespace MachineClassLibrary.Laser
{
    public class PWM2 : IPWM
    {
        private SerialPort _serialPort;
        private string _lastMessage;


        private bool IsInRange(int value, int min, int max) => value >= min && value <= max;

        public Task<bool> ClosePWM()
        {
            if (_serialPort?.IsOpen ?? false)
            {
                _lastMessage = "Close :1:1";
                _serialPort.Write(_lastMessage);
                _serialPort.Close();
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
                //throw new InvalidOperationException("serial port isn't opened");
            }
        }

        public Task<bool> FindOpen()
        {
            var portName = "COM3";

            return Task.FromResult(OpenPort(portName));
        }

        public bool OpenPort(string port)
        {
            if (_serialPort != null)
            {
                if (_serialPort.PortName == port & _serialPort.IsOpen)
                {
                    return true;
                }
                if (_serialPort.PortName != port & _serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
            }

            var comPort = new SerialPort
            {
                PortName = port,
                BaudRate = 9600,
                Parity = Parity.Even,
                WriteTimeout = 1000,
                ReadTimeout = 100
            };
            try
            {
                comPort.Open();
            }
            catch (Exception)
            {
                return false;
            }
            comPort.Encoding = Encoding.Default;

            if (comPort.IsOpen)
            {
                _serialPort = comPort;
                return true;
            }
            else
            {
                return false;
            }
        }

        public Task<bool> SetPWM(int freq, int dutyCycle1, int modFreq, int dutyCycle2)
        {
            if (!IsInRange(modFreq, 1, 4000))
            {
                throw new ArgumentException("the freq must be in range [1,4000] Hz");
            }
            if (!IsInRange(dutyCycle2, 1, 100))
            {
                throw new ArgumentException("the dutyCycle must be in range [1,100] %");
            }
            if (_serialPort?.IsOpen ?? false)
            {
                var f = Math.Round(1000000f / (modFreq + 1));
                var c = Math.Round(f * dutyCycle2 / 100);
                _lastMessage = $"Str :{f,5}:{c,5}";
                try
                {
                    _serialPort.Write(_lastMessage);
                }
                catch (Exception ex)
                {

                    throw;
                }
                return Task.FromResult(true);
            }
            else
            {
                throw new InvalidOperationException("serial port isn't opened");
            }
        }

        public Task<bool> StopPWM()
        {
           // _serialPort.Write("Close :1:1");
            return Task.FromResult(true);
        }

        public override string ToString() => _lastMessage;
        
    }
}
