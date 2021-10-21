using MachineClassLibrary.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace MachineClassLibrary.Machine.Parts
{
    public class FlowMeter : IComSensor, IDisposable
    {
        public FlowMeter(string com)
        {
            EstablishConnection(com);
            Task.Run(() => ReadingPort());
        }

        private Queue<byte> recievedData = new Queue<byte>();
        private SerialPort _serialPort;
        public bool EstablishConnection(string comPort)
        {

            _serialPort = new SerialPort
            {
                PortName = comPort,
                BaudRate = 9600,
                Parity = Parity.Even,
                WriteTimeout = 1000,
                ReadTimeout = 1000
            };
            try
            {
                _serialPort.Open();
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
                return false;
            }
            catch (Exception) { return false; }

            _serialPort.DiscardNull = true;
            return _serialPort.IsOpen;
        }

        private List<int> bufList;
        private async void ReadingPort()
        {
            while (_serialPort.IsOpen)
            {
                var val = new List<int>();
                var data = _serialPort.ReadExisting();
                var ch = data.Split(new[] { '\n', '\0' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in ch)
                {
                    int v = 0;
                    if (int.TryParse(s, out v))
                    {
                        val.Add(v);
                    }
                }

                if (val.Count > 0)
                {
                    decimal result = val.Sum() / val.Count;

                    GetData(Math.Round(result.Map(700, 4096, 0, 4), 1));
                }

                await Task.Delay(1).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            _serialPort.Dispose();
            GC.SuppressFinalize(_serialPort);
        }

        public event Action<decimal> GetData;
    }
}