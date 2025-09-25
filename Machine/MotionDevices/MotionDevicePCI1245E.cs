using System;
using System.Collections.Generic;
using Advantech.Motion;

namespace MachineClassLibrary.Machine.MotionDevices;

public class MotionDevicePCI1245E : MotionDevicePCI1240U
{
    private Dictionary<int, double> _storedSpeeds = new();
    public override void SetAxisConfig(int axisNum, MotionDeviceConfigs configs)
    {
        base.SetAxisConfig(axisNum, configs);
        double homeVelLow = configs.homeVelLow;
        double homeVelHigh = configs.homeVelHigh;
        var denominator = (uint)configs.denominator;
        _errors = new();
        _result = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.CFG_AxPPUDenominator, ref denominator, 4); _errors.Add(PropertyID.CFG_AxPPUDenominator, _result);
        _result = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.CFG_AxPulseInLogic, ref configs.plsInLogic, 4); _errors.Add(PropertyID.CFG_AxPulseInLogic, _result);
        //_result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxDirLogic, ref configs.axDirLogic, 4); _errors.Add(PropertyID.CFG_AxDirLogic, _result);
        _result = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.PAR_AxHomeVelLow, ref homeVelLow, 8); _errors.Add(PropertyID.PAR_AxHomeVelLow, _result);
        _result = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.PAR_AxHomeVelHigh, ref homeVelHigh, 8); _errors.Add(PropertyID.PAR_AxHomeVelHigh, _result);
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


    //public override async Task<double> MoveAxisPreciselyAsync(int axisNum, double lineCoefficient, double position, int rec = 0)
    //{
    //    if (rec > 20)
    //    {
    //        var result = position - CalcActualPosition(axisNum, lineCoefficient);
    //        return await Task.FromException<double>(new MotionException($"Cannot reach the accuracy. The current backlash is {result}", MotionExStatus.AccuracyNotReached));
    //    }
    //    Guard.IsLessThan(axisNum, _mAxishand.Length, $"{nameof(axisNum)} is invalid in the {nameof(MoveAxisPreciselyAsync)}");
    //    uint buf = 0;
    //    var id = _mAxishand[axisNum];
    //    var tolerance = _tolerance;
    //    //var backlash = 1;//mm
    //    var state = new ushort();
    //    buf = (uint)SwLmtEnable.SLMT_DIS;
    //    Motion.mAcm_SetProperty(id, (uint)PropertyID.CFG_AxSwPelEnable, ref buf, 4).CheckResult(axisNum);
    //    Motion.mAcm_SetProperty(id, (uint)PropertyID.CFG_AxSwMelEnable, ref buf, 4).CheckResult(axisNum);
    //    if (lineCoefficient != 0)
    //    {
    //        await Task.Delay(100);
    //        var newPos = CalcActualPosition(axisNum, lineCoefficient);
    //        Motion.mAcm_AxSetCmdPosition(id, newPos).CheckResult(axisNum);
    //        var diff = position - newPos;
    //        var gap = Math.Round(Math.Abs(diff), 3);
    //        if (gap > tolerance)
    //        {

    //            if (rec == 0)
    //            {
    //                _storedSpeeds[axisNum] = GetAxisVelocity(axisNum);
    //                Motion.mAcm_AxMoveAbs(id, position);
    //            }
    //            else
    //            {
    //                //var sign = Math.Sign(diff);
    //                //var positive = sign > 0;
    //                //buf = (uint)SwLmtEnable.SLMT_EN;
    //                //Motion.mAcm_SetF64Property(id, (uint)PropertyID.CFG_AxSwMelValue, position).CheckResult(axisNum);
    //                //Motion.mAcm_SetProperty(id, (uint)(positive ? PropertyID.CFG_AxSwPelEnable : PropertyID.CFG_AxSwMelEnable), ref buf, 4).CheckResult(axisNum);
    //                SetAxisVelocity(axisNum, 1);
    //                //Motion.mAcm_AxMoveRel(id, backlash * sign);
    //                Motion.mAcm_AxMoveAbs(id, position);

    //            }
    //            var token = new CancellationTokenSource(TimeSpan.FromMilliseconds(1000));
    //            await Task.Run(async () =>
    //            {
    //                uint status = 0;
    //                var slmtp = true;
    //                var slmtn = true;
    //                var rdy = false;
    //                var rrr = false;
    //                ushort st = 0;
    //                do
    //                {
    //                    await Task.Delay(10);
    //                    //Motion.mAcm_AxGetMotionIO(id, ref status);
    //                    Motion.mAcm_AxGetState(id, ref st);
    //                    //slmtp = (status & (uint)Ax_Motion_IO.AX_MOTION_IO_SLMTP) == 0;
    //                    //slmtn = (status & (uint)Ax_Motion_IO.AX_MOTION_IO_SLMTN) == 0;
    //                    rdy = (AxState)st == AxState.STA_AX_ERROR_STOP;
    //                    rrr = (AxState)st == AxState.STA_AX_READY;
    //                } while ((!rdy || slmtp && slmtn && !token.Token.IsCancellationRequested) && !rrr);
    //            }, token.Token).ConfigureAwait(false);

    //            //buf = (uint)SwLmtEnable.SLMT_DIS;
    //            //Motion.mAcm_SetProperty(id, (uint)PropertyID.CFG_AxSwPelEnable, ref buf, 4).CheckResult(axisNum);
    //            //Motion.mAcm_SetProperty(id, (uint)PropertyID.CFG_AxSwMelEnable, ref buf, 4).CheckResult(axisNum);
    //            Motion.mAcm_AxResetError(id).CheckResult();
    //            ushort st = 0;
    //            Motion.mAcm_AxGetState(id, ref st);
    //            if ((AxState)st != AxState.STA_AX_READY)
    //            {
    //                var result = (AxState)st;
    //                throw new MotionException($"Reset axis errors failed. axis number {axisNum}");
    //            }
    //            await MoveAxisPreciselyAsync(axisNum, lineCoefficient, position, ++rec);
    //            rec--;
    //            if (rec == 0)
    //            {
    //                Motion.mAcm_AxResetError(id).CheckResult(axisNum);
    //                SetAxisVelocity(axisNum, _storedSpeeds[axisNum]);
    //            }
    //        }
    //        return position - CalcActualPosition(axisNum, lineCoefficient);
    //    }
    //    else
    //    {
    //        await Task.Run(() =>
    //        {
    //            Motion.mAcm_AxMoveAbs(id, position);
    //            do
    //            {
    //                Task.Delay(1).Wait();
    //                Motion.mAcm_AxGetState(id, ref state);
    //            } while ((AxState)state == AxState.STA_AX_PTP_MOT/*.STA_AX_WAIT_PTP*/);
    //        });
    //        return 0d;
    //    }
    //}
}
