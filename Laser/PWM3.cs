using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using MachineClassLibrary.Miscellaneous;

namespace MachineClassLibrary.Laser
{
    public class PWM3 : WatchableDevice, IPWM
    {

        private SerialPort _serialPort;
        private string _lastMessage;
        private const string START_CMD = "START";
        private const string STOP_CMD = "STOP ";

        private (int bottom, int top) FREQ_RANGE = (10000, 150000);
        private (int bottom, int top) MODFREQ_RANGE = (50, 2000);
        private (int bottom, int top) DUTY_CYCLE = (1, 100);

        private string _response;
        private bool _isResponded;
        private const string PASSED = "Heisenberg";
        private const string PASSWORD = "ID ";

        public async Task<bool> FindOpen()
        {
            var availablePorts = SerialPort.GetPortNames();
            foreach (var port in availablePorts)
            {
                if (!OpenPort(port)) continue;
                if (await WaitCompareResponse($"{PASSWORD}", $"{PASSED}", 200))
                {
                    DeviceOK(this);
                    return true;
                }
                HasHealthProblem($"Cannot find the PWM", null, this);
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
                WriteTimeout = 500,
                ReadTimeout = 500,
                ReceivedBytesThreshold = 14,
                Handshake = Handshake.None
            };
            try
            {
                comPort.Open();
            }
            catch (Exception ex)
            {
                return false;
            }
            if (comPort.IsOpen)
            {
                _serialPort = comPort;
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
                _serialPort.ErrorReceived += _serialPort_ErrorReceived;
                _serialPort.DiscardOutBuffer();
                _serialPort.DiscardInBuffer();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void _serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            HasHealthProblem($"PWM's com-port got the error. {e}",null,this);
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                lock (this)
                {
                    var bytesCount = _serialPort.BytesToRead;
                    var message = new char[bytesCount];
                    var count = _serialPort.Read(message, 0, bytesCount);
                    _response = new string(message);
                    _serialPort.DiscardInBuffer();
                    _serialPort.DiscardOutBuffer();
                }
            }
            catch (Exception ex)
            {
                HasHealthProblem($"Cannot read the {_serialPort.PortName}", ex, this);
            }
            finally
            {
                _isResponded = true;
            }
            _isResponded = true;
        }

        private async Task<bool> WaitCompareResponse(string message, string assumedMessage, int waitingTime)
        {
            lock (this)
            {
                _serialPort.ReceivedBytesThreshold = assumedMessage.Length;
                _serialPort.Write(message);
            }
            var token = new CancellationTokenSource(waitingTime).Token;

            var task = Task.Run(() =>
            {
                while (!_isResponded && !token.IsCancellationRequested) ;
                return _isResponded;
            }, token);

            var answer = await task && _response.Contains(assumedMessage);
            lock (this)
            {
                var debugline = $"|{assumedMessage}| / |{_response}| / {answer}";
                if (debugline == string.Empty) debugline = "---------";
                Console.WriteLine(debugline);
            }
            _isResponded = false;
            _response = String.Empty;
            return answer;
        }

        public async Task<bool> SetPWM(int freq, int dutyCycle1, int modFreq, int dutyCycle2)
        {
            if (!_serialPort?.IsOpen ?? true) return false;
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
                _lastMessage = $"{START_CMD} f:{modFreq} d:{dutyCycle2} ";
                var result = await WaitCompareResponse(_lastMessage, _lastMessage, 200);
                if (result)
                {
                    DeviceOK(this);
                }
                else
                {
                    HasHealthProblem($"Set PWM command failed", null, this);
                }
                return result;
            }
            else
            {
                throw new InvalidOperationException("serial port isn't opened");
            }
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
                return await WaitCompareResponse($"{STOP_CMD}", $"{STOP_CMD}", 200);
            }
            //else
            //{
            //    throw new InvalidOperationException("serial port isn't opened");
            //}
            return false;
        }

        public override void CureDevice() => throw new NotImplementedException();
        public override void AskHealth() => throw new NotImplementedException();
    }
}
