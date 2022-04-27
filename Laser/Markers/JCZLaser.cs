using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace MachineClassLibrary.Laser.Markers
{
    public class JCZLaser : IMarkLaser
    {
        public bool IsMarkDeviceInit { get; private set; }

        public void CloseMarkDevice()
        {
            var result = Lmc.lmc1_Close();
            if (result != 0) throw new Exception($"The device closing failed with error code {(Lmc.EzCad_Error_Code)result}");
            else IsMarkDeviceInit = false;
        }

        public void InitMarkDevice(string initDirPath)
        {
            IntPtr Handle = new WindowInteropHelper(new Window()).Handle;
            var result = Lmc.lmc1_Initial(initDirPath, 0, Handle);

            if (result != 0)
            {
                throw new Exception($"The device opening failed with error code {(Lmc.EzCad_Error_Code)result}");
                Lmc.lmc1_Close();
            }
            else IsMarkDeviceInit = true;
        }

        public async Task<bool> PierceLineAsync(double x1, double y1, double x2, double y2)
        {
            Lmc.lmc1_MarkLine(x1, y1, x2, y2, 0);
            return true;
        }

        public async Task<bool> PierceObjectAsync(IPerforatorBuilder perforatorBuilder)
        {
            var perforator = perforatorBuilder.Build();
            await perforator.PierceObjectAsync();
            return true;
        }

        public async Task<bool> PiercePointAsync(double x = 0, double y = 0)
        {
            Lmc.lmc1_MarkPoint(x, y, 0, 0);
            return true;
        }

        public void SetMarkDeviceParams()
        {
            Lmc.lmc1_SetDevCfg2(false, false);
        }
    }
}
