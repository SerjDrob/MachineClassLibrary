using Advantech.Motion;
using MachineClassLibrary;
using MachineClassLibrary.Machine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineClassLibrary.Machine.MotionDevices
{
    public class MotionDevicePCI1245E : MotionDevicePCI1240U
    {
        public override void SetAxisConfig(int axisNum, MotionDeviceConfigs configs)
        {
            double homeVelLow = configs.homeVelLow;
            double homeVelHigh = configs.homeVelHigh;
            _errors = new();
            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxPulseInLogic, ref configs.plsInLogic, 4); _errors.Add(PropertyID.CFG_AxPulseInLogic, _result);
            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxHomeVelLow, ref homeVelLow, 8); _errors.Add(PropertyID.PAR_AxHomeVelLow, _result);
            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxHomeVelHigh, ref homeVelHigh, 8); _errors.Add(PropertyID.PAR_AxHomeVelHigh, _result);
            _initErrorsDictionaryInBaseClass = false;
            base.SetAxisConfig(axisNum, configs);
            _initErrorsDictionaryInBaseClass= true;
        }


        public override async Task MoveAxisPreciselyAsync(int axisNum, double lineCoefficient, double position, int rec = 0)
        {
            var state = new uint();
            if (rec == 0) _storeSpeed = GetAxisVelocity(axisNum);
            uint buf = 0;
            uint bufLength = 4;
            uint ppu = 0;
            uint res = 0;
            var tolerance = 0.003;

            ushort direction = 0;

            Motion.mAcm_GetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxPPU, ref ppu, ref bufLength).CheckResult();
            var pos = (int)(position * ppu);
            buf = (uint)SwLmtEnable.SLMT_DIS;
            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelEnable, ref buf, 4).CheckResult();
            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelEnable, ref buf, 4).CheckResult();
            if (lineCoefficient != 0)
            {
                var diff = position - CalcActualPosition(axisNum, lineCoefficient);
                if (Math.Abs(diff) > tolerance)
                {
                    Motion.mAcm_AxResetError(_mAxishand[axisNum]).CheckResult();
                    buf = (uint)SwLmtReact.SLMT_IMMED_STOP;
                    Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelReact, ref buf, 4).CheckResult();
                    Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelReact, ref buf, 4).CheckResult();
                    var tol = 0;
                    switch (Math.Sign(diff))
                    {
                        case 1:
                            pos = 5;
                            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelValue, ref pos, 4).CheckResult();
                            int getPos = 0;
                            uint bufL = 8;
                            Motion.mAcm_GetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelValue, ref getPos, ref bufL).CheckResult();
                            buf = (uint)SwLmtEnable.SLMT_EN;
                            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelEnable, ref buf, 4).CheckResult();
                            //buf = (uint)SwLmtToleranceEnable.TOLERANCE_ENABLE;
                            //Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelToleranceEnable, ref buf, 4).CheckResult();
                            //Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelToleranceValue, ref tol, 8).CheckResult();
                            direction = (ushort)VelMoveDir.DIR_POSITIVE;
                            break;

                        case -1:
                            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelValue, ref pos, 8).CheckResult();
                            buf = (uint)SwLmtEnable.SLMT_EN;
                            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelEnable, ref buf, 4).CheckResult();
                            //buf = (uint)SwLmtToleranceEnable.TOLERANCE_ENABLE;
                            //Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelToleranceEnable, ref buf, 4).CheckResult();
                            //Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelToleranceValue, ref tol, 8).CheckResult();
                            direction = (ushort)VelMoveDir.DIR_NEGATIVE;
                            break;
                    }

                    Motion.mAcm_AxMoveVel(_mAxishand[axisNum], direction).CheckResult();
                    uint status = 0;
                    uint slmtp = 0;
                    uint slmtn = 0;
                    await Task.Run(async () =>
                    {
                        do
                        {
                            await Task.Delay(10);
                            //Thread.Sleep(1);
                            Motion.mAcm_AxGetMotionIO(_mAxishand[axisNum], ref status);
                            slmtp = status & (uint)Ax_Motion_IO.AX_MOTION_IO_SLMTP;
                            slmtn = status & (uint)Ax_Motion_IO.AX_MOTION_IO_SLMTN;
                        } while (slmtp == 0 & slmtn == 0);
                    }
                    ).ConfigureAwait(false);
                    SetAxisVelocity(axisNum, 1);
                    Motion.mAcm_AxSetCmdPosition(_mAxishand[axisNum], CalcActualPosition(axisNum, lineCoefficient)).CheckResult();
                    await MoveAxisPreciselyAsync(axisNum, lineCoefficient, position, ++rec);
                    rec--;
                    if (rec == 0)
                    {
                        SetAxisVelocity(axisNum, _storeSpeed);
                        Motion.mAcm_AxResetError(_mAxishand[axisNum]).CheckResult();
                    }
                }
            }
            else
            {
                Motion.mAcm_AxMoveAbs(_mAxishand[axisNum], position);
                await Task.Run(() =>
                {
                    do
                    {
                        Task.Delay(1).Wait();
                        Motion.mAcm_AxGetMotionStatus(_mAxishand[axisNum], ref state);
                    } while ((state & 0x1) == 0);
                });
            }
        }
    }
}
