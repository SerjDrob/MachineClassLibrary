using Advantech.Motion;
using MachineClassLibrary.Classes;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineClassLibrary.Machine.MotionDevices
{

    public class MotionDevicePCI1240U : /*IDisposable, IMessager,*/ IMotionDevicePCI1240U
    {

        public MotionDevicePCI1240U()
        {
            _bridges = new Dictionary<int, int>();
            var device = GetAvailableDevs().First();
            DeviceHandle = OpenDevice(device);
        }

        public int AxisCount { get; private set; }
        protected IntPtr[] _mAxishand;
        protected uint _result;
        protected Dictionary<PropertyID, uint> _errors = new Dictionary<PropertyID, uint>();
        protected bool _initErrorsDictionaryInBaseClass = true;
        private List<IntPtr> _mGpHand;
        protected double _storeSpeed;
        private Dictionary<int, int> _bridges;
        private AxisState[] _axisStates;
        public static IntPtr DeviceHandle { get; private set; }

        public event EventHandler<AxNumEventArgs> TransmitAxState;

        public event Action<string, int> ThrowMessage;

        public bool DevicesConnection()
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

            _mAxishand = new IntPtr[AxisCount];
            for (var i = 0; i < axisEnableEvent.Length; i++)
            {
                Motion.mAcm_AxOpen(DeviceHandle, (ushort)i, ref _mAxishand[i]).CheckResult(i);

                double cmdPosition = 0;

                ushort state = 0;

                Motion.mAcm_AxGetState(_mAxishand[i], ref state);

                if(state == 3) Motion.mAcm_AxResetError(_mAxishand[i]).CheckResult(i);

                Motion.mAcm_AxSetCmdPosition(_mAxishand[i], cmdPosition).CheckResult(i);

                Motion.mAcm_AxSetActualPosition(_mAxishand[i], cmdPosition).CheckResult(i);

                axisEnableEvent[i] |= (uint)EventType.EVT_AX_MOTION_DONE;
                axisEnableEvent[i] |= (uint)EventType.EVT_AX_VH_START;
                axisEnableEvent[i] |= (uint)EventType.EVT_AX_VH_END;
            }

            Motion.mAcm_EnableMotionEvent(DeviceHandle, axisEnableEvent, gpEnableEvent, (uint)AxisCount, 1).CheckResult();

            return true;
        }
        public async Task StartMonitoringAsync()
        {
           await DeviceStateMonitorAsync();
        }

#if NOTTEST
private async Task DeviceStateMonitorAsync()
        {
            var axEvtStatusArray = new uint[4];
            var gpEvtStatusArray = new uint[1];
            var eventResult = new uint();
            var ioStatus = new uint();
            var position = new double();
            var bitData = new byte();


            while (true)
            {
                eventResult = Motion.mAcm_CheckMotionEvent(DeviceHandle, axEvtStatusArray, gpEvtStatusArray, (uint)AxisCount, 0, 10);
                for (int num = 0; num < _mAxishand.Length; num++)
                {
                    var axState = new AxisState();
                    IntPtr ax = _mAxishand[num];
                    Motion.mAcm_AxGetMotionIO(ax, ref ioStatus).CheckResult();
                    
                    axState.nLmt = (ioStatus & (uint)Ax_Motion_IO.AX_MOTION_IO_LMTN) > 0;
                    axState.pLmt = (ioStatus & (uint)Ax_Motion_IO.AX_MOTION_IO_LMTP) > 0;

                    for (var channel = 0; channel < 4; channel++)
                    {
                        Motion.mAcm_AxDiGetBit(ax, (ushort)channel, ref bitData).CheckResult();
                        axState.sensors = bitData != 0 ? axState.sensors.SetBit(channel) : axState.sensors.ResetBit(channel);
                    }

                    var bridge = 0;
                    if (_bridges != null && _bridges.Keys.Contains(num))
                    {
                        bridge = _bridges[num];
                    }
                    var sensors = axState.sensors;
                    axState.sensors |= bridge;

                    if (Success(Motion.mAcm_AxDoGetByte(ax, 0, ref bitData))) axState.outs = bitData;

                    _result = Motion.mAcm_AxGetCmdPosition(ax, ref position);
                    if (Success(_result)) axState.cmdPos = position;

                    _result = Motion.mAcm_AxGetActualPosition(ax, ref position);
                    if (Success(_result)) axState.actPos = position;

                    if (Success(eventResult))
                    {
                        axState.motionDone = (axEvtStatusArray[num] & (uint)EventType.EVT_AX_MOTION_DONE) > 0;
                        //axState.homeDone = (axEvtStatusArray[num] & (uint)EventType.EVT_AX_HOME_DONE) > 0;
                        axState.vhStart = (axEvtStatusArray[num] & (uint)EventType.EVT_AX_VH_START) > 0;
                        axState.vhEnd = (axEvtStatusArray[num] & (uint)EventType.EVT_AX_VH_END) > 0;
                    }

                    TransmitAxState?.Invoke(this, new AxNumEventArgs(num, axState));
                }
                await Task.Delay(1).ConfigureAwait(false);
            }
        }


#endif
        private async Task DeviceStateMonitorAsync()
        {
            var axEvtStatusArray = new uint[AxisCount];
            var gpEvtStatusArray = new uint[1];
            var eventResult = new uint();
            var ioStatus = new uint();
            var cmdPosition = 0d;
            var actPosition = 0d;
            var bitData = new byte();
            var motionDone = false;
            var homeDone = false;
            var vhStart = false;
            var vhEnd = false;
            var nLmt = false;
            var pLmt = false;

            while (true)
            {
                eventResult = Motion.mAcm_CheckMotionEvent(DeviceHandle, axEvtStatusArray, gpEvtStatusArray, (uint)AxisCount, 0, 10);
                for (int num = 0; num < _mAxishand.Length; num++)
                {
                    //var axState = new AxisState();
                    IntPtr ax = _mAxishand[num];
                    Motion.mAcm_AxGetMotionIO(ax, ref ioStatus).CheckResult();

                    nLmt = (ioStatus & (uint)Ax_Motion_IO.AX_MOTION_IO_LMTN) > 0;
                    pLmt = (ioStatus & (uint)Ax_Motion_IO.AX_MOTION_IO_LMTP) > 0;

                    var sensorsState = 0;

                    for (var channel = 0; channel < 4; channel++)
                    {
                        Motion.mAcm_AxDiGetBit(ax, (ushort)channel, ref bitData).CheckResult();
                        sensorsState = bitData != 0 ? sensorsState.SetBit(channel) : sensorsState.ResetBit(channel);
                    }


                    var bridge = 0;
                    if (_bridges != null && _bridges.Keys.Contains(num))
                    {
                        bridge = _bridges[num];
                    }
                    sensorsState |= bridge;

#if PCI1245
                    Motion.mAcm_AxDoGetByte(ax, 0, ref bitData).CheckResult(ax);
#endif

                    Motion.mAcm_AxGetCmdPosition(ax, ref cmdPosition).CheckResult(ax);
                    Motion.mAcm_AxGetActualPosition(ax, ref actPosition).CheckResult(ax);

                    if (Success(eventResult))
                    {
                        motionDone = (axEvtStatusArray[num] & (uint)EventType.EVT_AX_MOTION_DONE) > 0;
                        //axState.homeDone = (axEvtStatusArray[num] & (uint)EventType.EVT_AX_HOME_DONE) > 0;
                        vhStart = (axEvtStatusArray[num] & (uint)EventType.EVT_AX_VH_START) > 0;
                        vhEnd = (axEvtStatusArray[num] & (uint)EventType.EVT_AX_VH_END) > 0;
                    }


                    var axState = new AxisState
                    (
                        cmdPosition,
                        actPosition,
                        sensorsState,
                        bitData,
                        pLmt,
                        nLmt,
                        motionDone,
                        homeDone,
                        vhStart,
                        vhEnd
                    );
                    TransmitAxState?.Invoke(this, new AxNumEventArgs(num, axState));
                }
                await Task.Delay(1).ConfigureAwait(false);
            }

        }


        public int FormAxesGroup(int[] axisNums)
        {
            if (_mGpHand == null)
            {
                _mGpHand = new List<IntPtr>();
            }
            var hand = new IntPtr();
            for (int i = 0; i < axisNums.Length; i++)
            {
                var result = Motion.mAcm_GpAddAxis(ref hand, _mAxishand[axisNums[i]]);
                if (!Success(result))
                {
                    var sb = new StringBuilder();
                    Motion.mAcm_GetErrorMessage(result, sb, 50);
                    throw new MotionException($"{sb} Error Code: [0x{result:X}]");
                }
            }
            _mGpHand.Add(hand);
            return _mGpHand.IndexOf(hand);
        }
        public void MoveAxisContiniouslyAsync(int axisNum, AxDir dir)
        {
            Motion.mAcm_AxMoveVel(_mAxishand[axisNum], (ushort)dir);
        }

        public void MoveAxesByCoorsAsync((int axisNum, double position)[] ax)
        {
            if (ax.Any(ind => ind.axisNum > _mAxishand.Length - 1))
            {
                throw new MotionException($"Для настоящего устройства не определена ось № {ax.Max(num => num.axisNum)}");
            }


            foreach (var item in ax)
            {
                Motion.mAcm_AxMoveAbs(_mAxishand[item.axisNum], item.position);
            }
        }
        public async Task MoveAxesByCoorsPrecAsync((int axisNum, double position, double lineCoefficient)[] ax)
        {
            if (ax.Any(ind => ind.axisNum > _mAxishand.Length - 1))
            {
                throw new MotionException($"Для настоящего устройства не определена ось № {ax.Max(num => num.axisNum)}");
            }

            var tasks = new List<Task>(ax.Length);

            //foreach (var item in ax)
            //{
            //    tasks.Append(MoveAxisPreciselyAsync(item.axisNum, item.lineCoefficient, item.position));
            //}

            tasks = ax.Select(p => MoveAxisPreciselyAsync(p.axisNum, p.lineCoefficient, p.position)).ToList();


            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        protected double CalcActualPosition(int axisNum, double lineCoefficient)
        {
            //var result = new uint();
            var position = new double();
            if (lineCoefficient != 0)
            {
                _result = Motion.mAcm_AxGetActualPosition(_mAxishand[axisNum], ref position);
                if (!Success(_result)) { throw new MotionException($"Get actual position Failed With Error Code: [0x{_result:X}]"); }
                position *= lineCoefficient;
            }
            else
            {
                _result = Motion.mAcm_AxGetCmdPosition(_mAxishand[axisNum], ref position);
                if (Success(_result)) { throw new MotionException($"Get command position Failed With Error Code: [0x{_result:X}]"); }
            }

            return position;
        }
        public void SetAxisVelocity(int axisNum, double vel)
        {
            var velHigh = vel;
            var velLow = vel / 2;

            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxVelHigh, ref velHigh, 8).CheckResult(axisNum);
            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxVelLow, ref velLow, 8).CheckResult(axisNum);
        }
        public void SetGroupVelocity(int groupNum)
        {
            uint buf = 4;
            var axesInGroup = new uint();
            var axisNum = new int();
            _result = Motion.mAcm_GetProperty(_mGpHand[groupNum], (uint)PropertyID.CFG_GpAxesInGroup, ref axesInGroup, ref buf);
            if (!Success(_result))
            {
                throw new MotionException($"Запрос осей в группе № {groupNum}. Ошибка: {(ErrorCode)_result}");
            }
            for (int i = 1; i < 5; i++)
            {
                if ((axesInGroup & i) > 0)
                {
                    axisNum = i - 1;
                    break;
                }
            }
            var velHigh = new double();
            buf = 8;
            _result = Motion.mAcm_GetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxVelHigh, ref velHigh, ref buf);
            if (!Success(_result))
            {
                throw new MotionException($"Запрос скорости для оси № {axisNum}. Ошибка: {(ErrorCode)_result}");
            }
            var velLow = velHigh / 2;
            Motion.mAcm_SetProperty(_mGpHand[groupNum], (uint)PropertyID.PAR_GpVelLow, ref velLow, 8).CheckResult();
            //if (!Success(_result))
            //{
            //    throw new MotionException($"Скорость {velLow} не поддерживается группой № {groupNum}. Ошибка: {(ErrorCode)_result}");
            //}
            Motion.mAcm_SetProperty(_mGpHand[groupNum], (uint)PropertyID.PAR_GpVelHigh, ref velHigh, 8).CheckResult();
            //if (!Success(_result))
            //{
            //    throw new MotionException($"Скорость {velHigh} не поддерживается группой № {groupNum}. Ошибка: {(ErrorCode)_result}");
            //}
        }
        public void SetGroupVelocity(int groupNum, double velocity)
        {
            double velHigh = velocity;
            var velLow = velHigh / 2;
            Motion.mAcm_SetProperty(_mGpHand[groupNum], (uint)PropertyID.PAR_GpVelHigh, ref velHigh, 8).CheckResult();
            Motion.mAcm_SetProperty(_mGpHand[groupNum], (uint)PropertyID.PAR_GpVelLow, ref velLow, 8).CheckResult();
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
            Motion.mAcm_AxStopEmg(_mAxishand[axisNum]);
        }
        public void ResetErrors(int axisNum = 888)
        {
            ushort state = 1;
            if (axisNum == 888)
            {
                foreach (var handle in _mAxishand)
                {
                    Motion.mAcm_AxGetState(handle, ref state);
                    if(state == 3) Motion.mAcm_AxResetError(handle).CheckResult();
                }
            }
            else
            {
                Motion.mAcm_AxGetState(_mAxishand[axisNum], ref state);
                if(state == 3) Motion.mAcm_AxResetError(_mAxishand[axisNum]).CheckResult(axisNum);
            }

        }
        public void SetAxisDout(int axisNum, ushort dOut, bool val)
        {
            var b = val ? (byte)1 : (byte)0;
            Motion.mAcm_AxDoSetBit(_mAxishand[axisNum], dOut, b).CheckResult();
            //if (!Success(_result))
            //{
            //    ThrowMessage($"Switch on DOUT {dOut} of axis № {axisNum} failed with error:{(ErrorCode)_result}", 0);
            //}
        }
        public bool GetAxisDout(int axisNum, ushort dOut)
        {
            var data = new byte();
            Motion.mAcm_AxDoGetBit(_mAxishand[axisNum], dOut, ref data);
            return data != 0;
        }
        public virtual void SetAxisConfig(int axisNum, MotionDeviceConfigs configs)
        {
            //uint res;
            var acc = configs.acc;
            var dec = configs.dec;
            var jerk = configs.jerk;
            var ppu = configs.ppu;
            double axMaxAcc = configs.maxAcc;
            double axMaxDec = configs.maxDec;
            var axisMaxVel = 4000000;
            double axMaxVel = axisMaxVel / ppu;
            var buf = (uint)SwLmtEnable.SLMT_DIS;
            if (_initErrorsDictionaryInBaseClass) _errors = new();

            //double homeVelLow = configs.homeVelLow;
            //double homeVelHigh = configs.homeVelHigh;



            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxHomeResetEnable, ref configs.reset, 4); _errors.Add(PropertyID.CFG_AxHomeResetEnable, _result);
            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxPPU, ref ppu, 4); _errors.Add(PropertyID.CFG_AxPPU, _result);
            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxMaxAcc, ref axMaxAcc, 8); _errors.Add(PropertyID.CFG_AxMaxAcc, _result);
            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxMaxDec, ref axMaxDec, 8); _errors.Add(PropertyID.CFG_AxMaxDec, _result);
            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxMaxVel, ref axMaxVel, 8); _errors.Add(PropertyID.CFG_AxMaxVel, _result);
            // result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxPulseInLogic, ref configs.plsInLogic, 4); errors.Add(PropertyID.CFG_AxPulseInLogic, result);
            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxPulseInMode, ref configs.plsInMde, 4); _errors.Add(PropertyID.CFG_AxPulseInMode, _result);
            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxPulseOutMode, ref configs.plsOutMde, 4); _errors.Add(PropertyID.CFG_AxPulseOutMode, _result);
            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxAcc, ref acc, 8); _errors.Add(PropertyID.PAR_AxAcc, _result);
            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxDec, ref dec, 8); _errors.Add(PropertyID.PAR_AxDec, _result);
            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxJerk, ref jerk, 8); _errors.Add(PropertyID.PAR_AxJerk, _result);
            // result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxHomeVelLow, ref homeVelLow, 8); errors.Add(PropertyID.PAR_AxHomeVelLow, result);
            // result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxHomeVelHigh, ref homeVelHigh, 8); errors.Add(PropertyID.PAR_AxHomeVelHigh, result);
            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxSwPelEnable, ref buf, 4); _errors.Add(PropertyID.CFG_AxSwPelEnable, _result);



            var errorText = new string("");
            foreach (var error in _errors.Where(err => err.Value != 0))
            {
                errorText += $"Axis №{axisNum} In {error.Key} has {(ErrorCode)error.Value}\n";
            }
            if (errorText.Length != 0) throw new MotionException(errorText);
        }
        public void SetGroupConfig(int gpNum, MotionDeviceConfigs configs)
        {
            var acc = configs.acc;
            var dec = configs.dec;
            var jerk = configs.jerk;
            var ppu = configs.ppu;
            double axMaxAcc = 180;
            double axMaxDec = 180;
            double axMaxVel = 50;
            Motion.mAcm_SetProperty(_mGpHand[gpNum], (uint)PropertyID.CFG_GpMaxAcc, ref axMaxAcc, 8).CheckResult();
            Motion.mAcm_SetProperty(_mGpHand[gpNum], (uint)PropertyID.CFG_GpMaxDec, ref axMaxDec, 8).CheckResult();
            Motion.mAcm_SetProperty(_mGpHand[gpNum], (uint)PropertyID.CFG_GpMaxVel, ref axMaxVel, 8).CheckResult();
            Motion.mAcm_SetProperty(_mGpHand[gpNum], (uint)PropertyID.CFG_GpPPU, ref ppu, 4).CheckResult();
            Motion.mAcm_SetProperty(_mGpHand[gpNum], (uint)PropertyID.PAR_GpAcc, ref acc, 8).CheckResult();
            Motion.mAcm_SetProperty(_mGpHand[gpNum], (uint)PropertyID.PAR_GpDec, ref dec, 8).CheckResult();
            Motion.mAcm_SetProperty(_mGpHand[gpNum], (uint)PropertyID.PAR_GpJerk, ref jerk, 8).CheckResult();
        }
        protected double GetAxisVelocity(int axisNum)
        {
            //uint res = 0;
            double vel = 0;
            uint bufLength = 8;
            _result = Motion.mAcm_GetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxVelHigh, ref vel, ref bufLength);
            return vel;
        }
        public virtual async Task MoveAxisPreciselyAsync(int axisNum, double lineCoefficient, double position, int rec = 0)
        {
            double accuracy = 0.001;
            double backlash = 0.03;
            ushort state = default;
            double vel = 0.1;
            bool gotIt;
            int signx = 0;

            var storedVelocity = GetAxisVelocity(axisNum);
            
            await Task.Run(async () =>
            {
                for (int recurcy = 0; recurcy < 20; recurcy++)
                {
                    if (recurcy == 0)
                    {
                        Motion.mAcm_AxMoveAbs(_mAxishand[axisNum], position).CheckResult(_mAxishand[axisNum]);
                        var velocity = 0d;
                        do
                        {
                            Motion.mAcm_AxGetState(_mAxishand[axisNum], ref state);
                            await Task.Delay(1).ConfigureAwait(false);
                            Motion.mAcm_AxGetCmdVelocity(_mAxishand[axisNum], ref velocity);                           
                        } while (state == (ushort)Advantech.Motion.AxisState.STA_AX_PTP_MOT && velocity>0);//TODO WTF! fix it

                        if (lineCoefficient == 0) break;
                        Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxVelLow, ref vel, 8).CheckResult(axisNum);
                        Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxVelHigh, ref vel, 8).CheckResult(axisNum);
                    }
                    var diff = position - CalcActualPosition(axisNum, lineCoefficient);
                    if (Math.Abs(diff) <= accuracy) gotIt = true;
                    else gotIt = false;
                    

                    signx = Math.Sign(diff);
                    
                    if (!gotIt)
                    {
                        Motion.mAcm_AxMoveRel(_mAxishand[axisNum], diff + signx * backlash);
                        do
                        {
                            diff = position - CalcActualPosition(axisNum, lineCoefficient);
                            if (Math.Abs(diff) <= accuracy)
                            {
                                Motion.mAcm_AxStopEmg(_mAxishand[axisNum]);
                                gotIt = true;
                            }
                            Motion.mAcm_AxGetState(_mAxishand[0], ref state);
                        } while (!gotIt && state != (ushort)Advantech.Motion.AxisState.STA_AX_READY);
                        if (state == (ushort)Advantech.Motion.AxisState.STA_AX_ERROR_STOP) ResetErrors(axisNum);
                    }

                    if (gotIt) break;
                }
            }
            );

            SetAxisVelocity(axisNum, storedVelocity);
        }
        public void ResetAxisCounter(int axisNum)
        {
            Motion.mAcm_AxSetCmdPosition(_mAxishand[axisNum], 0).CheckResult();
            Motion.mAcm_AxSetActualPosition(_mAxishand[axisNum], 0).CheckResult();
        }
        public void HomeMovingAsync((int axisNum, double vel, uint mode)[] axVels)
        {
            var state = new ushort();
            foreach (var axis in axVels)
            {
                Motion.mAcm_AxGetState(_mAxishand[axis.axisNum], ref state);
                if ((state & (ushort)Advantech.Motion.AxisState.STA_AX_HOMING) != 0)
                {
                    return;
                }
            }

            ResetErrors();
            //var result = new uint();
            foreach (var axvel in axVels)
            {
                try
                {
                    SetAxisVelocity(axvel.axisNum, axvel.vel);
                }
                catch (Exception ex)
                {
                    ThrowMessage?.Invoke($"{ex.StackTrace} :\n {ex.Message}", 0);
                    break;
                }

                _result = Motion.mAcm_AxHome(_mAxishand[axvel.axisNum], axvel.mode, (uint)HomeDir.NegDir);

                if (!Success(_result))
                {
                    ThrowMessage?.Invoke($"Ось № {axvel.axisNum} прервало движение домой с ошибкой {(ErrorCode)_result}", 0);
                }
            }
        }


        public async Task HomeMovingAsync((AxDir direction, HomeRst homeRst, HmMode homeMode, double velocity, int axisNum)[] axs)
        {
            var state = new ushort();
            foreach (var axis in axs)
            {
                Motion.mAcm_AxGetState(_mAxishand[axis.axisNum], ref state);
               
                if (state != (ushort)Advantech.Motion.AxisState.STA_AX_READY)
                {
                    throw new MotionException($"Axis {axis.axisNum} isn't ready for homing");
                }
            }

            ResetErrors();

            foreach (var axvel in axs)
            {
                _result = Motion.mAcm_AxHome(_mAxishand[axvel.axisNum], (uint)axvel.homeMode, (uint)axvel.direction);

                if (!Success(_result))
                {
                    ThrowMessage?.Invoke($"Ось № {axvel.axisNum} прервало движение домой с ошибкой {(ErrorCode)_result}", 0);
                }
            }

            var tasks = axs.Select(ax=> Task.Run(async ()=>
            {
                ushort state = 0;
                do
                {
                    Motion.mAcm_AxGetState(_mAxishand[ax.axisNum], ref state);
                    await Task.Delay(1).ConfigureAwait(false);
                } while (state == (ushort)Advantech.Motion.AxisState.STA_AX_HOMING);
            
            })).ToArray();
            await Task.WhenAll(tasks);
        }

        public async Task MoveGroupAsync(int groupNum, double[] position)
        {
            uint elements = (uint)position.Length;
            var state = new ushort();
            //uint res = 0;
            double vel = 20;
            uint bufLength = 8;


            Motion.mAcm_GpResetError(_mGpHand[groupNum]);
            Motion.mAcm_GpMoveLinearAbs(_mGpHand[groupNum], position, ref elements);
            await Task.Run(() =>
            {
                do
                {
                    Task.Delay(10).Wait();
                    Motion.mAcm_GpGetState(_mGpHand[groupNum], ref state);
                } while ((state & (ushort)GroupState.STA_Gp_Motion) > 0);
            }).ConfigureAwait(false);
        }
        public async Task MoveGroupPreciselyAsync(int groupNum, double[] position, (int axisNum, double lineCoefficient)[] gpAxes)
        {
            var buf = (uint)SwLmtEnable.SLMT_DIS;
            for (int i = 0; i < gpAxes.Length; i++)
            {
                Motion.mAcm_SetProperty(_mAxishand[gpAxes[i].axisNum], (uint)PropertyID.CFG_AxSwPelEnable, ref buf, 4).CheckResult();
                Motion.mAcm_SetProperty(_mAxishand[gpAxes[i].axisNum], (uint)PropertyID.CFG_AxSwMelEnable, ref buf, 4).CheckResult();
            }


            await MoveGroupAsync(groupNum, position);
            for (int i = 0; i < gpAxes.Length; i++)
            {
                await MoveAxisPreciselyAsync(gpAxes[i].axisNum, gpAxes[i].lineCoefficient, position[i]);
            }
        }
        public void MoveAxisAsync(int axisNum, double position)
        {
            Motion.mAcm_AxMoveAbs(_mAxishand[axisNum], position);
        }
        private static IntPtr OpenDevice(in DEV_LIST device)
        {
            var deviceHandle = IntPtr.Zero;
            var result = Motion.mAcm_DevOpen(device.DeviceNum, ref deviceHandle);

            if (!Success(result))
            {
                throw new MotionException($"Open Device Failed With Error Code: [0x{result:X}]");
            }

            return deviceHandle;
        }
        private static IEnumerable<DEV_LIST> GetAvailableDevs()
        {
            var availableDevs = new DEV_LIST[Motion.MAX_DEVICES];
            uint deviceCount = default;
            var result = Motion.mAcm_GetAvailableDevs(availableDevs, Motion.MAX_DEVICES, ref deviceCount);

            if (!Success(result))
            {
                throw new MotionException($"Get Device Numbers Failed With Error Code: [{result:X}]");
            }

            return availableDevs.Take((int)deviceCount);
        }

        private int GetAxisCount()
        {
            uint axesPerDev = default;
            _result = Motion.mAcm_GetU32Property(DeviceHandle, (uint)PropertyID.FT_DevAxesCount, ref axesPerDev);

            if (!Success(_result))
            {
                throw new MotionException($"Get Axis Number Failed With Error Code: [0x{_result:X}]");
            }

            return (int)axesPerDev;
        }

        private static bool Success(uint result)
        {
            return result == (uint)ErrorCode.SUCCESS;
        }

        private static bool Success(int result)
        {
            return result == (int)ErrorCode.SUCCESS;
        }

        private void ReleaseUnmanagedResources()
        {
            //var copy = DeviceHandle;

            //if (copy != IntPtr.Zero)
            //{
            //    Motion.mAcm_DevClose(ref copy);
            //}
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }
        //public int MoveInPos(Vector3 position, int recurcy)
        //{
        //    Task task;
        //    uint ElCount = 3;
        //    double posX = new double();
        //    double posY = new double();
        //    double accuracy = 0.001;
        //    double backlash = 0;
        //    ushort state = new ushort();
        //    double vel = 0.1;
        //    bool gotItX;
        //    bool gotItY;
        //    int signx = 0;
        //    int signy = 0;
        //    if (recurcy == 0)
        //    {
        //        position.X = Math.Round(position.X, 3);
        //        position.Y = Math.Round(position.Y, 3);


        //        Motion.mAcm_GpMoveLinearAbs(m_GpHand, new double[3] { position.X, position.Y, position.Z }, ref ElCount);
        //        task = Task.Run(() => { while (state != (uint)GroupState.STA_Gp_Ready) Motion.mAcm_GpGetState(m_GpHand, ref state); });
        //        task.Wait();
        //        task.Dispose();
        //        // while (!globalProperties.GpMotionDone) { };

        //        Motion.mAcm_SetProperty(m_Axishand[0], (uint)PropertyID.PAR_AxVelLow, ref vel, 8);
        //        Motion.mAcm_SetProperty(m_Axishand[1], (uint)PropertyID.PAR_AxVelLow, ref vel, 8);
        //        Motion.mAcm_SetProperty(m_Axishand[0], (uint)PropertyID.PAR_AxVelHigh, ref vel, 8);
        //        Motion.mAcm_SetProperty(m_Axishand[1], (uint)PropertyID.PAR_AxVelHigh, ref vel, 8);
        //    }



        //    Motion.mAcm_AxGetActualPosition(m_Axishand[0], ref posX);
        //    Motion.mAcm_AxGetActualPosition(m_Axishand[1], ref posY);


        //    if (Math.Abs(Math.Round(posX * xLine, 3) - position.X) <= accuracy) gotItX = true;
        //    else gotItX = false;
        //    if (Math.Abs(Math.Round(posY * yLine, 3) - position.Y) <= accuracy) gotItY = true;
        //    else gotItY = false;

        //    signx = Math.Sign(position.X - Math.Round(XActual, 3));
        //    signy = Math.Sign(position.Y - Math.Round(YActual, 3));
        //    //Motion.mAcm_CheckMotionEvent(m_DeviceHandle, null, null, 2, 0, 10);


        //    if (!gotItX)
        //    {
        //        Motion.mAcm_AxMoveRel(m_Axishand[0], signx * (Math.Abs(position.X - Math.Round(posX * xLine, 3)) + signx * backlash));
        //        task = Task.Run(() =>
        //        {
        //            while (!gotItX)
        //            {
        //                Motion.mAcm_AxGetActualPosition(m_Axishand[0], ref posX);
        //                if (Math.Abs(Math.Round(posX * xLine, 3) - position.X) <= accuracy)
        //                {
        //                    Motion.mAcm_AxStopEmg(m_Axishand[0]);
        //                    gotItX = true;
        //                }
        //                Motion.mAcm_AxGetState(m_Axishand[0], ref state);
        //                if (state == (uint)AxisState.STA_AX_READY) break;
        //            }
        //        });
        //        task.Wait();
        //        task.Dispose();
        //    }
        //    if (!gotItY)
        //    {
        //        Motion.mAcm_AxMoveRel(m_Axishand[1], signy * (Math.Abs(position.Y - Math.Round(posY * yLine, 3)) + signy * backlash));
        //        task = Task.Run(() =>
        //        {
        //            while (!gotItY)
        //            {
        //                Motion.mAcm_AxGetActualPosition(m_Axishand[1], ref posY);
        //                if (Math.Abs(Math.Round(posY * yLine, 3) - position.Y) <= accuracy)
        //                {
        //                    Motion.mAcm_AxStopEmg(m_Axishand[1]);
        //                    gotItY = true;
        //                }
        //                Motion.mAcm_AxGetState(m_Axishand[1], ref state);
        //                if (state == (uint)AxisState.STA_AX_READY) break;
        //            }
        //        });
        //        task.Wait();
        //        task.Dispose();
        //    }


        //    if (!(gotItX & gotItY || recurcy > 15)) recurcy = MoveInPos(position, ++recurcy);
        //    else recurcy = recurcy > 20 ? 1000 : recurcy;
        //    return recurcy;
        //}

        //public async Task MoveInPos(Vector3 position)
        //{
        //    uint ElCount = 2;
        //    double posX = new double();
        //    double posY = new double();
        //    double accuracy = 0.001;
        //    double backlash = 0;
        //    ushort state = new ushort();
        //    ushort stateZ = new ushort();
        //    double vel = 0.1;
        //    bool gotItX;
        //    bool gotItY;
        //    int signx = 0;
        //    int signy = 0;
        //    await Task.Run(() =>
        //    {
        //        for (int recurcy = 0; recurcy < 20; recurcy++)
        //        {
        //            if (recurcy == 0)
        //            {
        //                position.X = Math.Round(position.X, 3);
        //                position.Y = Math.Round(position.Y, 3);
        //                Motion.mAcm_AxMoveAbs(m_Axishand[2], position.Z);
        //                Motion.mAcm_GpMoveLinearAbs(GpXYHand, new double[2] { position.X, position.Y }, ref ElCount);
        //                while (state != (uint)GroupState.STA_Gp_Ready)
        //                {
        //                    Motion.mAcm_AxGetState(m_Axishand[2], ref stateZ);
        //                    Motion.mAcm_GpGetState(m_GpHand, ref state);
        //                }

        //                // while (!globalProperties.GpMotionDone) { };

        //                Motion.mAcm_SetProperty(m_Axishand[0], (uint)PropertyID.PAR_AxVelLow, ref vel, 8);
        //                Motion.mAcm_SetProperty(m_Axishand[1], (uint)PropertyID.PAR_AxVelLow, ref vel, 8);
        //                Motion.mAcm_SetProperty(m_Axishand[0], (uint)PropertyID.PAR_AxVelHigh, ref vel, 8);
        //                Motion.mAcm_SetProperty(m_Axishand[1], (uint)PropertyID.PAR_AxVelHigh, ref vel, 8);
        //            }
        //            Thread.Sleep(300);
        //            Motion.mAcm_AxGetActualPosition(m_Axishand[0], ref posX);
        //            Motion.mAcm_AxGetActualPosition(m_Axishand[1], ref posY);

        //            if (Math.Abs(Math.Round(posX * xLine, 3) - position.X) <= accuracy) gotItX = true;
        //            else gotItX = false;
        //            if (Math.Abs(Math.Round(posY * yLine, 3) - position.Y) <= accuracy) gotItY = true;
        //            else gotItY = false;

        //            signx = Math.Sign(position.X - Math.Round(XActual, 3));
        //            signy = Math.Sign(position.Y - Math.Round(YActual, 3));
        //            //Motion.mAcm_CheckMotionEvent(m_DeviceHandle, null, null, 2, 0, 10);


        //            if (!gotItX)
        //            {
        //                Motion.mAcm_AxMoveRel(m_Axishand[0], signx * (Math.Abs(position.X - Math.Round(posX * xLine, 3)) + signx * backlash));

        //                while (!gotItX)
        //                {
        //                    Motion.mAcm_AxGetActualPosition(m_Axishand[0], ref posX);
        //                    if (Math.Abs(Math.Round(posX * xLine, 3) - position.X) <= accuracy)
        //                    {
        //                        Motion.mAcm_AxStopEmg(m_Axishand[0]);
        //                        gotItX = true;
        //                    }
        //                    Motion.mAcm_AxGetState(m_Axishand[0], ref state);
        //                    if (state == (uint)AxisState.STA_AX_READY) break;
        //                }

        //            }
        //            if (!gotItY)
        //            {
        //                Motion.mAcm_AxMoveRel(m_Axishand[1], signy * (Math.Abs(position.Y - Math.Round(posY * yLine, 3)) + signy * backlash));

        //                while (!gotItY)
        //                {
        //                    Motion.mAcm_AxGetActualPosition(m_Axishand[1], ref posY);
        //                    if (Math.Abs(Math.Round(posY * yLine, 3) - position.Y) <= accuracy)
        //                    {
        //                        Motion.mAcm_AxStopEmg(m_Axishand[1]);
        //                        gotItY = true;
        //                    }
        //                    Motion.mAcm_AxGetState(m_Axishand[1], ref state);
        //                    if (state == (uint)AxisState.STA_AX_READY) break;
        //                }

        //            }

        //            if (gotItX & gotItY) break;
        //        }
        //    }
        //    );
        //}
    }
}