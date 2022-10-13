using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MachineClassLibrary.Laser
{
    public class PWM : IPWM
    {

        private SerialPort _serialPort;
        private string _lastMessage;
        private const string START_CMD = "String:Str";
        private const string STOP_CMD = "String:Close";

        private (int bottom, int top) FREQ_RANGE = (10000, 150000);
        private (int bottom, int top) MODFREQ_RANGE = (50, 1000);
        private (int bottom, int top) DUTY_CYCLE = (1, 100);

        private string _response;
        private bool _isResponded;
        private const string PASSED = "Heisenberg";
        private const string PASSWORD = "String:Say_my_name ";

        public async Task<bool> FindOpen()
        {
            var avaliablePorts = SerialPort.GetPortNames();
            foreach (var port in avaliablePorts)
            {
                if (!OpenPort(port)) continue;

                _serialPort.Write($"{PASSWORD}");
                if (await WaitCompareResponse($"{PASSED}", 200)) return true;
                _serialPort.Close();
            }
            return false;
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
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                WriteTimeout = 100,
                ReadTimeout = 100,
                ReceivedBytesThreshold = 1,
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
                _serialPort.DataReceived += _serialPort_DataReceived;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var bytesCount = _serialPort.BytesToRead;
            var message = new char[bytesCount];
            var count = _serialPort.Read(message, 0, bytesCount);
            _response = new String(message);
            _isResponded = true;
        }

        private async Task<bool> WaitCompareResponse(string assumedMessage, int waitingTime)
        {
            var token = new CancellationTokenSource(waitingTime).Token;

            var task = Task.Run(() =>
            {
                while (!_isResponded && !token.IsCancellationRequested) ;
                return _isResponded;
            }, token);

            var answer = await task && assumedMessage.Equals(_response);
            _isResponded = false;
            _response = String.Empty;
            return answer;

        }

        public async Task<bool> SetPWM(int freq, int dutyCycle1, int modFreq, int dutyCycle2)
        {
            if (!_serialPort.IsOpen) return false;
            if (!IsInRange(freq, FREQ_RANGE.bottom, FREQ_RANGE.top))
            {
                throw new ArgumentException($"the frequency must be in range [{FREQ_RANGE.bottom}, {FREQ_RANGE.top}] Hz");
            }
            if (!IsInRange(dutyCycle1, DUTY_CYCLE.bottom, DUTY_CYCLE.top))
            {
                throw new ArgumentException($"the dutyCycle1 must be in range [{DUTY_CYCLE.bottom}, {DUTY_CYCLE.top}] %");
            }
            if (!IsInRange(modFreq, MODFREQ_RANGE.bottom, MODFREQ_RANGE.top))
            {
                throw new ArgumentException($"the frequency must be in range [{MODFREQ_RANGE.bottom}, {MODFREQ_RANGE.top}] Hz");
            }
            if (!IsInRange(dutyCycle2, DUTY_CYCLE.bottom, DUTY_CYCLE.top))
            {
                throw new ArgumentException($"the dutyCycle2 must be in range [{DUTY_CYCLE.bottom}, {DUTY_CYCLE.top}] %");
            }

            if (_serialPort?.IsOpen ?? false)
            {
                var f1 = Math.Round(48000000f / (freq + 1));
                var c1 = Math.Round(f1 * dutyCycle1 / 100);

                var f2 = Math.Round(12000000f / (modFreq + 1));
                var c2 = Math.Round(f2 * dutyCycle2 / 100);
                _lastMessage = $"f1:{f1,5} d1:{c1,5} f2:{f2,5} d2:{c2,5}";
                _serialPort.Write($"{START_CMD} {_lastMessage}");

                return await WaitCompareResponse("START", 200);
            }
            else
            {
                throw new InvalidOperationException("serial port isn't opened");
            }
            return false;
        }

        public override string ToString()
        {
            return _lastMessage;
        }

        private bool IsInRange(int value, int min, int max) => value >= min && value <= max;


        public async Task<bool> ClosePWM()
        {
            if (await StopPWM())
            {
                _serialPort?.Close();
                return true;
            }
            return false;
        }

        public async Task<bool> StopPWM()
        {
            if (_serialPort?.IsOpen ?? false)
            {
                _serialPort.Write($"{STOP_CMD} {_lastMessage}");

                return await WaitCompareResponse("STOP", 200); ;
            }
            //else
            //{
            //    throw new InvalidOperationException("serial port isn't opened");
            //}
            return false;
        }

    }
}
