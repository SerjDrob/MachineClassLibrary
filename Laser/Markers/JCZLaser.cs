using MachineClassLibrary.Laser.Entities;
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
        private MarkLaserParams _markLaserParams;
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
            }
            else IsMarkDeviceInit = true;
        }

        public async Task<bool> PierceLineAsync(double x1, double y1, double x2, double y2)
        {
            return await Task.FromResult(Lmc.lmc1_MarkLine(x1, y1, x2, y2, 0) == 0);
        }

        public async Task<bool> PierceObjectAsync(IPerforating perforator)
        {
            await perforator.PierceObjectAsync();
            return true;
        }

        public async Task<bool> PierceDxfObjectAsync(string filePath)
        {
         
            Lmc.SetPenParams(_markLaserParams.PenParams);
            Lmc.SetHatchParams(_markLaserParams.HatchParams);
            Lmc.lmc1_AddFileToLib(filePath, "ProcEntity", 0, 0, 0, 0, 1, _markLaserParams.PenParams.PenNo, true);
            

            await Task.Run(() =>
            {
                var result = Lmc.lmc1_MarkEntity("ProcEntity");
                if (result != 0)
                {
                    Lmc.lmc1_DeleteEnt("ProcEntity");
                    throw new Exception($"Marking failed with code {(Lmc.EzCad_Error_Code)result}");
                }
            }
            );
            Lmc.lmc1_DeleteEnt("ProcEntity");
            
            return true;
        }

        public async Task<bool> PiercePointAsync(double x = 0, double y = 0)
        {
            return await Task.FromResult(Lmc.lmc1_MarkPoint(x, y, 0, 0) == 0);
        }

        public void SetExtMarkParams(ExtParamsAdapter paramsAdapter)
        {
            SetMarkParams(paramsAdapter.MixParams(_markLaserParams));
        }

        public void SetMarkDeviceParams()
        {
            Lmc.lmc1_SetDevCfg2(false, false);
        }

        public void SetMarkParams(MarkLaserParams markLaserParams)
        {
            var result = Lmc.SetPenParams(markLaserParams.PenParams);
            if (result != 0)
            {
                throw new Exception($"The device opening failed with error code {(Lmc.EzCad_Error_Code)result}");
            }
            result = Lmc.SetHatchParams(markLaserParams.HatchParams);
            if (result != 0)
            {
                throw new Exception($"The device opening failed with error code {(Lmc.EzCad_Error_Code)result}");
            }
        }
    }
}
