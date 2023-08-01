using Advantech.Motion;
using System;
using System.Threading.Tasks;

namespace MachineClassLibrary.Machine.MotionDevices
{
    public class MotionDevicePCI1245E : MotionDevicePCI1240U
    {
        public override void SetAxisConfig(int axisNum, MotionDeviceConfigs configs)
        {
            base.SetAxisConfig(axisNum, configs);
            double homeVelLow = configs.homeVelLow;
            double homeVelHigh = configs.homeVelHigh;
            var denominator = (uint)configs.denominator;
            _errors = new();
            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxPPUDenominator, ref denominator, 4); _errors.Add(PropertyID.CFG_AxPPUDenominator, _result);
            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxPulseInLogic, ref configs.plsInLogic, 4); _errors.Add(PropertyID.CFG_AxPulseInLogic, _result);
            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxHomeVelLow, ref homeVelLow, 8); _errors.Add(PropertyID.PAR_AxHomeVelLow, _result);
            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxHomeVelHigh, ref homeVelHigh, 8); _errors.Add(PropertyID.PAR_AxHomeVelHigh, _result);
            _initErrorsDictionaryInBaseClass = false;
            
            _initErrorsDictionaryInBaseClass = true;
        }

        private bool ResetAxErrorsWithEmgStop(IntPtr axHandle)
        {
            ushort axState = 0;
            Motion.mAcm_AxGetState(axHandle, ref axState);
            if (axState != (uint)Advantech.Motion.AxisState.STA_AX_READY)
            {
                Motion.mAcm_AxStopEmg(axHandle).CheckResult();
                Motion.mAcm_AxResetError(axHandle).CheckResult();
            }
            Motion.mAcm_AxResetError(axHandle).CheckResult();
            Motion.mAcm_AxResetError(axHandle).CheckResult();

            Motion.mAcm_AxGetState(axHandle, ref axState);
            return axState == (uint)Advantech.Motion.AxisState.STA_AX_READY;
        }

        public override async Task MoveAxisPreciselyAsync(int axisNum, double lineCoefficient, double position, int rec = 0)
        {
            var state = new uint();
            if (rec == 0) _storeSpeed = GetAxisVelocity(axisNum);
            uint buf = 0;
            uint bufLength = 4;
            uint ppu = 0;
            //uint res = 0;
            var tolerance = 0.001;

            ushort direction = 0;

            Motion.mAcm_GetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxPPU, ref ppu, ref bufLength).CheckResult(axisNum);
            //var pos = (int)(position * ppu);
            var pos = position;
            buf = (uint)SwLmtEnable.SLMT_DIS;
            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelEnable, ref buf, 4).CheckResult(axisNum);
            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelEnable, ref buf, 4).CheckResult(axisNum);
            if (lineCoefficient != 0)
            {
                var diff = position - CalcActualPosition(axisNum, lineCoefficient);
                if (Math.Abs(diff) > tolerance)
                {
                    Motion.mAcm_AxResetError(_mAxishand[axisNum]).CheckResult(_mAxishand[axisNum]);
                    buf = (uint)SwLmtReact.SLMT_IMMED_STOP;
                    Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelReact, ref buf, 4).CheckResult(axisNum);
                    Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelReact, ref buf, 4).CheckResult(axisNum);
                    var tol = 0;
                    uint react = 0;
                    switch (Math.Sign(diff))
                    {
                        case 1:
                            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelReact, ref react, 4).CheckResult(axisNum);
                            Motion.mAcm_SetF64Property(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelValue, pos).CheckResult(axisNum);
                            buf = (uint)SwLmtEnable.SLMT_EN;
                            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelEnable, ref buf, 4).CheckResult(axisNum);
                            buf = (uint)SwLmtToleranceEnable.TOLERANCE_ENABLE;
                            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelToleranceEnable, ref buf, 4).CheckResult(axisNum);
                            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelToleranceValue, ref tol, 8).CheckResult(axisNum);
                            direction = (ushort)VelMoveDir.DIR_POSITIVE;
                            break;

                        case -1:
                            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelReact, ref react, 4).CheckResult(axisNum);
                            Motion.mAcm_SetF64Property(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelValue, pos).CheckResult(axisNum);
                            buf = (uint)SwLmtEnable.SLMT_EN;
                            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelEnable, ref buf, 4).CheckResult(axisNum);
                            buf = (uint)SwLmtToleranceEnable.TOLERANCE_ENABLE;
                            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelToleranceEnable, ref buf, 4).CheckResult(axisNum);
                            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelToleranceValue, ref tol, 8).CheckResult(axisNum);
                            direction = (ushort)VelMoveDir.DIR_NEGATIVE;
                            break;
                    }

                    Motion.mAcm_AxMoveVel(_mAxishand[axisNum], direction).CheckResult(axisNum);
                    uint status = 0;
                    uint slmtp = 0;
                    uint slmtn = 0;
                    await Task.Run(async () =>
                    {
                        do
                        {
                            await Task.Delay(10);
                            Motion.mAcm_AxGetMotionIO(_mAxishand[axisNum], ref status);
                            slmtp = status & (uint)Ax_Motion_IO.AX_MOTION_IO_SLMTP;
                            slmtn = status & (uint)Ax_Motion_IO.AX_MOTION_IO_SLMTN;
                        } while (slmtp == 0 & slmtn == 0);
                    }
                    ).ConfigureAwait(false);

                    buf = (uint)SwLmtEnable.SLMT_DIS;
                    Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelEnable, ref buf, 4).CheckResult(axisNum);
                    Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelEnable, ref buf, 4).CheckResult(axisNum);
                    if (!ResetAxErrorsWithEmgStop(_mAxishand[axisNum]))
                    {
                        throw new MotionException($"Reset axis errors failed. axis number {axisNum}");
                    }
                    SetAxisVelocity(axisNum, 1);
                    Motion.mAcm_AxSetCmdPosition(_mAxishand[axisNum], CalcActualPosition(axisNum, lineCoefficient)).CheckResult(axisNum);


                    await MoveAxisPreciselyAsync(axisNum, lineCoefficient, position, ++rec);
                    rec--;
                    if (rec == 0)
                    {
                        // Motion.mAcm_AxResetError(_mAxishand[axisNum]).CheckResult(axisNum);
                        Motion.mAcm_AxResetError(_mAxishand[axisNum]).CheckResult(_mAxishand[axisNum]);

                        SetAxisVelocity(axisNum, _storeSpeed);
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
