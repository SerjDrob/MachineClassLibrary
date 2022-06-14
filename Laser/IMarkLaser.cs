using MachineClassLibrary.Laser.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MachineClassLibrary.Laser
{
    public interface IMarkLaser
    {
        public bool IsMarkDeviceInit { get; }
        public void InitMarkDevice(string initDirPath);
        public void CloseMarkDevice();
        public void SetMarkDeviceParams();        
        public Task<bool> PierceObjectAsync(IPerforating perforator);
        //public Task<bool> PierceHatchRingAsync(double outerD, double width);
        public Task<bool> PiercePointAsync(double x=0, double y=0);
        public Task<bool> PierceLineAsync(double x1, double y1, double x2, double y2);
        Task<bool> PierceCircleAsync(double diameter);
    }
}
