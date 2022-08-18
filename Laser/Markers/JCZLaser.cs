using MachineClassLibrary.Laser.Entities;
using MachineClassLibrary.Laser.Parameters;
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
        private PWM _pwm;
        public void CloseMarkDevice()
        {
            var result = Lmc.lmc1_Close();
            if (result != 0) throw new Exception($"The device closing failed with error code {(Lmc.EzCad_Error_Code)result}");
            else IsMarkDeviceInit = false;
        }

        public async Task<bool> InitMarkDevice(string initDirPath)
        {
            IntPtr Handle = new WindowInteropHelper(new Window()).Handle;
            var result = Lmc.lmc1_Initial(initDirPath, 0, Handle);

            if (result != 0)
            {
                throw new Exception($"The device opening failed with error code {(Lmc.EzCad_Error_Code)result}");
            }
            _pwm = new PWM();
            if (!await _pwm.FindOpen())
            {
                throw new Exception($"The device opening failed. Can't open PWM device");
            }
            else IsMarkDeviceInit = true;
            return IsMarkDeviceInit;
        }

        public async Task<bool> PierceLineAsync(double x1, double y1, double x2, double y2)
        {
            if (!await _pwm.SetPWM(40000,4, 1000, 50))
            {

            }
            var result =  Lmc.lmc1_MarkLine(x1, y1, x2, y2, 0) == 0;
            if (!await _pwm.StopPWM())
            {

            }
            return await Task.FromResult(result);
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
            Lmc.lmc1_AddFileToLib(filePath, "ProcEntity", 0, 0, 0, 0, 1, _markLaserParams.PenParams.PenNo, _markLaserParams.HatchParams.EnableHatch);
            //Lmc.lmc1_SaveEntLibToFile("D:/TestFile.ezd");
            if (_markLaserParams.PenParams.IsModulated)
            {
                var freq = _markLaserParams.PenParams.Freq;
                var dutyCycle = freq * _markLaserParams.PenParams.QPulseWidth * 1e-6 * 100;
                var modFreq = _markLaserParams.PenParams.ModFreq;
                var modDutyCycle = _markLaserParams.PenParams.ModDutyCycle;

                if (!await _pwm.SetPWM(freq, (int)Math.Round(dutyCycle), modFreq, modDutyCycle))
                {

                }
            }
            await Task.Run(async () =>
            {
                var result = Lmc.lmc1_MarkEntity("ProcEntity");
                if (!await _pwm.StopPWM())
                {

                }
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
            var resParams = paramsAdapter.MixParams(_markLaserParams);
            SetMarkParams(resParams);
        }

        public void SetMarkDeviceParams()
        {
            Lmc.lmc1_SetDevCfg2(false, false);
        }

        public void SetMarkParams(MarkLaserParams markLaserParams)
        {
            _markLaserParams = markLaserParams;

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
        public async Task<bool> PierceCircleAsync(double diameter)
        {
            Lmc.lmc1_AddCircleToLib(0, 0, diameter / 2, "circle", 0);
            Lmc.lmc1_MarkEntity("circle");
            Lmc.lmc1_DeleteEnt("circle");
            return true;
        }

        public async Task<bool> CancelMarkingAsync()
        {
            var result = Lmc.lmc1_CancelMark();
            return await _pwm.StopPWM();
            //if (result != 0) throw new Exception($"Cancelling of marking failed with error code {(Lmc.EzCad_Error_Code)result}");
        }
    }
}
