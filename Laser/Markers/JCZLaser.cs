using MachineClassLibrary.Laser.Entities;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

        public void InitMarkDevice()
        {
            IntPtr Handle = new WindowInteropHelper(new Window()).Handle;
            var result = Lmc.lmc1_Initial(Directory.GetCurrentDirectory(), 0, Handle);

            if (result != 0)
            {
                throw new Exception($"The device closing failed with error code {(Lmc.EzCad_Error_Code)result}");
                Lmc.lmc1_Close();
            }
            else IsMarkDeviceInit = true;
        }

        public async Task<bool> PierceObjectAsync(IPerforatorBuilder perforatorBuilder)
        {
            var perforator = perforatorBuilder.Build();
            await perforator.PierceObjectAsync();
            return true;
        }

        public void SetMarkDeviceParams()
        {
            Lmc.lmc1_SetDevCfg2(false, false);
        }
    }
}
