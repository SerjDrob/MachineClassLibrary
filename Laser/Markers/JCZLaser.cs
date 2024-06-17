using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using MachineClassLibrary.Laser.Parameters;
using MachineClassLibrary.Miscellaneous;

namespace MachineClassLibrary.Laser.Markers
{
    public class JCZLaser : WatchableDevice, IMarkLaser
    {
        public bool IsMarkDeviceInit
        {
            get; private set;
        }
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

            using var pwm = _pwm as WatchableDevice;
            pwm?.OfType<HealthOK>()
                .Subscribe(ok =>
                {
                    DeviceOK(_pwm);
                });
            pwm?.OfType<HealthProblem>()
                .Subscribe(hp =>
                {
                    HasHealthProblem(hp.Message, hp.Exception, _pwm);
                });

            var result = await Task.Run(() =>
            {
                return JczLmc.InitializeTotal(initDirPath, false, Handle); 
            });
            var markerInit = true;
            if ((JczLmc.EzCad_Error_Code)result == JczLmc.EzCad_Error_Code.LMC1_ERR_SUCCESS) DeviceOK(this);
            else
            {
                var ex = new Exception($"The device opening failed with error code: {(Lmc.EzCad_Error_Code)result}");
                HasHealthProblem("",ex,this);
                markerInit = false;
                //throw new Exception($"The device opening failed with error code {(Lmc.EzCad_Error_Code)result}");
            }
            if (!await _pwm.FindOpen())
            {
                //throw new Exception($"The device opening failed. Can't open PWM device");
            }
            else IsMarkDeviceInit = true && markerInit;
            return IsMarkDeviceInit;
        }

        public async Task<bool> PierceLineAsync(double x1, double y1, double x2, double y2)
        {
            if(!IsMarkDeviceInit) return false;
            var result = JczLmc.SetPenParams(_markLaserParams.PenParams) == 0;
            result = JczLmc.MarkLine(x1, y1, x2, y2, 0) == 0;
            if (!await _pwm.StopPWM())
            {

            }
            return result;
        }

        public async Task<bool> PierceObjectAsync(IPerforating perforator)
        {
            if (!IsMarkDeviceInit) return false;
            await perforator.PierceObjectAsync();
            return true;
        }

        public async Task<bool> PierceDxfObjectAsync(string filePath)
        {
            if (!IsMarkDeviceInit) return false;

            int result;
            result = JczLmc.SetPenParams(_markLaserParams.PenParams);
            result += JczLmc.SetHatchParams(_markLaserParams.HatchParams);
            var hatch = _markLaserParams.HatchParams;
            result += JczLmc.AddFileToLib(
                strFileName: filePath,
                strEntName: "Entity",
                dPosX: 0,
                dPosY: 0,
                dPosZ: 0,
                nAlign: 0,
                dRatio: 1,
                nPenNo: _markLaserParams.PenParams.PenNo,
                bHatchFile: 0);//_markLaserParams.HatchParams.EnableHatch ? 1:0);

            int GetHatchType(int attribute)
            {
                if ((attribute & JczLmc.HATCHATTRIB_LOOP) > 0) return 2;
                if ((attribute & JczLmc.HATCHATTRIB_CROSSLINE) > 0) return 1;
                return 0;
            };


            result += JczLmc.SetHatchEntParam2(
                HatchName: "Entity",
                bEnableContour: hatch.EnableContour,//<---------------
                nParamIndex: 1,
                bEnableHatch: hatch.EnableHatch ? 1 : 0,
                bContourFirst: hatch.HatchContourFirst,
                nPenNo: _markLaserParams.PenParams.PenNo,
                nHatchType: GetHatchType(hatch.HatchAttribute),//2<----------
                bHatchAllCalc: false,
                bHatchEdge: hatch.HatchEdge,//when lines
                bHatchAverageLine: hatch.HatchAverageLine,
                dHatchAngle: 0,
                dHatchLineDist: hatch.HatchLineDist,
                dHatchEdgeDist: 0.0001,//hatch.HatchEdgeDist,
                dHatchStartOffset: hatch.HatchStartOffset,
                dHatchEndOffset: hatch.HatchEndOffset,
                dHatchLineReduction: 0,//hatch.HatchLineReduction,
                dHatchLoopDist: hatch.HatchLoopDist,
                nEdgeLoop: hatch.EdgeLoop,//<-----------------
                nHatchLoopRev: (hatch.HatchAttribute & JczLmc.HATCHATTRIB_OUT) != 0,
                bHatchAutoRotate: hatch.HatchAutoRotate,//<----------
                dHatchRotateAngle: hatch.HatchRotateAngle,//<---------
                bHatchCrossMode: (hatch.HatchAttribute & JczLmc.HATCHATTRIB_CROSSLINE) != 0,
                dCycCount: 1
                );
            if(result!=0) Console.WriteLine($"In the{nameof(PierceDxfObjectAsync)} JczLmc has result = {result} is {(JczLmc.EzCad_Error_Code)result}");

            //result = JczLmc.SetHatchEntParam2(
            //   HatchName: "Entity",
            //   bEnableContour: hatch.EnableContour,//<---------------
            //   nParamIndex: 1,
            //   bEnableHatch: hatch.EnableHatch ? 1 : 0,
            //   bContourFirst: hatch.HatchContourFirst,
            //   nPenNo: _markLaserParams.PenParams.PenNo,
            //   nHatchType: 1,//<----------Figure
            //   bHatchAllCalc: false,
            //   bHatchEdge: hatch.HatchEdge,
            //   bHatchAverageLine: hatch.HatchAverageLine,
            //   dHatchAngle: 0,//<----------Угол штриховки (град)
            //   dHatchLineDist: hatch.HatchLineDist,
            //   dHatchEdgeDist: 0.0001,//hatch.HatchEdgeDist,
            //   dHatchStartOffset: hatch.HatchStartOffset,
            //   dHatchEndOffset: hatch.HatchEndOffset,
            //   dHatchLineReduction: 0,//hatch.HatchLineReduction,
            //   dHatchLoopDist: hatch.HatchLineDist, //hatch.HatchLoopDist,
            //   nEdgeLoop: 0,//hatch.EdgeLoop,<-----------------
            //   nHatchLoopRev: (hatch.HatchAttribute & JczLmc.HATCHATTRIB_OUT) != 0,
            //   bHatchAutoRotate: false,//hatch.HatchAutoRotate,//<---------- автоугол
            //   dHatchRotateAngle: hatch.HatchRotateAngle,//<--------- закоментированно
            //   bHatchCrossMode: true,
            //   dCycCount: 1
            //   );


            var tempFilePath = Path.Combine(Path.GetTempPath(), "TestFile.ezd");
            result = JczLmc.SaveEntLibToFile(tempFilePath);
            await SetPwm(_markLaserParams.PenParams);
            await MarkEntityAndDelete("Entity");
            return true;
        }

        private async Task MarkEntityAndDelete(string entityName)
        {
            await Task.Run(async () =>
            {
                var result = (JczLmc.EzCad_Error_Code)JczLmc.MarkEntity(entityName);
                if (!result.HasFlag(JczLmc.EzCad_Error_Code.LMC1_ERR_USERSTOP))
                {
                    if (!result.HasFlag(JczLmc.EzCad_Error_Code.LMC1_ERR_SUCCESS))
                    {
                        JczLmc.DeleteEnt(entityName);
                        throw new OperationCanceledException($"Marking failed with code {(Lmc.EzCad_Error_Code)result}");
                    }
                    await _pwm.StopPWM();//TODO think and fix it
                }
            });
            JczLmc.DeleteEnt(entityName);
        }

        private async Task SetPwm(PenParams penParams)
        {
            if (penParams.IsModulated)
            {
                var freq = penParams.Freq;
                var dutyCycle = freq * penParams.QPulseWidth * 1e-6 * 100;
                var modFreq = penParams.ModFreq;
                var modDutyCycle = penParams.ModDutyCycle;
                try
                {
                    var pwmResult = await _pwm.SetPWM(freq, (int)Math.Round(dutyCycle), modFreq, modDutyCycle);
                    if (!pwmResult) throw new MarkerException("PWM is failed. Cannot get the response.");
                }
                catch (InvalidOperationException ex)
                {
                    throw new MarkerException("PWM is failed. Serial port is close.", ex);
                }
                catch (ArgumentException ex)
                {
                    throw new MarkerException("PWM is failed. An argument is invalid", ex);
                }
                catch (Exception ex)
                {
                    throw new MarkerException(ex.Message, ex);
                }
            }
        }

        public async Task<bool> PiercePointAsync(double x = 0, double y = 0)
        {
            if (!IsMarkDeviceInit) return false;

            return await Task.FromResult(JczLmc.MarkPoint(x, y, 0, 0) == 0);
        }

        public void SetExtMarkParams(ExtParamsAdapter paramsAdapter)
        {
            if (!IsMarkDeviceInit) return;

            var resParams = paramsAdapter.MixParams(_markLaserParams);
            SetMarkParams(resParams);
        }

        public async Task<bool> MarkTextAsync(string text, double textSize, double angle)//TODO return bool or info or through exception?
        {
            if (!IsMarkDeviceInit) return false;

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
                bItalic: false);
            result += JczLmc.SetPenParams(_markLaserParams.PenParams);
            result += JczLmc.AddTextToLib(text, "Text", 0, 0, 0, 8, angle, 0, 1); 
            
            if (result != 0) return false;
            await SetPwm(_markLaserParams.PenParams);
            await MarkEntityAndDelete("Text");
            return true;
        }

        public void SetMarkParams(MarkLaserParams markLaserParams)
        {
            if (!IsMarkDeviceInit) return;

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
            if (!IsMarkDeviceInit) return false;

            Lmc.lmc1_AddCircleToLib(0, 0, diameter / 2, "circle", 0);
            Lmc.lmc1_MarkEntity("circle");
            Lmc.lmc1_DeleteEnt("circle");
            return true;
        }

        public async Task<bool> CancelMarkingAsync()
        {
            if (!IsMarkDeviceInit) return false;

            //var result = Lmc.lmc1_CancelMark();
            var result = await Task.FromResult(JczLmc.StopMark());
            var res = true;
            if (_markLaserParams.PenParams.IsModulated) res = await _pwm.StopPWM();//TODO exception?
            return res & result == 0;
            //if (result != 0) throw new Exception($"Cancelling of marking failed with error code {(Lmc.EzCad_Error_Code)result}");
        }

        public bool SetDevConfig() => JczLmc.SetDevCfg2(false, false) == 0;

        public override void CureDevice()
        {
            throw new NotImplementedException();
        }

        public override void AskHealth()
        {
            throw new NotImplementedException();
        }
        public void SetSystemAngle(double angle)
        {
            var result = JczLmc.SetRotateMoveParam(0,0,0,0,angle);
        }
    }
}
