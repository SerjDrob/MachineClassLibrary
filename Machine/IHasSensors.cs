using System.Collections.Generic;
using System;
using MachineClassLibrary.Classes;

namespace MachineClassLibrary.Machine
{
    public interface IHasSensors
    {       
        public event EventHandler<SensorsEventArgs> OnSensorStateChanged;
        public string GetSensorName(Sensors sensor);
        public void SetBridgeOnSensors(Sensors sensor, bool setBridge);
        public void ConfigureSensors(Dictionary<Sensors, (Ax, Di, bool, string)> sensors);
    }
}