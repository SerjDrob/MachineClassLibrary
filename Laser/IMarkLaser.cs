using MachineClassLibrary.Laser.Entities;
using MachineClassLibrary.Laser.Parameters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MachineClassLibrary.Laser
{
    public interface IMarkLaser
    {
        public bool IsMarkDeviceInit { get; }
        public Task<bool> InitMarkDevice(string initDirPath);
        public void CloseMarkDevice();
        public void SetMarkDeviceParams();

        public void SetMarkParams(MarkLaserParams markLaserParams);

        public void SetExtMarkParams(ExtParamsAdapter paramsAdapter);

        public Task<bool> PierceObjectAsync(IPerforating perforator);
        //public Task<bool> PierceHatchRingAsync(double outerD, double width);
        public Task<bool> PiercePointAsync(double x=0, double y=0);
        public Task<bool> PierceLineAsync(double x1, double y1, double x2, double y2);
        Task<bool> PierceCircleAsync(double diameter);
        Task<bool> PierceDxfObjectAsync(string filePath);
        Task<bool> CancelMarkingAsync();
    }
}
