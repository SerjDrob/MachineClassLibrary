using MachineClassLibrary.Laser.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MachineClassLibrary.Laser
{
    public interface IMarkLaser
    {
        public bool IsMarkDeviceInit { get; }
        public void InitMarkDevice();
        public void CloseMarkDevice();
        public void SetMarkDeviceParams();        
        public Task<bool> PierceObjectAsync(IPerforatorBuilder perforatorBuilder);

    }
}
