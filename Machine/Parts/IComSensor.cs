using MachineClassLibrary.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace MachineClassLibrary.Machine.Parts
{
    public interface IComSensor
    {
        public bool EstablishConnection(string comPort);
        public event Action<decimal> GetData;
    }
}