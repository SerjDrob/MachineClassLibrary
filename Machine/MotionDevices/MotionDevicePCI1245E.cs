﻿using Advantech.Motion;
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

            Motion.mAcm_GetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxPPU, ref ppu, ref bufLength);
            var pos = (int)(position * ppu);
            buf = (uint)SwLmtEnable.SLMT_DIS;
            res = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelEnable, ref buf, 4);
            res = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelEnable, ref buf, 4);
            if (lineCoefficient != 0)
            {
                var diff = position - CalcActualPosition(axisNum, lineCoefficient);
                if (Math.Abs(diff) > tolerance)
                {
                    Motion.mAcm_AxResetError(_mAxishand[axisNum]);
                    buf = (uint)SwLmtReact.SLMT_IMMED_STOP;
                    res = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelReact, ref buf, 4);
                    res = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelReact, ref buf, 4);
                    var tol = 0;
                    switch (Math.Sign(diff))
                    {
                        case 1:
                            res = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelValue, ref pos, 4);
                            buf = (uint)SwLmtEnable.SLMT_EN;
                            res = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelEnable, ref buf, 4);
                            buf = (uint)SwLmtToleranceEnable.TOLERANCE_ENABLE;
                            res = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelToleranceEnable, ref buf, 4);
                            res = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelToleranceValue, ref tol, 4);
                            direction = (ushort)VelMoveDir.DIR_POSITIVE;
                            break;

                        case -1:
                            res = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelValue, ref pos, 4);
                            buf = (uint)SwLmtEnable.SLMT_EN;
                            res = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelEnable, ref buf, 4);
                            buf = (uint)SwLmtToleranceEnable.TOLERANCE_ENABLE;
                            res = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelToleranceEnable, ref buf, 4);
                            res = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwMelToleranceValue, ref tol, 4);
                            direction = (ushort)VelMoveDir.DIR_NEGATIVE;
                            break;
                    }

                    Motion.mAcm_AxMoveVel(_mAxishand[axisNum], direction);
                    uint status = 0;
                    uint slmtp = 0;
                    uint slmtn = 0;
                    await Task.Run(() =>
                    {
                        do
                        {
                            Task.Delay(1).Wait();
                            //Thread.Sleep(1);
                            Motion.mAcm_AxGetMotionIO(_mAxishand[axisNum], ref status);
                            slmtp = status & (uint)Ax_Motion_IO.AX_MOTION_IO_SLMTP;
                            slmtn = status & (uint)Ax_Motion_IO.AX_MOTION_IO_SLMTN;
                        } while (slmtp == 0 & slmtn == 0);
                    }
                    );
                    SetAxisVelocity(axisNum, 1);
                    Motion.mAcm_AxSetCmdPosition(_mAxishand[axisNum], CalcActualPosition(axisNum, lineCoefficient));
                    await MoveAxisPreciselyAsync(axisNum, lineCoefficient, position, ++rec);
                    rec--;
                    if (rec == 0)
                    {
                        SetAxisVelocity(axisNum, _storeSpeed);
                        Motion.mAcm_AxResetError(_mAxishand[axisNum]);
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
