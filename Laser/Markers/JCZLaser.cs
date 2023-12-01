using MachineClassLibrary.Laser.Entities;
using MachineClassLibrary.Laser.Parameters;
using netDxf.Entities;
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
        private readonly IPWM _pwm;


        //private MarkLaserParams _defaultLaserParams = new()

        public JCZLaser(IPWM pwm)
        {
            _pwm = pwm;
        }

        public void CloseMarkDevice()
        {
            //var result = Lmc.lmc1_Close();
            var result = JczLmc.Close();
            if (result != 0) throw new Exception($"The device closing failed with error code {(Lmc.EzCad_Error_Code)result}");
            else IsMarkDeviceInit = false;
        }

        public async Task<bool> InitMarkDevice(string initDirPath)
        {
            IntPtr Handle = new WindowInteropHelper(new Window()).Handle;
            //var result = Lmc.lmc1_Initial(initDirPath, 0, Handle);
            var result = JczLmc.InitializeTotal(initDirPath, false, Handle);
            if (result != 0)
            {
                throw new Exception($"The device opening failed with error code {(Lmc.EzCad_Error_Code)result}");
            }
            //_pwm = new PWM();
            if (!await _pwm.FindOpen())
            {
                throw new Exception($"The device opening failed. Can't open PWM device");
            }
            else IsMarkDeviceInit = true;
            return IsMarkDeviceInit;
        }

        public async Task<bool> PierceLineAsync(double x1, double y1, double x2, double y2)
        {
            var result = JczLmc.SetPenParams(_markLaserParams.PenParams) == 0;
                result = JczLmc.MarkLine(x1, y1, x2, y2, 0) == 0;
                if (!await _pwm.StopPWM())
                {

                }
                return result;
        }

        public async Task<bool> PierceObjectAsync(IPerforating perforator)
        {
            await perforator.PierceObjectAsync();
            return true;
        }

        public async Task<bool> PierceDxfObjectAsync(string filePath)
        {
            int result;
            result = JczLmc.SetPenParams(_markLaserParams.PenParams);
            result = JczLmc.SetHatchParams(_markLaserParams.HatchParams);
            var hatch = _markLaserParams.HatchParams;
            result = JczLmc.AddFileToLib(
                strFileName: filePath,
                strEntName: "Entity",
                dPosX: 0,
                dPosY: 0,
                dPosZ: 0,
                nAlign: 0,
                dRatio: 1,
                nPenNo: _markLaserParams.PenParams.PenNo,
                bHatchFile: 0);//_markLaserParams.HatchParams.EnableHatch ? 1:0);

            var GetHatchType = (int attribute) =>
            {
                if ((attribute & JczLmc.HATCHATTRIB_LOOP) > 0) return 0;
                if ((attribute & JczLmc.HATCHATTRIB_CROSSLINE) > 0) return 1;
                return 0;
            };


            result = JczLmc.SetHatchEntParam2(
                HatchName: "Entity",
                bEnableContour: hatch.EnableContour,//<---------------
                nParamIndex: 1,
                bEnableHatch: hatch.EnableHatch ? 1 : 0,
                bContourFirst: hatch.HatchContourFirst,
                nPenNo: _markLaserParams.PenParams.PenNo,
                nHatchType: GetHatchType(hatch.HatchAttribute),//2<----------
                bHatchAllCalc: false,
                bHatchEdge: hatch.HatchEdge,
                bHatchAverageLine: hatch.HatchAverageLine,
                dHatchAngle: 0,
                dHatchLineDist: hatch.HatchLineDist,
                dHatchEdgeDist: hatch.HatchEdgeDist,
                dHatchStartOffset: hatch.HatchStartOffset,
                dHatchEndOffset: hatch.HatchEndOffset,
                dHatchLineReduction: hatch.HatchLineReduction,
                dHatchLoopDist: hatch.HatchLoopDist,
                nEdgeLoop: hatch.EdgeLoop,//<-----------------
                nHatchLoopRev: (hatch.HatchAttribute & JczLmc.HATCHATTRIB_OUT) != 0,
                bHatchAutoRotate: hatch.HatchAutoRotate,//<----------
                dHatchRotateAngle: hatch.HatchRotateAngle,//<---------
                bHatchCrossMode: (hatch.HatchAttribute & JczLmc.HATCHATTRIB_CROSSLINE) != 0,
                dCycCount: 1
                );
           
            /*
            result = JczLmc.SetHatchEntParam2(
               HatchName: "Entity",
               bEnableContour: hatch.EnableContour,//<---------------
               nParamIndex: 1,
               bEnableHatch: hatch.EnableHatch ? 1 : 0,
               bContourFirst: hatch.HatchContourFirst,
               nPenNo: _markLaserParams.PenParams.PenNo,
               nHatchType:1,//<----------Figure
               bHatchAllCalc: false,
               bHatchEdge: hatch.HatchEdge,
               bHatchAverageLine: hatch.HatchAverageLine,
               dHatchAngle:0,//<----------Угол штриховки (град)
               dHatchLineDist: hatch.HatchLineDist,
               dHatchEdgeDist: hatch.HatchEdgeDist,
               dHatchStartOffset: hatch.HatchStartOffset,
               dHatchEndOffset: hatch.HatchEndOffset,
               dHatchLineReduction: 0.001,//hatch.HatchLineReduction,
               dHatchLoopDist: hatch.HatchLineDist, //hatch.HatchLoopDist,
               nEdgeLoop: 0,//hatch.EdgeLoop,<-----------------
               nHatchLoopRev: (hatch.HatchAttribute & JczLmc.HATCHATTRIB_OUT) != 0,
               bHatchAutoRotate: false,//hatch.HatchAutoRotate,//<---------- автоугол
               dHatchRotateAngle: hatch.HatchRotateAngle,//<--------- закоментированно
               bHatchCrossMode: true,
               dCycCount: 1
               );
            */
            JczLmc.SaveEntLibToFile("D:/TestFile.ezd");

            if (_markLaserParams.PenParams.IsModulated)
            {
                var freq = _markLaserParams.PenParams.Freq;
                var dutyCycle = freq * _markLaserParams.PenParams.QPulseWidth * 1e-6 * 100;
                var modFreq = _markLaserParams.PenParams.ModFreq;
                var modDutyCycle = _markLaserParams.PenParams.ModDutyCycle;

                if (await _pwm.SetPWM(freq, (int)Math.Round(dutyCycle), modFreq, modDutyCycle))
                {

                }
            }
            await Task.Run(async () =>
            {
                var result = (JczLmc.EzCad_Error_Code)JczLmc.MarkEntity("Entity");
                if (!await _pwm.StopPWM())
                {

                }
                if (!result.HasFlag(JczLmc.EzCad_Error_Code.LMC1_ERR_SUCCESS) & !result.HasFlag(JczLmc.EzCad_Error_Code.LMC1_ERR_USERSTOP))
                {
                    JczLmc.DeleteEnt("Entity");
                    throw new OperationCanceledException($"Marking failed with code {(Lmc.EzCad_Error_Code)result}");
                }
            }
            );
            JczLmc.DeleteEnt("Entity");
            
            return true;
        }

        public async Task<bool> PiercePointAsync(double x = 0, double y = 0)
        {
            return await Task.FromResult(JczLmc.MarkPoint(x, y, 0, 0) == 0);
        }

        public void SetExtMarkParams(ExtParamsAdapter paramsAdapter)
        {
            var resParams = paramsAdapter.MixParams(_markLaserParams);
            SetMarkParams(resParams);
        }

        public async Task<bool> MarkTextAsync(string text, double textSize, double angle)//TODO return bool or info or through exception?
        {
            var result = JczLmc.SetFontParam3(
                fontname: "Cambria",
                CharHeight: textSize,
                CharWidthRatio: 0.625,// * textSize,
                CharAngle: 0,
                CharSpace: 0,
                LineSpace: 0,
                spaceWidthRatio: 0.1,
                EqualCharWidth: true,
                nTextAlign: 8,
                bBold: false,
                bItalic: false);// Lmc.lmc1_SetFontParam("Cambria", textSize, 0.625 * textSize, 0, 0, 0, false);
            result += JczLmc.AddTextToLib(text, "text", 0, 0, 0, 8, angle, 0, 1); // Lmc.lmc1_AddTextToLib(text, "text", 0, 0, 0, 8, angle, 0, true);

            if (result!= 0) return false;
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
                var result = Lmc.lmc1_MarkEntity("text");
                if(_markLaserParams.PenParams.IsModulated) if(!await _pwm.StopPWM()){}
                
                if (result != 0)
                {
                    Lmc.lmc1_DeleteEnt("text");
                    throw new Exception($"Marking failed with code {(Lmc.EzCad_Error_Code)result}");
                }
            });
            Lmc.lmc1_DeleteEnt("text");
            return true;
        }

        public void SetMarkParams(MarkLaserParams markLaserParams)
        {
            _markLaserParams = markLaserParams;

            //var result = Lmc.SetPenParams(markLaserParams.PenParams);
            var result = JczLmc.SetPenParams(markLaserParams.PenParams);
            if (result != 0)
            {
                throw new Exception($"The device opening failed with error code {(Lmc.EzCad_Error_Code)result}");
            }
            //result = Lmc.SetHatchParams(markLaserParams.HatchParams);
            result = JczLmc.SetHatchParams(markLaserParams.HatchParams);
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
            //var result = Lmc.lmc1_CancelMark();
            var result = await Task.FromResult(JczLmc.StopMark());
            var res = true;
            if (_markLaserParams.PenParams.IsModulated) res = await _pwm.StopPWM();
            return res & result==0;
            //if (result != 0) throw new Exception($"Cancelling of marking failed with error code {(Lmc.EzCad_Error_Code)result}");
        }

        public bool SetDevConfig() => JczLmc.SetDevCfg2(false, false) == 0;
    }
}
