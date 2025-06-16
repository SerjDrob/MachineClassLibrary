using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Advantech.Motion;
using MachineClassLibrary.Classes;
using Microsoft.Toolkit.Diagnostics;
using AxState = Advantech.Motion.AxisState;

namespace MachineClassLibrary.Machine.MotionDevices
{
    public class MotionDevicePCIE1245 : IMotionDevicePCI1240U
    {

        public MotionDevicePCIE1245()
        {
            _deviceCount++;
            _bridges = new Dictionary<int, int>();
            _tolerance = 0.001;
            Motion2.mAcm2_DevInitialize().CheckResult2();
            Motion2.mAcm2_SetProperty(_devID, (uint)PropertyID2.CFG_DevEmgLogic, 0d).CheckResult2();
        }

        private uint _devID;
        public int AxisCount
        {
            get; private set;
        }
        //protected IntPtr[] _mAxishand;
        protected uint _result;
        protected Dictionary<PropertyID, uint> _errors = new();
        protected bool _initErrorsDictionaryInBaseClass = true;
        private List<AxGroup> _mGpHand;
        protected double _storeSpeed;
        private Dictionary<int, double> _storedSpeeds = new();
        private readonly Dictionary<int, int> _bridges;
        private AxisState[] _axisStates;
        protected double _tolerance;
        private Axis[] _axisLogicalIDList;
        private double _diChannelPerDev;
        private double _doChannelPerDev;
        private static ptrRegCallBack motDonePtrCallBack;
        private static ptrRegCallBack vhStartPtrCallBack;
        private static ptrRegCallBack vhEndPtrCallBack;
        private static int _deviceCount = 0;
        public static IntPtr DeviceHandle
        {
            get; private set;
        }

        public event EventHandler<AxNumEventArgs> TransmitAxState;

        // public event Action<string, int> ThrowMessage;

        public async Task<bool> DevicesConnection()
        {
            try
            {
                AxisCount = GetAxisCount();
                _axisStates = new AxisState[AxisCount];
            }
            //catch (MotionException e)
            //{
            //    MessageBox.Show(e.Message);
            //    return false;
            //}
            finally { }

            var axisEnableEvent = new uint[AxisCount];
            var gpEnableEvent = new uint[1];
            var axesPerDev = 0d;
            //_mAxishand = new IntPtr[AxisCount];
            var result = Motion2.mAcm2_GetProperty(_devID, (uint)PropertyID2.FT_DevAxesCount, ref axesPerDev);
            if (!Success(result))
            {
                throw new MotionException("Get Axis Number Failed With Error Code: [0x" + Convert.ToString(result, 16) + "]");
            }
            var logicalAxisCount = (uint)axesPerDev;
            var axIDs = new uint[logicalAxisCount];
            //Get Select Device Contains LogicalAxes
            result = Motion2.mAcm2_GetMappedLogicalIDList(ADV_OBJ_TYPE.ADV_AXIS, _devID, axIDs, ref logicalAxisCount);
            if (!Success(result))
            {
                throw new MotionException("Get AxisLogicalID List Failed With Error Code: [0x" + Convert.ToString(result, 16) + "]");
            }
            else
            {
                _axisLogicalIDList = axIDs.Select(id => new Axis(id)).ToArray();
            }
            result = Motion2.mAcm2_GetProperty(_devID, (uint)PropertyID2.FT_DaqDiMaxChan, ref _diChannelPerDev);
            if (!Success(result))
            {
                throw new MotionException("Get DI Number Failed With Error Code: [0x" + Convert.ToString(result, 16) + "]");
            }

            result = Motion2.mAcm2_GetProperty(_devID, (uint)PropertyID2.FT_DaqDoMaxChan, ref _doChannelPerDev);
            if (!Success(result))
            {
                throw new MotionException("Get DO Number Failed With Error Code: [0x" + Convert.ToString(result, 16) + "]");
            }

            motDonePtrCallBack = new ptrRegCallBack(AxMotDoneCallback);
            vhStartPtrCallBack = new ptrRegCallBack(AxVHStartCallback);
            vhEndPtrCallBack = new ptrRegCallBack(AxVHEndCallback);


            for (var i = 0; i < AxisCount; i++)
            {
                double cmdPosition = 0;

                var state = 0u;

                Motion2.mAcm2_AxGetState(_axisLogicalIDList[i].ID, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
                if (state!=(uint)AxState.STA_AX_READY) Motion2.mAcm2_AxMotionStop([_axisLogicalIDList[i].ID], MOTION_STOP_MODE.MOTION_STOP_MODE_EMG, 180);
                var token = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;
                await Task.Run(async () =>
                {
                    do
                    {
                        await Task.Delay(100);
                        Motion2.mAcm2_AxGetState(_axisLogicalIDList[i].ID, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
                    } while (state == (uint)AxState.STA_AX_STOPPING && !token.IsCancellationRequested);
                },token);
                Motion2.mAcm2_AxGetState(_axisLogicalIDList[i].ID, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
                if (state == (uint)AxState.STA_AX_ERROR_STOP) Motion2.mAcm2_AxResetError(_axisLogicalIDList[i].ID).CheckResult2(i);
                Motion2.mAcm2_AxGetState(_axisLogicalIDList[i].ID, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
                if (state != (uint)AxState.STA_AX_READY) return false;
                Motion2.mAcm2_AxSetPosition(_axisLogicalIDList[i].ID, POSITION_TYPE.POSITION_CMD, cmdPosition).CheckResult2(i);
                Motion2.mAcm2_AxSetPosition(_axisLogicalIDList[i].ID, POSITION_TYPE.POSITION_ACT, cmdPosition).CheckResult2(i);
                Motion2.mAcm2_EnableCallBackFuncForOneEvent(_axisLogicalIDList[i].ID, ADV_EVENT_SUBSCRIBE.AXIS_MOTION_DONE, motDonePtrCallBack).CheckResult2(i);
                Motion2.mAcm2_EnableCallBackFuncForOneEvent(_axisLogicalIDList[i].ID, ADV_EVENT_SUBSCRIBE.AXIS_VH_START, vhStartPtrCallBack).CheckResult2(i);
                Motion2.mAcm2_EnableCallBackFuncForOneEvent(_axisLogicalIDList[i].ID, ADV_EVENT_SUBSCRIBE.AXIS_VH_END, vhEndPtrCallBack).CheckResult2(i);
            }

            return true;
        }

        private uint AxVHEndCallback(uint axId, IntPtr UserParameter)
        {
            _axisStates[axId] = _axisStates[axId].AlterVHEnd(true);//TODO this is id not the number
            _axisStates[axId] = _axisStates[axId].AlterVHStart(false);

            return 0;
        }

        private uint AxVHStartCallback(uint axId, IntPtr UserParameter)
        {
            _axisStates[axId] = _axisStates[axId].AlterVHStart(true);
            _axisStates[axId] = _axisStates[axId].AlterVHEnd(false);
            _axisStates[axId] = _axisStates[axId].AlterMotDone(false);

            return 0;
        }

        private uint AxMotDoneCallback(uint axId, IntPtr UserParameter)
        {
            _axisStates[axId] = _axisStates[axId].AlterMotDone(true);
            _axisStates[axId] = _axisStates[axId].AlterVHEnd(false);
            _axisStates[axId] = _axisStates[axId].AlterVHStart(false);

            return 0;
        }

        public async Task StartMonitoringAsync()
        {
            try
            {
                await DeviceStateMonitorAsync();
            }
            catch (Exception ex)
            {
                throw new MotionException($"{nameof(DeviceStateMonitorAsync)} failed with exception {ex.Message}", ex);
            }
        }
        private async Task DeviceStateMonitorAsync()
        {
            var axEvtStatusArray = new uint[AxisCount];
            var gpEvtStatusArray = new uint[1];
            var ioStatus = new MOTION_IO();
            var cmdPosition = 0d;
            var actPosition = 0d;
            var bitData = new uint();
            var homeDone = false;
            var nLmt = false;
            var pLmt = false;
            var ez = false;
            var org = false;

            while (true)
            {
                var outState = 0;
                for (var channel = 0u; channel < _doChannelPerDev; channel++)
                {
                    Motion2.mAcm2_ChGetDOBit(channel, ref bitData).CheckResult2();
                    outState = bitData != 0 ? outState.SetBit((int)channel) : outState.ResetBit((int)channel);
                }

                for (int num = 0; num < _axisLogicalIDList.Length; num++)
                {
                    var ax = _axisLogicalIDList[num].ID;
                    Motion2.mAcm2_AxGetMotionIO(ax, ref ioStatus).CheckResult2(num);
                    nLmt = ioStatus.LMT_N > 0;
                    pLmt = ioStatus.LMT_P > 0;
                    ez = ioStatus.EZ > 0;
                    org = ioStatus.ORG > 0;
                    var sensorsState = 0;
                    var id = _axisLogicalIDList[num].ID;
                    for (var channel = 0u; channel < _diChannelPerDev; channel++)
                    {
                        Motion2.mAcm2_ChGetDIBit(channel, ref bitData).CheckResult2(num);
                        sensorsState = bitData != 0 ? sensorsState.SetBit((int)channel) : sensorsState.ResetBit((int)channel);
                    }

                    if (_bridges?.TryGetValue(num, out var bridge) ?? false) sensorsState |= bridge;

                    Motion2.mAcm2_AxGetPosition(id, POSITION_TYPE.POSITION_CMD, ref cmdPosition).CheckResult2(num);
                    Motion2.mAcm2_AxGetPosition(id, POSITION_TYPE.POSITION_ACT, ref actPosition).CheckResult2(num);

                    var st = _axisStates[id];
                    var axState = new AxisState
                    (
                        cmdPosition,
                        actPosition,
                        sensorsState,
                        outState,
                        pLmt,
                        nLmt,
                        st.motionDone,
                        homeDone,
                        st.vhStart,
                        st.vhEnd,
                        ez,
                        org
                    );
                    _axisStates[id] = axState;
                    try
                    {
                        TransmitAxState?.Invoke(this, new AxNumEventArgs(num, axState));
                    }
                    catch (Exception ex)
                    {
                        await Console.Error.WriteLineAsync(ex.Message);
                    }
                }
                await Task.Delay(1).ConfigureAwait(false);
            }

        }
        public int FormAxesGroup(int[] axisNums)
        {
            _mGpHand ??= new List<AxGroup>();
            var hand = (uint)_mGpHand.Count;
            var axesCount = (uint)axisNums.Count();
            var axes = axisNums.Select(n => _axisLogicalIDList[n].ID).ToArray();
            //clear axes count in the group
            Motion2.mAcm2_GpCreate(hand, axes, 0u).CheckResult2();
            Motion2.mAcm2_AxResetAllError();//.mAcm2_DevResetAllError();
            //create group
            Motion2.mAcm2_GpCreate(hand, axes, axesCount).CheckResult2();
            var group = new AxGroup(hand, axisNums.Select(num => _axisLogicalIDList[num].ID).ToArray());
            _mGpHand.Add(group);
            return _mGpHand.IndexOf(group);
        }
        public void MoveAxisContiniouslyAsync(int axisNum, AxDir dir)
        {
            Guard.IsLessThan(axisNum, _axisLogicalIDList.Length, $"{nameof(axisNum)} in the {nameof(MoveAxisContiniouslyAsync)}");
            uint state = default;
            Motion2.mAcm2_AxGetState(_axisLogicalIDList[axisNum].ID, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
            if ((AxState)state == AxState.STA_AX_BUSY
                || (AxState)state == AxState.STA_AX_CONTI_MOT
                || (AxState)state == AxState.STA_AX_DISABLE
                || (AxState)state == AxState.STA_AX_HOMING
                || (AxState)state == AxState.STA_AX_PTP_MOT
                || (AxState)state == AxState.STA_AX_STOPPING) return;
            var crossdistance = 200d;
            Motion2.mAcm2_AxPTP(_axisLogicalIDList[axisNum].ID, ABS_MODE.MOVE_REL, (dir == AxDir.Pos ? 1 : -1) * crossdistance).CheckResult2(axisNum);
            //Motion2.mAcm2_AxMoveContinue(_axisLogicalIDList[axisNum].ID, dir switch
            //{
            //    AxDir.Pos => MOTION_DIRECTION.DIRECTION_POS,
            //    AxDir.Neg => MOTION_DIRECTION.DIRECTION_NEG,
            //    _ => throw new MotionException($"{nameof(MoveAxisContiniouslyAsync)} for {axisNum} axis failed 'cause direction {dir} is not valid ")
            //}).CheckResult2(axisNum);
        }
        public async Task MoveAxesByCoorsAsync((int axisNum, double position)[] ax)
        {
            if (ax.Any(ind => ind.axisNum >= _axisLogicalIDList.Length))
            {
                throw new MotionException($"Для настоящего устройства не определена ось № {ax.Max(num => num.axisNum)}");
            }


            foreach (var item in ax)
            {
                Motion2.mAcm2_AxPTP(_axisLogicalIDList[item.axisNum].ID, ABS_MODE.MOVE_ABS, item.position);
            }
        }
        public async Task MoveAxesByCoorsPrecAsync((int axisNum, double position, double lineCoefficient)[] ax)
        {
            if (ax.Any(ind => ind.axisNum >= _axisLogicalIDList.Length))
            {
                throw new MotionException($"Для настоящего устройства не определена ось № {ax.Max(num => num.axisNum)}");
            }

            var tasks = new List<Task<double>>(ax.Length);
            tasks = ax.Select(p => MoveAxisPreciselyAsync(p.axisNum, p.lineCoefficient, p.position)).ToList();
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        protected double CalcActualPosition(int axisNum, double lineCoefficient)
        {
            var position = new double();
            if (lineCoefficient != 0)
            {
                _result = Motion2.mAcm2_AxGetPosition(_axisLogicalIDList[axisNum].ID, POSITION_TYPE.POSITION_ACT, ref position);
                if (!Success(_result)) { throw new MotionException($"Get actual position Failed With Error Code: [0x{_result:X}]"); }

            }
            else
            {
                _result = Motion2.mAcm2_AxGetPosition(_axisLogicalIDList[axisNum].ID, POSITION_TYPE.POSITION_CMD, ref position);
                if (!Success(_result)) { throw new MotionException($"Get command position Failed With Error Code: [0x{_result:X}]"); }
            }

            return position/* * lineCoefficient*/;
        }
        public void SetAxisVelocity(int axisNum, double vel)
        {
            var velHigh = vel;
            var velLow = 0d;// vel / 2;TODO think it

            _axisLogicalIDList[axisNum].ChangeSpeed(velLow, velHigh);

            Motion2.mAcm2_SetProperty(_axisLogicalIDList[axisNum].ID, (uint)PropertyID2.PAR_AxVelHigh, velHigh).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(_axisLogicalIDList[axisNum].ID, (uint)PropertyID2.PAR_AxVelLow, velLow).CheckResult2(axisNum);

            //Motion2.mAcm2_AxSetSpeedProfile(_axisLogicalIDList[axisNum].ID, _axisLogicalIDList[axisNum].AxVel).CheckResult2(axisNum);
        }
        public void SetGroupVelocity(int groupNum)
        {
            Guard.IsLessThan(groupNum, _mGpHand.Count, $"{nameof(groupNum)} is not valid in the {nameof(SetGroupVelocity)}");
            throw new NotImplementedException(nameof(SetGroupVelocity));
            //uint buf = 4;
            //var axesInGroup = new uint();
            //var axisNum = new int();
            //_result = Motion2..mAcm_GetProperty(_mGpHand[groupNum], (uint)PropertyID.CFG_GpAxesInGroup, ref axesInGroup, ref buf);
            //if (!Success(_result))
            //{
            //    throw new MotionException($"Запрос осей в группе № {groupNum}. Ошибка: {(ErrorCode)_result}");
            //}
            //for (int i = 1; i < 5; i++)
            //{
            //    if ((axesInGroup & i) > 0)
            //    {
            //        axisNum = i - 1;
            //        break;
            //    }
            //}
            //var velHigh = new double();
            //buf = 8;
            //_result = Motion2.mAcm_GetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxVelHigh, ref velHigh, ref buf);
            //if (!Success(_result))
            //{
            //    throw new MotionException($"Запрос скорости для оси № {axisNum}. Ошибка: {(ErrorCode)_result}");
            //}
            //var velLow = velHigh / 2;
            //Motion2.mAcm_SetProperty(_mGpHand[groupNum], (uint)PropertyID.PAR_GpVelLow, ref velLow, 8).CheckResult();
            //Motion2.mAcm_SetProperty(_mGpHand[groupNum], (uint)PropertyID.PAR_GpVelHigh, ref velHigh, 8).CheckResult();
        }
        public void SetGroupVelocity(int groupNum, double velocity)
        {
            Guard.IsLessThan(groupNum, _mGpHand.Count, $"{nameof(groupNum)} is not valid in the {nameof(SetGroupVelocity)}");
            var velHigh = velocity;
            var velLow = velHigh / 2;
            Motion2.mAcm2_GpSetSpeedProfile(_mGpHand[groupNum].ID, _mGpHand[groupNum].GpVel);
        }
        public void SetBridgeOnAxisDin(int axisNum, int bitNum, bool setReset)
        {
            if (_bridges.Keys.Contains(axisNum))
            {
                var bridge = _bridges[axisNum];
                _bridges[axisNum] = setReset ? bridge.SetBit(bitNum) : bridge.ResetBit(bitNum);
            }
            else
            {
                if (setReset)
                {
                    int bridge = 0;
                    _bridges.Add(axisNum, bridge.SetBit(bitNum));
                }

            }
        }
        public void StopAxis(int axisNum)
        {
            Guard.IsLessThan(axisNum, _axisLogicalIDList.Length, $"{nameof(axisNum)} is invalid in the {nameof(StopAxis)}");
            Motion2.mAcm2_AxMotionStop([_axisLogicalIDList[axisNum].ID], MOTION_STOP_MODE.MOTION_STOP_MODE_DEC, _axisLogicalIDList[axisNum].AxVel.Dec);
        }
        public void ResetErrors(int axisNum = 888)
        {
            uint state = default;
            if (axisNum == 888)
            {
                Motion2.mAcm2_AxResetAllError();//.mAcm2_DevResetAllError();
                foreach (var handle in _axisLogicalIDList.Select(ax => ax.ID))
                {
                    Motion2.mAcm2_AxGetState(handle, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
                    if (state == (uint)AxState.STA_AX_ERROR_STOP) Motion2.mAcm2_AxResetError(handle);
                }
            }
            else
            {
                Guard.IsLessThan(axisNum, _axisLogicalIDList.Length, $"{nameof(axisNum)} is invalid in the {nameof(ResetErrors)}");
                Motion2.mAcm2_AxGetState(_axisLogicalIDList[axisNum].ID, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
                if (state == (uint)AxState.STA_AX_ERROR_STOP) Motion2.mAcm2_AxResetError(_axisLogicalIDList[axisNum].ID);
            }

        }
        public void SetAxisDout(int axisNum, ushort ch, bool val) => Motion2.mAcm2_ChSetDOBit(ch, val ? 1u : 0u).CheckResult2();
        public bool GetAxisDout(int axisNum, ushort ch)
        {
            uint data = default;
            Motion2.mAcm2_ChGetDOBit(ch, ref data).CheckResult2();
            return data != 0;
        }
        public virtual void SetAxisConfig(int axisNum, MotionDeviceConfigs configs)
        {
            Guard.IsLessThan(axisNum, _axisLogicalIDList.Length, $"{nameof(axisNum)} is invalid in the {nameof(SetAxisConfig)}");

            var acc = configs.acc;
            var dec = configs.dec;
            var jerk = configs.jerk;
            var jerkFac = 2d;
            var ppu = configs.ppu;
            double axMaxAcc = configs.maxAcc;
            double axMaxDec = configs.maxDec;
            var axisMaxVel = 4000000;
            double axMaxVel = axisMaxVel / ppu;
            var buf = (uint)SwLmtEnable.SLMT_DIS;
            var id = _axisLogicalIDList[axisNum].ID;
            double homeVelLow = configs.homeVelLow;
            double homeVelHigh = configs.homeVelHigh;
            var denominator = (uint)configs.denominator;

            var speedProfile = new SPEED_PROFILE_PRM
            {
                Acc = configs.acc,
                Dec = configs.dec,
                JerkFac = 5d,
                FH = axMaxVel,
                FL = axMaxVel / 2
            };

            var homingSpeedProfile = new SPEED_PROFILE_PRM
            {
                Acc = configs.acc,
                Dec = configs.dec,
                JerkFac = 5d,
                FH = configs.homeVelHigh,
                FL = configs.homeVelLow
            };

            if (_initErrorsDictionaryInBaseClass) _errors = new();


            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxHomeResetEnable, configs.reset).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxPPU, ppu).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxMaxAcc, axMaxAcc).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxMaxDec, axMaxDec).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxMaxVel, axMaxVel).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxPulseInMode, configs.plsInMde).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxPulseOutMode, configs.plsOutMde).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.PAR_AxHomeVelLow, homeVelLow).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.PAR_AxHomeVelHigh, homeVelHigh).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.PAR_AxAcc, acc).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.PAR_AxDec, dec).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.PAR_AxJerk, jerk).CheckResult2(axisNum);
            
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.PAR_AxJerkFactor, jerkFac).CheckResult2(axisNum);

            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.PAR_AxHomeAcc, acc).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.PAR_AxHomeDec, dec).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.PAR_AxHomeVelLow, homeVelLow).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.PAR_AxHomeVelHigh, homeVelHigh).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.PAR_AxHomeJerk, 1d).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.PAR_AxHomeJerkFactor, 5d).CheckResult2(axisNum);

            //Motion2.mAcm2_AxSetSpeedProfile(id, speedProfile).CheckResult2(axisNum);
            //Motion2.mAcm2_AxSetHomeSpeedProfile(id, homingSpeedProfile).CheckResult2(axisNum);

            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxSwPelEnable, (double)SwLmtEnable.SLMT_DIS).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxSwMelEnable, (double)SwLmtEnable.SLMT_DIS).CheckResult2(axisNum);

            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxElLogic, configs.hLmtLogic).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxEzLogic, (double)EZLogic.EZ_ACT_HIGH).CheckResult2(axisNum);

            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxPPUDenominator, denominator).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxPulseInLogic, configs.plsInLogic).CheckResult2(axisNum);

            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxAlmLogic, (double)AlarmLogic.ALM_ACT_LOW).CheckResult2(axisNum);
            //Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxAlmEnable, (double)AlarmEnable.ALM_DIS).CheckResult2(axisNum);

            var tol = 0;
            Motion2.mAcm2_AxResetError(id).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxSwPelReact, (double)SwLmtReact.SLMT_DEC_TO_STOP/*.SLMT_IMMED_STOP*/).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxSwMelReact, (double)SwLmtReact.SLMT_DEC_TO_STOP/*.SLMT_IMMED_STOP*/).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxSwPelToleranceEnable, (double)SwLmtToleranceEnable.TOLERANCE_ENABLE).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxSwPelToleranceValue, tol).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxSwMelToleranceEnable, (double)SwLmtToleranceEnable.TOLERANCE_ENABLE).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxSwMelToleranceValue, tol).CheckResult2(axisNum);


            _axisLogicalIDList[axisNum].AxVel = speedProfile;

            _initErrorsDictionaryInBaseClass = true;
        }
        public void SetGroupConfig(int gpNum, MotionDeviceConfigs configs)
        {
            Guard.IsLessThan(gpNum, _mGpHand.Count, $"{nameof(gpNum)} is invalid in the {nameof(SetGroupConfig)}");

            var acc = configs.acc;
            var dec = configs.dec;
            var jerk = configs.jerk;
            var ppu = configs.ppu;
            double axMaxAcc = 180;
            double axMaxDec = 180;
            double axMaxVel = 50;
            var id = _mGpHand[gpNum].ID;


            Motion2.mAcm2_SetProperty(id, (uint)PropertyID.CFG_GpMaxAcc, axMaxAcc).CheckResult2();
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID.CFG_GpMaxDec, axMaxDec).CheckResult2();
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID.CFG_GpMaxVel, axMaxVel).CheckResult2();
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID.CFG_GpPPU, ppu).CheckResult2();
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID.PAR_GpAcc, acc).CheckResult2();
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID.PAR_GpDec, dec).CheckResult2();
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID.PAR_GpJerk, jerk).CheckResult2();
        }
        protected double GetAxisVelocity(int axisNum)
        {
            Guard.IsLessThan(axisNum, _axisLogicalIDList.Length, $"{nameof(axisNum)} is invalid in the {nameof(GetAxisVelocity)}");
            var vel = 0d;
            Motion2.mAcm2_GetProperty(_axisLogicalIDList[axisNum].ID, (uint)PropertyID2.PAR_AxVelHigh, ref vel).CheckResult2(axisNum);
            return vel;
        }
        private bool ResetAxErrorsWithEmgStop(int axisNum)
        {
            var id = _axisLogicalIDList[axisNum].ID;
            var dec = _axisLogicalIDList[axisNum].AxVel.Dec;
            uint axState = default;
            Motion2.mAcm2_AxGetState(id, AXIS_STATUS_TYPE.AXIS_STATE, ref axState);
            if (axState != (uint)AxState.STA_AX_READY)
            {
                //Motion2.mAcm2_AxMotionStop([id], MOTION_STOP_MODE.MOTION_STOP_MODE_EMG, dec).CheckResult2();
                Motion2.mAcm2_AxResetError(id).CheckResult2();
            }

            Motion2.mAcm2_AxGetState(id, AXIS_STATUS_TYPE.AXIS_STATE, ref axState);
            return axState == (uint)AxState.STA_AX_READY;
        }
        //public async Task<double> MoveAxisPreciselyAsync(int axisNum, double lineCoefficient, double position, int rec = 0)
        //{
        //    if (rec > 20)
        //    {
        //        var result = position - CalcActualPosition(axisNum, lineCoefficient);
        //        return await Task.FromException<double>(new MotionException($"Cannot reach the accuracy. The current backlash is {result}", MotionExStatus.AccuracyNotReached));
        //    }
        //    Guard.IsLessThan(axisNum, _axisLogicalIDList.Length, $"{nameof(axisNum)} is invalid in the {nameof(MoveAxisPreciselyAsync)}");
        //    var id = _axisLogicalIDList[axisNum].ID;
        //    var tolerance = _tolerance;
        //    var backlash = 2;//mm
        //    var state = new uint();
        //    var direction = MOTION_DIRECTION.DIRECTION_POS;

        //    Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxSwPelEnable, (double)SwLmtEnable.SLMT_DIS).CheckResult2(axisNum);
        //    Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxSwMelEnable, (double)SwLmtEnable.SLMT_DIS).CheckResult2(axisNum);
        //    if (lineCoefficient != 0)
        //    {
        //        await Task.Delay(100);
        //        var newPos = CalcActualPosition(axisNum, lineCoefficient);
        //        Motion2.mAcm2_AxSetPosition(id, POSITION_TYPE.POSITION_CMD, newPos).CheckResult2(axisNum);
        //        var diff = position - newPos;
        //        var gap = Math.Round(Math.Abs(diff), 3);
        //        if (gap > tolerance)
        //        {
        //            var sign = Math.Sign(diff);
        //            var positive = Math.Sign(diff) > 0;
        //            Motion2.mAcm2_SetProperty(id, (uint)(positive ? PropertyID2.CFG_AxSwPelValue : PropertyID2.CFG_AxSwMelValue), position).CheckResult2(axisNum);
        //            Motion2.mAcm2_SetProperty(id, (uint)(positive ? PropertyID2.CFG_AxSwPelEnable : PropertyID2.CFG_AxSwMelEnable), (double)SwLmtEnable.SLMT_EN).CheckResult2(axisNum);
        //            direction = positive ? MOTION_DIRECTION.DIRECTION_POS : MOTION_DIRECTION.DIRECTION_NEG;
        //            if (rec == 0)
        //            {
        //                _storedSpeeds[axisNum] = GetAxisVelocity(axisNum);
        //                Motion2.mAcm2_AxPTP(id, ABS_MODE.MOVE_ABS, position).CheckResult2(axisNum);
        //            }
        //            else
        //            {
        //                SetAxisVelocity(axisNum, 1);
        //                Motion2.mAcm2_AxPTP(id, ABS_MODE.MOVE_REL, backlash * sign);
        //            }
        //            var token = new CancellationTokenSource(TimeSpan.FromMilliseconds(1000));
        //            await Task.Run(async () =>
        //            {
        //                var status = new MOTION_IO();
        //                var slmtp = true;
        //                var slmtn = true;
        //                var rdy = false;
        //                var rrr = false;
        //                var st = 0u;
        //                do
        //                {
        //                    await Task.Delay(10);
        //                    Motion2.mAcm2_AxGetMotionIO(id, ref status);
        //                    Motion2.mAcm2_AxGetState(id, AXIS_STATUS_TYPE.AXIS_STATE, ref st);
        //                    slmtp = status.SLMT_P == 0;
        //                    slmtn = status.SLMT_N == 0;
        //                    rdy = (AxState)st == AxState.STA_AX_ERROR_STOP;
        //                    rrr = (AxState)st == AxState.STA_AX_READY;
        //                } while ((!rdy || slmtp && slmtn && !token.Token.IsCancellationRequested) && !rrr);
        //            }, token.Token).ConfigureAwait(false);

        //            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxSwPelEnable, (double)SwLmtEnable.SLMT_DIS).CheckResult2(axisNum);
        //            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxSwMelEnable, (double)SwLmtEnable.SLMT_DIS).CheckResult2(axisNum);
        //            Motion2.mAcm2_AxResetError(id).CheckResult2();
        //            var st = 0u;
        //            Motion2.mAcm2_AxGetState(id, AXIS_STATUS_TYPE.AXIS_STATE, ref st);
        //            if ((AxState)st != AxState.STA_AX_READY)
        //            {
        //                var result = (AxState)st;
        //                throw new MotionException($"Reset axis errors failed. axis number {axisNum}");
        //            }
        //            await MoveAxisPreciselyAsync(axisNum, lineCoefficient, position, ++rec);
        //            rec--;
        //            if (rec == 0)
        //            {
        //                Motion2.mAcm2_AxResetError(id).CheckResult2(axisNum);
        //                SetAxisVelocity(axisNum, _storedSpeeds[axisNum]);
        //            }
        //        }
        //        return position - CalcActualPosition(axisNum, lineCoefficient);
        //    }
        //    else
        //    {
        //        await Task.Run(() =>
        //        {
        //            Motion2.mAcm2_AxPTP(id, ABS_MODE.MOVE_ABS, position);
        //            do
        //            {
        //                Task.Delay(1).Wait();
        //                Motion2.mAcm2_AxGetState(id, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
        //            } while ((AxState)state == AxState.STA_AX_WAIT_PTP);
        //        });
        //        return 0d;
        //    }
        //}
        public async Task<double> MoveAxisPreciselyAsync(int axisNum, double lineCoefficient, double position, int rec = 0)
        {
            if (rec > 20)
            {
                var result = position - CalcActualPosition(axisNum, lineCoefficient);
                return await Task.FromException<double>(new MotionException($"Cannot reach the accuracy. The current backlash is {result}", MotionExStatus.AccuracyNotReached));
            }
            Guard.IsLessThan(axisNum, _axisLogicalIDList.Length, $"{nameof(axisNum)} is invalid in the {nameof(MoveAxisPreciselyAsync)}");
            var id = _axisLogicalIDList[axisNum].ID;
            var tolerance = _tolerance;
            var backlash = 2;//mm
            var state = new uint();
            var direction = MOTION_DIRECTION.DIRECTION_POS;

            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxSwPelEnable, (double)SwLmtEnable.SLMT_DIS).CheckResult2(axisNum);
            Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxSwMelEnable, (double)SwLmtEnable.SLMT_DIS).CheckResult2(axisNum);
            if (lineCoefficient != 0)
            {
                await Task.Delay(100);
                var newPos = CalcActualPosition(axisNum, lineCoefficient);
                Motion2.mAcm2_AxSetPosition(id, POSITION_TYPE.POSITION_CMD, newPos).CheckResult2(axisNum);
                var diff = position - newPos;
                var gap = Math.Round(Math.Abs(diff), 3);
                if (gap > tolerance)
                {
                    //var sign = Math.Sign(diff);
                    //var positive = Math.Sign(diff) > 0;
                    //Motion2.mAcm2_SetProperty(id, (uint)(positive ? PropertyID2.CFG_AxSwPelValue : PropertyID2.CFG_AxSwMelValue), position).CheckResult2(axisNum);
                    //Motion2.mAcm2_SetProperty(id, (uint)(positive ? PropertyID2.CFG_AxSwPelEnable : PropertyID2.CFG_AxSwMelEnable), (double)SwLmtEnable.SLMT_EN).CheckResult2(axisNum);
                    //direction = positive ? MOTION_DIRECTION.DIRECTION_POS : MOTION_DIRECTION.DIRECTION_NEG;
                    if (rec == 0)
                    {
                        _storedSpeeds[axisNum] = GetAxisVelocity(axisNum);
                        Motion2.mAcm2_AxPTP(id, ABS_MODE.MOVE_ABS, position).CheckResult2(axisNum);
                    }
                    else
                    {
                        SetAxisVelocity(axisNum, 1);
                        Motion2.mAcm2_AxPTP(id, ABS_MODE.MOVE_ABS, position).CheckResult2(axisNum);
                    }
                    var token = new CancellationTokenSource(TimeSpan.FromMilliseconds(1000));
                    await Task.Run(async () =>
                    {
                        var status = new MOTION_IO();
                        var slmtp = true;
                        var slmtn = true;
                        var rdy = false;
                        var rrr = false;
                        var st = 0u;
                        do
                        {
                            await Task.Delay(10);
                            Motion2.mAcm2_AxGetMotionIO(id, ref status);
                            Motion2.mAcm2_AxGetState(id, AXIS_STATUS_TYPE.AXIS_STATE, ref st);
                            slmtp = status.SLMT_P == 0;
                            slmtn = status.SLMT_N == 0;
                            rdy = (AxState)st == AxState.STA_AX_ERROR_STOP;
                            rrr = (AxState)st == AxState.STA_AX_READY;
                        } while ((!rdy || slmtp && slmtn && !token.Token.IsCancellationRequested)&&!rrr);
                    }, token.Token).ConfigureAwait(false);

                    //Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxSwPelEnable, (double)SwLmtEnable.SLMT_DIS).CheckResult2(axisNum);
                    //Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.CFG_AxSwMelEnable, (double)SwLmtEnable.SLMT_DIS).CheckResult2(axisNum);
                    //Motion2.mAcm2_AxResetError(id).CheckResult2();
                    var st = 0u;
                    Motion2.mAcm2_AxGetState(id, AXIS_STATUS_TYPE.AXIS_STATE, ref st);
                    if ((AxState)st!=AxState.STA_AX_READY)
                    {
                        var result = (AxState)st;
                        throw new MotionException($"Reset axis errors failed. axis number {axisNum}");
                    }
                    await MoveAxisPreciselyAsync(axisNum, lineCoefficient, position, ++rec);
                    rec--;
                    if (rec == 0)
                    {
                        Motion2.mAcm2_AxResetError(id).CheckResult2(axisNum);
                        SetAxisVelocity(axisNum, _storedSpeeds[axisNum]);
                    }
                }
                return position - CalcActualPosition(axisNum, lineCoefficient);
            }
            else
            {
                await Task.Run(() =>
                {
                    Motion2.mAcm2_AxPTP(id, ABS_MODE.MOVE_ABS, position);
                    do
                    {
                        Task.Delay(1).Wait();
                        Motion2.mAcm2_AxGetState(id, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
                    } while ((AxState)state !=AxState.STA_AX_READY /*== AxState.STA_AX_WAIT_PTP*/);
                });
                return 0d;
            }
        }
        public async Task<double> MoveAxisPreciselyAsync_2(int axisNum, double lineCoefficient, double position, int rec = 0)
        {
            if (rec > 20)
            {
                var result = position - CalcActualPosition(axisNum, lineCoefficient);
                return await Task.FromException<double>(new MotionException($"Cannot reach the accuracy. The current backlash is {result}", MotionExStatus.AccuracyNotReached));
            }
            Guard.IsLessThan(axisNum, _axisLogicalIDList.Length, $"{nameof(axisNum)} is invalid in the {nameof(MoveAxisPreciselyAsync)}");
            var id = _axisLogicalIDList[axisNum].ID;
            var tolerance = _tolerance;
            var state = new uint();
            if (rec == 0)
            {
                _storedSpeeds[axisNum] = GetAxisVelocity(axisNum);
                var token = new CancellationTokenSource(TimeSpan.FromMilliseconds(15000));
                Motion2.mAcm2_AxPTP(id, ABS_MODE.MOVE_ABS, position).CheckResult2(axisNum);
                await Task.Run(async () =>
                {
                    do
                    {
                        await Task.Delay(10);
                        Motion2.mAcm2_AxGetState(id, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
                    } while (!token.Token.IsCancellationRequested && (AxState)state == AxState.STA_AX_PTP_MOT);
                }, token.Token).ConfigureAwait(false);
            }

            if (lineCoefficient != 0)
            {
                var diff = position - CalcActualPosition(axisNum, lineCoefficient);
                var gap = Math.Round(Math.Abs(diff), 3);
                if (gap > tolerance)
                {
                    var sign = Math.Sign(diff);
                    SetAxisVelocity(axisNum, 1);
                    Motion2.mAcm2_AxPTP(id, ABS_MODE.MOVE_REL, gap * sign);
                    var token = new CancellationTokenSource(TimeSpan.FromMilliseconds(1000));
                    await Task.Run(async () =>
                    {
                        do
                        {
                            await Task.Delay(10);
                            Motion2.mAcm2_AxGetState(id, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
                        } while (!token.Token.IsCancellationRequested && (AxState)state == AxState.STA_AX_PTP_MOT);
                    }, token.Token).ConfigureAwait(false);

                    var st = 0u;
                    Motion2.mAcm2_AxGetState(id, AXIS_STATUS_TYPE.AXIS_STATE, ref st);
                    if ((AxState)st != AxState.STA_AX_READY)
                    {
                        var result = (AxState)st;
                        throw new MotionException($"Reset axis errors failed. axis number {axisNum}");
                    }
                    await MoveAxisPreciselyAsync_2(axisNum, lineCoefficient, position, ++rec);
                    rec--;
                    if (rec == 0)
                    {
                        Motion2.mAcm2_AxResetError(id).CheckResult2(axisNum);
                        SetAxisVelocity(axisNum, _storedSpeeds[axisNum]);
                    }
                }
                return position - CalcActualPosition(axisNum, lineCoefficient);
            }
            else
            {
                await Task.Run(() =>
                {
                    Motion2.mAcm2_AxPTP(id, ABS_MODE.MOVE_ABS, position);
                    do
                    {
                        Task.Delay(1).Wait();
                        Motion2.mAcm2_AxGetState(id, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
                    } while ((AxState)state == AxState.STA_AX_WAIT_PTP);
                });
                return 0d;
            }
        }
        public void ResetAxisCounter(int axisNum)
        {
            Motion2.mAcm2_AxSetPosition(_axisLogicalIDList[axisNum].ID, POSITION_TYPE.POSITION_CMD, 0d).CheckResult2(axisNum);
            Motion2.mAcm2_AxSetPosition(_axisLogicalIDList[axisNum].ID, POSITION_TYPE.POSITION_ACT, 0d).CheckResult2(axisNum);
        }
        public async Task HomeMovingAsync((int axisNum, double vel, uint mode)[] axVels)
        {
            uint state = default;
            foreach (var axis in axVels)
            {
                Motion2.mAcm2_AxGetState(_axisLogicalIDList[axis.axisNum].ID, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
                if ((AxState)state == AxState.STA_AX_HOMING)
                {
                    return;
                }
            }

            ResetErrors();

            var homings = new List<Task>();
            foreach (var axvel in axVels)
            {
                try
                {
                    SetAxisVelocity(axvel.axisNum, axvel.vel);
                }
                catch (Exception ex)
                {
                    throw new MotionException($"In the {nameof(HomeMovingAsync)} method {nameof(SetAxisVelocity)} failed for the axis number {axvel.axisNum} ", ex);
                }

                var result = Motion2.mAcm2_AxHome(_axisLogicalIDList[axvel.axisNum].ID, (HomeMode)axvel.mode, MOTION_DIRECTION.DIRECTION_NEG);

                if (!Success(result))
                {
                    throw new MotionException($"Ось № {axvel.axisNum} прервало движение домой с ошибкой {(ErrorCode)_result}");
                }

                homings.Add(Task.Run(() =>
                {
                    do
                    {
                        Motion2.mAcm2_AxGetState(_axisLogicalIDList[axvel.axisNum].ID, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
                    } while ((AxState)state == AxState.STA_AX_HOMING);
                }));
            }
            await Task.WhenAll(homings);
        }
        public async Task HomeMovingAsync((AxDir direction, HomeRst homeRst, HmMode homeMode, double velocity, int axisNum)[] axs)
        {
            ResetErrors();
            uint state = default;
            foreach (var axis in axs)
            {
                var id = _axisLogicalIDList[axis.axisNum].ID;
                Motion2.mAcm2_AxGetState(id, AXIS_STATUS_TYPE.AXIS_STATE, ref state);

                if ((AxState)state != AxState.STA_AX_READY)
                {
                    throw new MotionException($"Axis {axis.axisNum} isn't ready for homing");
                }
            }

            foreach (var axvel in axs)
            {
                var id = _axisLogicalIDList[axvel.axisNum].ID;
                /*
                var result = Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.PAR_AxHomeVelLow, axvel.velocity / 2);
                result = Motion2.mAcm2_SetProperty(id, (uint)PropertyID2.PAR_AxHomeVelHigh, axvel.velocity);
                */

                var result = Motion2.mAcm2_AxHome(id, (HomeMode)axvel.homeMode, (MOTION_DIRECTION)axvel.direction);

                if (!Success(result))
                {
                    throw new MotionException($"Ось № {axvel.axisNum} прервало движение домой с ошибкой {(ErrorCode)_result}");
                }
            }

            var tasks = axs.Select(ax => Task.Run(async () =>
            {
                var id = _axisLogicalIDList[ax.axisNum].ID;
                uint state = default;
                do
                {
                    Motion2.mAcm2_AxGetState(id, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
                    await Task.Delay(1).ConfigureAwait(false);
                } while ((AxState)state == AxState.STA_AX_HOMING);

            })).ToArray();
            await Task.WhenAll(tasks);
            foreach (var axis in axs.Select(a => a.axisNum))
            {
                ResetAxisCounter(axis);
            }
        }
        public async Task MoveGroupAsync(int groupNum, double[] position)
        {
            var elements = (uint)position.Length;
            uint state = default;
            var id = _mGpHand[groupNum].ID;
            Motion2.mAcm2_GpResetError(id);
            Motion2.mAcm2_GpMoveSplinearAbs(id, [position[0]], [position[1]], 1, ref elements);
            await Task.Run(() =>
            {
                do
                {
                    Task.Delay(10).Wait();
                    Motion2.mAcm2_GpGetState(id, ref state);
                } while ((GroupState)state == GroupState.STA_Gp_Motion);
            }).ConfigureAwait(false);
        }
        public async Task MoveGroupPreciselyAsync(int groupNum, double[] position, (int axisNum, double lineCoefficient)[] gpAxes)
        {
            var buf = (uint)SwLmtEnable.SLMT_DIS;
            for (var i = 0; i < gpAxes.Length; i++)
            {
                Motion2.mAcm2_SetProperty(_axisLogicalIDList[gpAxes[i].axisNum].ID, (uint)PropertyID.CFG_AxSwPelEnable, buf).CheckResult2();
                Motion2.mAcm2_SetProperty(_axisLogicalIDList[gpAxes[i].axisNum].ID, (uint)PropertyID.CFG_AxSwMelEnable, buf).CheckResult2();
            }


            await MoveGroupAsync(groupNum, position);
            for (var i = 0; i < gpAxes.Length; i++)
            {
                await MoveAxisPreciselyAsync(gpAxes[i].axisNum, gpAxes[i].lineCoefficient, position[i]);
            }
        }
        public async Task MoveAxisAsync(int axisNum, double position)
        {
            var id = _axisLogicalIDList[axisNum].ID;
            if (Math.Abs(_axisStates[id].cmdPos - position) < _tolerance) return;
            uint state = default;
            await Task.Run(() =>
            {
                Motion2.mAcm2_AxPTP(id, ABS_MODE.MOVE_ABS, position);
                do
                {
                    Task.Delay(1).Wait();
                    Motion2.mAcm2_AxGetState(id, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
                } while ((AxState)state != AxState.STA_AX_READY);
            });
        }
        public void SetAxisCoordinate(int axisNum, double coordinate)
        {
            var id = _axisLogicalIDList[axisNum].ID;
            uint state = default;
            Motion2.mAcm2_AxGetState(id, AXIS_STATUS_TYPE.AXIS_STATE, ref state);
            if ((AxState)state == AxState.STA_AX_READY)
            {
                Motion2.mAcm2_AxSetPosition(id, POSITION_TYPE.POSITION_CMD, coordinate);
                Motion2.mAcm2_AxSetPosition(id, POSITION_TYPE.POSITION_ACT, coordinate);
            }
        }
        private static IntPtr OpenDevice(in DEV_LIST device)
        {
            /*
            var deviceHandle = IntPtr.Zero;
            var result = Motion2.mAcm_DevOpen(device.DeviceNum, ref deviceHandle);
            if (!Success(result))
            {
                throw new MotionException($"Open Device Failed With Error Code: [0x{result:X}]");
            }
            return deviceHandle;
            */
            throw new NotImplementedException();
        }
        private static IEnumerable<DEV_LIST> GetAvailableDevs()
        {
            var availableDevs = new DEV_LIST[Motion.MAX_DEVICES];
            uint deviceCount = default;
            var result = Motion2.mAcm2_GetAvailableDevs(availableDevs, Motion.MAX_DEVICES, ref deviceCount);

            if (!Success(result))
            {
                throw new MotionException($"Get Device Numbers Failed With Error Code: [{result:X}]");
            }

            return availableDevs.Take((int)deviceCount);
        }
        private int GetAxisCount()
        {
            var axesPerDev = 0d;
            _result = Motion2.mAcm2_GetProperty(_devID, (uint)PropertyID.FT_DevAxesCount, ref axesPerDev);

            if (!Success(_result))
            {
                throw new MotionException($"Get Axis Number Failed With Error Code: [0x{_result:X}]");
            }

            return (int)axesPerDev;
        }
        private static bool Success(uint result) => result == (uint)ErrorCode.SUCCESS;
        private static bool Success(int result) => result == (int)ErrorCode.SUCCESS;
        private void ReleaseUnmanagedResources()
        {
            //var copy = DeviceHandle;

            //if (copy != IntPtr.Zero)
            //{
            //    Motion2.mAcm_DevClose(ref copy);
            //}
        }
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }
        public double GetAxActual(int axNum)
        {
            var pos = 0d;
            Motion2.mAcm2_AxGetPosition(_axisLogicalIDList[axNum].ID, POSITION_TYPE.POSITION_ACT, ref pos).CheckResult2(axNum);
            return pos;
        }
        public double GetAxCmd(int axNum)
        {
            var pos = 0d;
            Motion2.mAcm2_AxGetPosition(_axisLogicalIDList[axNum].ID, POSITION_TYPE.POSITION_CMD, ref pos).CheckResult2(axNum);
            return pos;
        }
        public void SetPrecision(double tolerance) => _tolerance = tolerance;
        public void CloseDevice()
        {
            var result = Motion2.mAcm2_DevAllClose();
        }

        public bool GetAxisReady(int axisNum)
        {
            var status = default(uint);
            Motion2.mAcm2_AxGetState(_axisLogicalIDList[axisNum].ID, AXIS_STATUS_TYPE.AXIS_STATE, ref status);
            return (AxState)status == AxState.STA_AX_READY;
        }
    }


    struct Axis
    {
        public Axis(uint id)
        {
            ID = id;
        }
        public readonly uint ID;
        public SPEED_PROFILE_PRM AxVel;
        public void SetSpeedProfile(SPEED_PROFILE_PRM profile) => AxVel = profile;
        public void ChangeSpeed(double low, double high)
        {
            AxVel.FL = low;
            AxVel.FH = high;
        }
    }

    struct AxGroup
    {
        public AxGroup(uint id, uint[] axes)
        {
            ID = id;
            AxesID = axes;
        }

        public readonly uint ID;
        public readonly uint[] AxesID;
        public SPEED_PROFILE_PRM GpVel;
        public void SetSpeedProfile(SPEED_PROFILE_PRM profile) => GpVel = profile;
        public void ChangeSpeed(double low, double high)
        {
            GpVel.FL = low;
            GpVel.FH = high;
        }
    }

}
