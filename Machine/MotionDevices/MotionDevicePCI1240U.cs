using Advantech.Motion;
using MachineClassLibrary.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MachineClassLibrary.Machine.MotionDevices
{

    public class MotionDevicePCI1240U : IDisposable, IMessager
    {

        public MotionDevicePCI1240U()
        {
            _bridges = new Dictionary<int, int>();
            var device = GetAvailableDevs().First();
            DeviceHandle = OpenDevice(device);
        }

        public int AxisCount { get; set; }
        protected IntPtr[] _mAxishand;
        protected uint _result;
        protected Dictionary<PropertyID, uint> _errors = new Dictionary<PropertyID, uint>();
        protected bool _initErrorsDictionaryInBaseClass = true;
        private List<IntPtr> _mGpHand;
        protected double _storeSpeed;
        private Dictionary<int, int> _bridges;
        public static IntPtr DeviceHandle { get; private set; }

        public event EventHandler<AxNumEventArgs> TransmitAxState;

        public event Action<string, int> ThrowMessage;

        public bool DevicesConnection()
        {
            try
            {
                AxisCount = GetAxisCount();
            }
            //catch (MotionException e)
            //{
            //    MessageBox.Show(e.Message);
            //    return false;
            //}
            finally { }
            string strTemp;

            var axisEnableEvent = new uint[AxisCount];
            var gpEnableEvent = new uint[1];

            //uint result;
            _mAxishand = new IntPtr[AxisCount];
            for (var i = 0; i < axisEnableEvent.Length; i++)
            {
                _result = Motion.mAcm_AxOpen(DeviceHandle, (ushort)i, ref _mAxishand[i]);
                if (!Success(_result))
                {
                    throw new MotionException($"Open Axis Failed With Error Code: [0x{_result:X}]");
                }

                double cmdPosition = 0;

                _result = Motion.mAcm_AxSetCmdPosition(_mAxishand[i], cmdPosition);

                _result = Motion.mAcm_AxSetActualPosition(_mAxishand[i], cmdPosition);

                axisEnableEvent[i] |= (uint)EventType.EVT_AX_MOTION_DONE;
                axisEnableEvent[i] |= (uint)EventType.EVT_AX_VH_START;
                axisEnableEvent[i] |= (uint)EventType.EVT_AX_HOME_DONE;
                axisEnableEvent[i] |= (uint)EventType.EVT_AX_VH_START;
               
            }

            gpEnableEvent[0] |= (uint)EventType.EVT_AX_MOTION_DONE;

            _result = Motion.mAcm_EnableMotionEvent(DeviceHandle, axisEnableEvent, gpEnableEvent, (uint)AxisCount, 1);
            if (!Success(_result))
            {
                throw new MotionException($"Enable motion events Failed With Error Code: [0x{_result:X}]");
            }

            return true;
        }
        public Task StartMonitoringAsync()
        {
            return DeviceStateMonitorAsync();
        }
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
                eventResult = Motion.mAcm_CheckMotionEvent(DeviceHandle, axEvtStatusArray, gpEvtStatusArray, (uint)AxisCount, 1, 10);
                for (int num = 0; num < _mAxishand.Length; num++)
                {
                    var axState = new AxisState();
                    IntPtr ax = _mAxishand[num];
                    _result = Motion.mAcm_AxGetMotionIO(ax, ref ioStatus);
                    if (Success(_result))
                    {
                        axState.nLmt = (ioStatus & (uint)Ax_Motion_IO.AX_MOTION_IO_LMTN) > 0;
                        axState.pLmt = (ioStatus & (uint)Ax_Motion_IO.AX_MOTION_IO_LMTP) > 0;
                    }

                    for (var channel = 0; channel < 4; channel++)
                    {
                        _result = Motion.mAcm_AxDiGetBit(ax, (ushort)channel, ref bitData);
                        if (Success(_result))
                        {
                            axState.sensors = bitData != 0 ? axState.sensors.SetBit(channel) : axState.sensors.ResetBit(channel);
                        }
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
                        if (axState.motionDone) GetMotionDoneEvent?.Invoke(num);
                        axState.homeDone = (axEvtStatusArray[num] & (uint)EventType.EVT_AX_HOME_DONE) > 0;
                        if (axState.homeDone) GetHomeMotionDoneEvent?.Invoke(num);
                        axState.vhStart = (axEvtStatusArray[num] & (uint)EventType.EVT_AX_VH_START) > 0;
                        if ((gpEvtStatusArray[0] & (uint)EventType.EVT_GP1_MOTION_DONE) > 0) GetGPMotionDoneEvent?.Invoke();
                    }

                    TransmitAxState?.Invoke(this, new AxNumEventArgs(num, axState));
                }
                await Task.Delay(1).ConfigureAwait(false);
            }
        }

        private event Action<int> GetMotionDoneEvent;
        private event Action<int> GetHomeMotionDoneEvent;
        private event Action GetGPMotionDoneEvent;

        public int FormAxesGroup(int[] axisNums)
        {
            if (_mGpHand == null)
            {
                _mGpHand = new List<IntPtr>();
            }
            var hand = new IntPtr();
            for (int i = 0; i < axisNums.Length; i++)
            {
                _result = Motion.mAcm_GpAddAxis(ref hand, _mAxishand[axisNums[i]]);
                if (!Success(_result))
                {
                    throw new MotionException($"Open Axis Failed With Error Code: [0x{_result:X}]");
                }
            }
            _mGpHand.Add(hand);
            return _mGpHand.IndexOf(hand);
        }
        public async Task MoveAxisContiniouslyAsync(int axisNum, AxDir dir)
        {
            await WrapAxisMovementToAsync(axisNum, () => Motion.mAcm_AxMoveVel(_mAxishand[axisNum], (ushort)dir));
        }

        private Task WrapAxisMovementToAsync(int axisNum, Func<uint> motion)
        {
            var moving = true;
            void Watcher(int n)
            {
                if (axisNum == n)
                {
                    GetMotionDoneEvent -= Watcher;
                    moving = false;
                }
            }
            GetMotionDoneEvent += Watcher;

            var result = motion.Invoke();
            if (!Success(result))
            {
                throw new MotionException($"Motion was cancelled with code {(ErrorCode)result}");
            }

            return Task.Run(() =>
            {
                while (moving) ;
            });
        }

        private Task WrapAxisHomingToAsync(int axisNum, Func<uint> motion)
        {
            var moving = true;
            void Watcher(int n)
            {
                if (axisNum == n)
                {
                    GetHomeMotionDoneEvent -= Watcher;
                    moving = false;
                }
            }
            GetHomeMotionDoneEvent += Watcher;

            var result = motion.Invoke();
            if (!Success(result))
            {
                throw new MotionException($"Axis homing was cancelled with code {(ErrorCode)result}");
            }

            return Task.Run(() =>
            {
                while (moving) ;
            });
        }

        private Task WrapGPMotionToAsync(Func<uint> motion)
        {
            var moving = true;
            void Watcher()
            {
                GetGPMotionDoneEvent -= Watcher;
                moving = false;
            }
            GetGPMotionDoneEvent += Watcher;

            var result = motion.Invoke();
            if (!Success(result))
            {
                throw new MotionException($"GP motion was cancelled with code {(ErrorCode)result}");
            }

            return Task.Run(() =>
            {
                while (moving) ;
            });
        }
        public async Task MoveAxesByCoorsAsync((int axisNum, double position)[] ax)
        {
            if (ax.Where(ind => ind.axisNum > _mAxishand.Length - 1).Any())
            {
                throw new MotionException($"Для настоящего устройства не определена ось № {ax.Max(num => num.axisNum)}");
            }
            var tasks = new List<Task>(ax.Length);
            foreach (var item in ax)
            {
                var task = MoveAxisAsync(item.axisNum, item.position);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }
        public async Task MoveAxesByCoorsPrecAsync((int axisNum, double position, double lineCoefficient)[] ax)
        {
            if (ax.Where(ind => ind.axisNum > _mAxishand.Length - 1).Any())
            {
                throw new MotionException($"Для настоящего устройства не определена ось № {ax.Max(num => num.axisNum)}");
            }
            var tasks = new List<Task>(ax.Length);
            foreach (var item in ax)
            {
                var task = MoveAxisPreciselyAsync(item.axisNum, item.lineCoefficient, item.position);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
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

            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxVelHigh, ref velHigh, 8);
            if (!Success(_result))
            {
                throw new MotionException($"Скорость {vel} не поддерживается осью № {axisNum}. Ошибка: {(ErrorCode)_result}");
            }
            Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxVelLow, ref velLow, 8);
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
            _result = Motion.mAcm_SetProperty(_mGpHand[groupNum], (uint)PropertyID.PAR_GpVelLow, ref velLow, 8);
            if (!Success(_result))
            {
                throw new MotionException($"Скорость {velLow} не поддерживается группой № {groupNum}. Ошибка: {(ErrorCode)_result}");
            }
            _result = Motion.mAcm_SetProperty(_mGpHand[groupNum], (uint)PropertyID.PAR_GpVelHigh, ref velHigh, 8);
            if (!Success(_result))
            {
                throw new MotionException($"Скорость {velHigh} не поддерживается группой № {groupNum}. Ошибка: {(ErrorCode)_result}");
            }
        }
        public void SetGroupVelocity(int groupNum, double velocity)
        {
            double velHigh = velocity;
            var velLow = velHigh / 2;
            _result = Motion.mAcm_SetProperty(_mGpHand[groupNum], (uint)PropertyID.PAR_GpVelLow, ref velLow, 8);
            if (!Success(_result))
            {
                throw new MotionException($"Скорость {velLow} не поддерживается группой № {groupNum}. Ошибка: {(ErrorCode)_result}");
            }
            _result = Motion.mAcm_SetProperty(_mGpHand[groupNum], (uint)PropertyID.PAR_GpVelHigh, ref velHigh, 8);
            if (!Success(_result))
            {
                throw new MotionException($"Скорость {velHigh} не поддерживается группой № {groupNum}. Ошибка: {(ErrorCode)_result}");
            }
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
            if (axisNum == 888)
            {
                foreach (var handle in _mAxishand)
                {
                    Motion.mAcm_AxResetError(handle);
                }
            }
            else
            {
                Motion.mAcm_AxResetError(_mAxishand[axisNum]);
            }

        }
        public void SetAxisDout(int axisNum, ushort dOut, bool val)
        {
            var b = val ? (byte)1 : (byte)0;
            _result = Motion.mAcm_AxDoSetBit(_mAxishand[axisNum], dOut, b);
            if (!Success(_result))
            {
                ThrowMessage($"Switch on DOUT {dOut} of axis № {axisNum} failed with error:{(ErrorCode)_result}", 0);
            }
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
            //var res = new uint();
            var acc = configs.acc;
            var dec = configs.dec;
            var jerk = configs.jerk;
            var ppu = configs.ppu;
            double axMaxAcc = 180;
            double axMaxDec = 180;
            double axMaxVel = 50;
            uint buf = 0;
            _result = Motion.mAcm_SetProperty(_mGpHand[gpNum], (uint)PropertyID.CFG_GpMaxAcc, ref axMaxAcc, 8);
            _result = Motion.mAcm_SetProperty(_mGpHand[gpNum], (uint)PropertyID.CFG_GpMaxDec, ref axMaxDec, 8);
            _result = Motion.mAcm_SetProperty(_mGpHand[gpNum], (uint)PropertyID.CFG_GpMaxVel, ref axMaxVel, 8);
            _result = Motion.mAcm_SetProperty(_mGpHand[gpNum], (uint)PropertyID.CFG_GpPPU, ref ppu, 4);
            _result = Motion.mAcm_SetProperty(_mGpHand[gpNum], (uint)PropertyID.PAR_GpAcc, ref acc, 8);
            _result = Motion.mAcm_SetProperty(_mGpHand[gpNum], (uint)PropertyID.PAR_GpDec, ref dec, 8);
            _result = Motion.mAcm_SetProperty(_mGpHand[gpNum], (uint)PropertyID.PAR_GpJerk, ref jerk, 8);
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
            throw new NotImplementedException();
        }
        public void ResetAxisCounter(int axisNum)
        {
            _result = Motion.mAcm_AxSetCmdPosition(_mAxishand[axisNum], 0);
            _result = Motion.mAcm_AxSetActualPosition(_mAxishand[axisNum], 0);
        }
        public async Task HomeMovingAsync((int axisNum, double vel, uint mode)[] axVels)
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

            var tasks = new List<Task>(axVels.Length);

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

                var task = WrapAxisHomingToAsync(axvel.axisNum, () => Motion.mAcm_AxHome(_mAxishand[axvel.axisNum], axvel.mode, (uint)HomeDir.NegDir));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }
        public async Task MoveGroupAsync(int groupNum, double[] position)
        {
            uint elements = (uint)position.Length;
            //var state = new ushort();


            Motion.mAcm_GpResetError(_mGpHand[groupNum]);
            //Motion.mAcm_GpMoveLinearAbs(_mGpHand[groupNum], position, ref elements);

            await WrapGPMotionToAsync(() => Motion.mAcm_GpMoveLinearAbs(_mGpHand[groupNum], position, ref elements));


            //await Task.Run(() =>
            //{
            //    do
            //    {
            //        Task.Delay(10).Wait();
            //        Motion.mAcm_GpGetState(_mGpHand[groupNum], ref state);
            //    } while ((state & (ushort)GroupState.STA_Gp_Motion) > 0);
            //});
        }
        public async Task MoveGroupPreciselyAsync(int groupNum, double[] position, (int axisNum, double lineCoefficient)[] gpAxes)
        {
            uint buf = 0;

            buf = (uint)SwLmtEnable.SLMT_DIS;
            for (int i = 0; i < gpAxes.Length; i++)
            {
                _result = Motion.mAcm_SetProperty(_mAxishand[gpAxes[i].axisNum], (uint)PropertyID.CFG_AxSwPelEnable, ref buf, 4);
                _result = Motion.mAcm_SetProperty(_mAxishand[gpAxes[i].axisNum], (uint)PropertyID.CFG_AxSwMelEnable, ref buf, 4);
            }


            await MoveGroupAsync(groupNum, position);
            for (int i = 0; i < gpAxes.Length; i++)
            {
                await MoveAxisPreciselyAsync(gpAxes[i].axisNum, gpAxes[i].lineCoefficient, position[i]);
            }
        }
        public async Task MoveAxisAsync(int axisNum, double position)
        {
            await WrapAxisMovementToAsync(axisNum, () => Motion.mAcm_AxMoveAbs(_mAxishand[axisNum], position));
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