using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MachineClassLibrary.Laser.Parameters;

namespace MachineClassLibrary.Laser.Markers
{
    public class MockLaser : IMarkLaser
    {
        public bool IsMarkDeviceInit { get; private set; }

        public void CancelMarking()
        {
            throw new NotImplementedException();
        }

        public void CloseMarkDevice()
        {
            IsMarkDeviceInit = false;
        }

        public Task InitMarkDevice(string initDirPath)
        {
            IsMarkDeviceInit = true;
            return Task.CompletedTask;
        }

        public Task<bool> PierceCircleAsync(double diameter)
        {
            Debug.WriteLine($"Laser is piercing circle d = {diameter}");
            return Task.FromResult(true);
        }
        
        public Task<bool> PierceDxfObjectAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PierceLineAsync(double x1, double y1, double x2, double y2)
        {
            Debug.WriteLine($"Laser is piercing line ({x1},{y1}),({x2},{y2})");
            return Task.FromResult(true);
        }

        public Task<bool> PierceObjectAsync(IPerforating perforator)
        {
            Debug.WriteLine($"Laser is piercing object {perforator.GetType().Name}");
            return Task.FromResult(true);
        }

        public Task<bool> PiercePointAsync(double x = 0, double y = 0)
        {
            Debug.WriteLine($"Laser is piercing point ({x},{y})");
            return Task.FromResult(true);
        }

        public void SetExtMarkParams(ExtParamsAdapter paramsAdapter)
        {
            throw new NotImplementedException();
        }

        public void SetMarkDeviceParams()
        {
            Debug.WriteLine($"Device params is set");
        }

        public void SetMarkParams(MarkLaserParams markLaserParams)
        {
            throw new NotImplementedException();
        }
    }
}
