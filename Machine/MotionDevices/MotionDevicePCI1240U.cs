using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Advantech.Motion;
using MachineClassLibrary.Classes;
using static System.Windows.Forms.AxHost;
using AxState = Advantech.Motion.AxisState;

namespace MachineClassLibrary.Machine.MotionDevices;

public class MotionDevicePCI1240U : IMotionDevicePCI1240U
{
    private const int RESET_ALL_AXES = -1;
    private Dictionary<int, double> _storedSpeeds = new();
    private IntPtr _deviceHandle;
    protected IntPtr[] _mAxisHand;
    private List<IntPtr> _mGpHand;

    protected uint _result;
    protected Dictionary<PropertyID, uint> _errors = new();
    protected bool _initErrorsDictionaryInBaseClass = true;

    protected double _storeSpeed;
    private readonly Dictionary<int, int> _bridges;
    protected double _tolerance;

    private (int ppu, double ratio, double discrete)[] _axPRD;

    private bool _disposedValue;

    private CancellationTokenSource _monitoringCts;
    private Task _monitoringTask;
    public MotionDevicePCI1240U()
    {
        _bridges = new Dictionary<int, int>();
        _tolerance = 0.005;
        var device = GetAvailableDevs().First();
        _deviceHandle = OpenDevice(device);
    }

    public int AxisCount { get; private set; }
    public event EventHandler<AxNumEventArgs> TransmitAxState;

    public Task<bool> DevicesConnection()
    {
        try
        {
            AxisCount = GetAxisCount();
            var buf = (uint)EmgLogic.EMG_ACT_LOW;
            var result = Motion.mAcm_SetProperty(_deviceHandle, (uint)PropertyID.CFG_DevEmgLogic, ref buf, 4);//.CheckResult(_deviceHandle);
        }
        //catch (MotionException e)
        //{
        //    MessageBox.Show(e.Message);
        //    return false;
        //}
        finally { }

        var axisEnableEvent = new uint[AxisCount];
        var gpEnableEvent = new uint[1];

        _mAxisHand = new IntPtr[AxisCount];
        _axPRD = Enumerable.Range(1, AxisCount).Select(n => (1000, 1d, 0d)).ToArray(); //new (int ppu, double ratio, double discrete)[AxisCount];
        for (var i = 0; i < axisEnableEvent.Length; i++)
        {
            Motion.mAcm_AxOpen(_deviceHandle, (ushort)i, ref _mAxisHand[i]).CheckResult(i);

            double cmdPosition = 0;

            ushort state = 0;

            Motion.mAcm_AxGetState(_mAxisHand[i], ref state);

            if ((AxState)state != AxState.STA_AX_READY) Motion.mAcm_AxResetError(_mAxisHand[i]).CheckResult(i);

            //{
            //    switch ((AxState)state)
            //    {
            //        case AxState.STA_AX_DISABLE:
            //            break;

            //        case AxState.STA_AX_READY:
            //            break;

            //        case AxState.STA_AX_STOPPING:
            //            break;

            //        case AxState.STA_AX_ERROR_STOP:
            //            break;

            //        case AxState.STA_AX_HOMING:
            //            break;

            //        case AxState.STA_AX_PTP_MOT:
            //            break;

            //        case AxState.STA_AX_CONTI_MOT:
            //            break;

            //        case AxState.STA_AX_SYNC_MOT:
            //            break;

            //        case AxState.STA_AX_EXT_JOG:
            //            break;

            //        case AxState.STA_AX_EXT_MPG:
            //            break;

            //        case AxState.STA_AX_PAUSE:
            //            break;

            //        case AxState.STA_AX_BUSY:
            //            break;

            //        case AxState.STA_AX_WAIT_DI:
            //            break;

            //        case AxState.STA_AX_WAIT_PTP:
            //            break;

            //        case AxState.STA_AX_WAIT_VEL:
            //            break;

            //        default:
            //            break;
            //    }
            //    Motion.mAcm_AxResetError(_mAxisHand[i]).CheckResult(i);
            //}

            Motion.mAcm_AxSetCmdPosition(_mAxisHand[i], cmdPosition).CheckResult(i);
            Motion.mAcm_AxSetActualPosition(_mAxisHand[i], cmdPosition).CheckResult(i);

            axisEnableEvent[i] |= (uint)EventType.EVT_AX_MOTION_DONE;
            axisEnableEvent[i] |= (uint)EventType.EVT_AX_VH_START;
            axisEnableEvent[i] |= (uint)EventType.EVT_AX_VH_END;
        }

        Motion.mAcm_EnableMotionEvent(_deviceHandle, axisEnableEvent, gpEnableEvent, (uint)AxisCount, 1).CheckResult();

        return Task.FromResult(true);
    }

    public async Task StartMonitoringAsync()
    {
        try
        {
            _monitoringCts = new CancellationTokenSource();
            _monitoringTask = DeviceStateMonitorAsync(_monitoringCts.Token);
        }
        catch (Exception ex)
        {
            throw new MotionException($"{nameof(DeviceStateMonitorAsync)} failed with exception {ex.Message}", ex);
        }
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

    private async Task DeviceStateMonitorAsync(CancellationToken token)
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
        var ez = false;
        var org = false;
        var state = default(ushort);
        var axStates = new Dictionary<IntPtr, AxState>();

        var startTime = DateTime.Now;

        var getState = (int state, int ch) =>
        {
            var result = state & (1 << ch);
            return result == 0 ? 0 : 1;
        };

        var axesStates = new Dictionary<int, (int clearIns, int bridgedIns, int outs)>();

        try
        {
            while (!token.IsCancellationRequested && !_disposedValue)
            {
                eventResult = Motion.mAcm_CheckMotionEvent(_deviceHandle, axEvtStatusArray, gpEvtStatusArray, (uint)AxisCount, 0, 10);
                for (int num = 0; num < _mAxisHand.Length; num++)
                {
                    IntPtr ax = _mAxisHand[num];
                    Motion.mAcm_AxGetMotionIO(ax, ref ioStatus).CheckResult();

                    nLmt = (ioStatus & (uint)Ax_Motion_IO.AX_MOTION_IO_LMTN) > 0;
                    pLmt = (ioStatus & (uint)Ax_Motion_IO.AX_MOTION_IO_LMTP) > 0;
                    ez = (ioStatus & (uint)Ax_Motion_IO.AX_MOTION_IO_EZ) > 0;
                    org = (ioStatus & (uint)Ax_Motion_IO.AX_MOTION_IO_ORG) > 0;

                    var clearState = 0;
                    var sensorsState = 0;
                    var outState = 0;

                    for (var channel = 0; channel < 4; channel++)
                    {
                        Motion.mAcm_AxDiGetBit(ax, (ushort)channel, ref bitData).CheckResult();
                        sensorsState = bitData != 0 ? sensorsState.SetBit(channel) : sensorsState.ResetBit(channel);
                    }
#if PCI1245
                Motion.mAcm_AxDoGetByte(ax, 0, ref bitData).CheckResult();
#endif
                    var bridge = 0;
                    if (_bridges != null && _bridges.Keys.Contains(num))
                    {
                        bridge = _bridges[num];
                    }



                    clearState = sensorsState;
                    sensorsState |= bridge;


#if PCI1245
                Motion.mAcm_AxDoGetByte(ax, 0, ref bitData).CheckResult(ax);//TODO fix it
#else
                    for (var channel = 4; channel < 8; channel++)
                    {
                        Motion.mAcm_AxDoGetBit(ax, (ushort)channel, ref bitData).CheckResult();
                        outState = bitData != 0 ? outState.SetBit(channel) : outState.ResetBit(channel);
                    }
#endif
                    axesStates[num] = (clearState, sensorsState, outState);

                    /*
                    Motion.mAcm_AxGetCmdPosition(ax, ref cmdPosition).CheckResult(ax);
                    Motion.mAcm_AxGetActualPosition(ax, ref actPosition).CheckResult(ax);
                    */

                    if (Success(eventResult))
                    {
                        motionDone = (axEvtStatusArray[num] & (uint)EventType.EVT_AX_MOTION_DONE) > 0;
                        //axState.homeDone = (axEvtStatusArray[num] & (uint)EventType.EVT_AX_HOME_DONE) > 0;
                        vhStart = (axEvtStatusArray[num] & (uint)EventType.EVT_AX_VH_START) > 0;
                        vhEnd = (axEvtStatusArray[num] & (uint)EventType.EVT_AX_VH_END) > 0;
                    }

                    var axState = new AxisState
                    (
                        cmdPosition = GetAxCmd(num),
                        actPosition = GetAxActual(num),
                        sensorsState,
                        outState,
                        pLmt,
                        nLmt,
                        motionDone,
                        homeDone,
                        vhStart,
                        vhEnd,
                        ez,
                        org
                    );

                    Motion.mAcm_AxGetState(ax, ref state).CheckResult(ax);

                    //if (!(axStates.TryGetValue(ax,out var st) && st==(AxState)state))
                    //{
                    //    axStates[ax] = (AxState)state;
                    OnAxStateChanged?.Invoke(ax, (AxState)state);
                    //}

                    try
                    {
                        TransmitAxState?.Invoke(this, new AxNumEventArgs(num, axState));
                    }
                    catch (Exception ex)
                    {
                        await Console.Error.WriteLineAsync(ex.Message).ConfigureAwait(false);
                    }
                }

                if (DateTime.Now - startTime >= TimeSpan.FromMilliseconds(50))
                {
                    var line = 0;
                    var r = 0;
                    Console.CursorVisible = false;

                    void writeIN(int state, int inNum)
                    {
                        Console.Write($"IN{inNum}:");
                        r = getState(state, inNum);
                        Console.ForegroundColor = r == 0 ? ConsoleColor.Green : ConsoleColor.Red;
                        Console.Write($"{r}, ");
                        Console.ForegroundColor = ConsoleColor.White;
                    };

                    foreach (var item in axesStates)
                    {
                        Console.SetCursorPosition(0, line); line++;
                        Console.WriteLine($"----------------------------------Axis{item.Key} --------------------------------------");
                        Console.WriteLine();
                        Console.SetCursorPosition(0, line); line++;
                        var st = item.Value.clearIns;
                        var bst = item.Value.bridgedIns;
                        var vs = item.Value.outs;
                        Console.SetCursorPosition(0, line); line++;
                        Console.Write("ClearSensors: ");
                        writeIN(st, 0);
                        writeIN(st, 1);
                        writeIN(st, 2);
                        writeIN(st, 3);
                        Console.SetCursorPosition(0, line); line++;
                        Console.WriteLine("BridgeSensors: IN0:{0}, IN1:{1}, IN2:{2}, IN3:{3}", getState(bst, 0), getState(bst, 1), getState(bst, 2), getState(bst, 3));
                        Console.SetCursorPosition(0, line); line++;
                        Console.WriteLine("Valves: OUT4:{0}, OUT5:{1}, OUT6:{2}, OUT7:{3}", getState(vs, 4), getState(vs, 5), getState(vs, 6), getState(vs, 7));
                    }
                    startTime = DateTime.Now;
                }
                await Task.Delay(1).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.ToString());
            Console.ResetColor();
        }
    }


    private Action<IntPtr,AxState> OnAxStateChanged = default;
    private bool _isInEMG_Regime;

    public int FormAxesGroup(int[] axisNums)
    {
        if (_mGpHand == null)
        {
            _mGpHand = new List<IntPtr>();
        }
        var hand = new IntPtr();
        for (var i = 0; i < axisNums.Length; i++)
        {
            var result = Motion.mAcm_GpAddAxis(ref hand, _mAxisHand[axisNums[i]]);
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

    public void MoveAxisContinuouslyAsync(int axisNum, AxDir dir) => Motion.mAcm_AxMoveVel(_mAxisHand[axisNum], (ushort)dir);

    public async Task MoveAxesByCoorsAsync((int axisNum, double position)[] ax)
    {
        if (ax.Any(ind => ind.axisNum >= _mAxisHand.Length))
        {
            throw new MotionException($"Для настоящего устройства не определена ось № {ax.Max(num => num.axisNum)}");
        }

        var tasks = ax.Select(p => MoveAxisPreciselyAsync(p.axisNum, 0, p.position)).ToList();
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }


    public async Task MoveAxesByCoorsPrecAsync((int axisNum, double position, double lineCoefficient)[] ax)
    {
        if (ax.Any(ind => ind.axisNum >= _mAxisHand.Length))
        {
            throw new MotionException($"Для настоящего устройства не определена ось № {ax.Max(num => num.axisNum)}");
        }

        var tasks = new List<Task<double>>(ax.Length);

        tasks = ax.Select(p => MoveAxisPreciselyAsync(p.axisNum, p.lineCoefficient, p.position)).ToList();

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    protected double CalcActualPosition(int axisNum, double lineCoefficient)
    {
        //var result = new uint();
        var position = new double();
        //var cmd = 0d;
        if (lineCoefficient != 0)
        {
            _result = Motion.mAcm_AxGetActualPosition(_mAxisHand[axisNum], ref position);
            if (!Success(_result)) { throw new MotionException($"Get actual position Failed With Error Code: [0x{_result:X}]"); }
        }
        else
        {
            _result = Motion.mAcm_AxGetCmdPosition(_mAxisHand[axisNum], ref position);
            if (!Success(_result)) { throw new MotionException($"Get command position Failed With Error Code: [0x{_result:X}]"); }
        }

        return position /** lineCoefficient*/;
    }

    protected double CalcActualPosition(int axisNum) => _axPRD[axisNum].discrete != 0 ? GetAxActual(axisNum) : GetAxCmd(axisNum);

    protected double GetRawCmd(int axisNum, double position) => position / _axPRD[axisNum].ratio;

    public void SetAxisVelocity(int axisNum, double vel)
    {
        //var velHigh = vel;
        var velHigh = vel / _axPRD[axisNum].ratio;
        var velLow = velHigh/* / 2*/;

        Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.PAR_AxVelHigh, ref velHigh, 8).CheckResult(axisNum);
        Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.PAR_AxVelLow, ref velLow, 8).CheckResult(axisNum);
    }


    public void SetAxisSwLmt(int axisNum, double position)
    {
        if (position! > 0) return;
        var cmd = GetRawCmd(axisNum, position);
        var swEna = (int)SwLmtEnable.SLMT_EN;
        var swReact = (int)SwLmtReact.SLMT_DEC_TO_STOP;
        var res = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.CFG_AxSwPelEnable, ref swEna, 4);
        res = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.CFG_AxSwPelReact, ref swReact, 4);
        res = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.CFG_AxSwPelValue, ref cmd, 8);//possibly not supported in pci1240
    }
    public void ReSetAxisSwLmt(int axisNum)
    {
        var swEna = (int)SwLmtEnable.SLMT_DIS;
        var res = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.CFG_AxSwPelEnable, ref swEna, 4);
    }



    public void StopAxesEMG()
    {
        _isInEMG_Regime = true;
        foreach (var axis in _mAxisHand)
        {
            Motion.mAcm_AxStopEmg(axis);
        }
    }
    public void ResetEMG_Regime()
    {
        _isInEMG_Regime = false;
        ResetErrors(RESET_ALL_AXES);
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
        _result = Motion.mAcm_GetProperty(_mAxisHand[axisNum], (uint)PropertyID.PAR_AxVelHigh, ref velHigh, ref buf);
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
        var velHigh = velocity;
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

    public void StopAxis(int axisNum) => Motion.mAcm_AxStopDec(_mAxisHand[axisNum]);
    public void ResetErrors(int axisNum = RESET_ALL_AXES)
    {
        if (_isInEMG_Regime) return;
        ushort state = 1;
        if (axisNum == RESET_ALL_AXES)
        {
            foreach (var handle in _mAxisHand)
            {
                Motion.mAcm_AxGetState(handle, ref state);
                if (state == 3) Motion.mAcm_AxResetError(handle).CheckResult();
            }
        }
        else
        {
            Motion.mAcm_AxGetState(_mAxisHand[axisNum], ref state);
            if (state == 3) Motion.mAcm_AxResetError(_mAxisHand[axisNum]).CheckResult(axisNum);
        }
    }

    public void SetAxisDout(int axisNum, ushort dOut, bool val)
    {
        var b = val ? (byte)1 : (byte)0;
        Motion.mAcm_AxDoSetBit(_mAxisHand[axisNum], dOut, b).CheckResult();
    }

    public bool GetAxisDout(int axisNum, ushort dOut)
    {
        var data = new byte();
        Motion.mAcm_AxDoGetBit(_mAxisHand[axisNum], dOut, ref data);
        return data != 0;
    }
    /*
 set order:
CFG_AxPPU
CFG_AxMaxVel
CFG_AxMaxAcc
CFG_AxMaxDec
PAR_AxAcc
PAR_AxDec
PAR_AxJerk
PAR_AxVelLow                                 | Check
PAR_AxVelLow<= PAR_AxVelHigh <= CFG_AxMaxVel | if Jerk = 1 (S-Curve)
 */
    public virtual void SetAxisConfig(int axisNum, MotionDeviceConfigs configs)
    {
        //uint res;
        var acc = configs.acc;
        var dec = configs.dec;
        var jerk = configs.jerk;
        var ppu = configs.ppu;
        double axMaxAcc = configs.maxAcc;
        double axMaxDec = configs.maxDec;
        //var axisMaxVel = 4000000;
        double axMaxVel = configs.maxVel / configs.ratio; /*axisMaxVel / ppu;*/
        var buf = (uint)SwLmtEnable.SLMT_DIS;


        _axPRD[axisNum] = (ppu, configs.ratio, configs.lineDiscrete);


        

        if (_initErrorsDictionaryInBaseClass) _errors = new();

        //double homeVelLow = configs.homeVelLow;
        //double homeVelHigh = configs.homeVelHigh;
        var denominator = configs.denominator;

        var dir = (PulseOutMode)configs.plsOutMde;

        _result = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.CFG_AxPPU, ref ppu, 4); _errors.Add(PropertyID.CFG_AxPPU, _result);
        _result = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.CFG_AxMaxVel, ref axMaxVel, 8); _errors.Add(PropertyID.CFG_AxMaxVel, _result);
        _result = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.CFG_AxMaxAcc, ref axMaxAcc, 8); _errors.Add(PropertyID.CFG_AxMaxAcc, _result);
        _result = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.CFG_AxMaxDec, ref axMaxDec, 8); _errors.Add(PropertyID.CFG_AxMaxDec, _result);
        _result = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.PAR_AxAcc, ref acc, 8); _errors.Add(PropertyID.PAR_AxAcc, _result);
        _result = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.PAR_AxDec, ref dec, 8); _errors.Add(PropertyID.PAR_AxDec, _result);
        // _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxJerk, ref jerk, 8); _errors.Add(PropertyID.PAR_AxJerk, _result);

        _result = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.CFG_AxHomeResetEnable, ref configs.reset, 4); _errors.Add(PropertyID.CFG_AxHomeResetEnable, _result);
        _result = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.CFG_AxPulseInMode, ref configs.plsInMde, 4); _errors.Add(PropertyID.CFG_AxPulseInMode, _result);
        _result = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.CFG_AxPulseOutMode, ref configs.plsOutMde, 4); _errors.Add(PropertyID.CFG_AxPulseOutMode, _result);
        // not supported in pci1240            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxDirLogic, ref configs.axDirLogic, 4); _errors.Add(PropertyID.CFG_AxDirLogic, _result);
        // not supported in pci1240            _result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxPulseInLogic, ref configs.plsInLogic, 4); _errors.Add(PropertyID.CFG_AxPulseInLogic, _result);

       

        // result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxHomeVelLow, ref homeVelLow, 8); errors.Add(PropertyID.PAR_AxHomeVelLow, result);
        // result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.PAR_AxHomeVelHigh, ref homeVelHigh, 8); errors.Add(PropertyID.PAR_AxHomeVelHigh, result);
        //possibly not supported in pci1240    _result = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.CFG_AxSwPelEnable, ref buf, 4); _errors.Add(PropertyID.CFG_AxSwPelEnable, _result);

        //uint l = 1;
        _result = Motion.mAcm_SetProperty(_mAxisHand[axisNum], (uint)PropertyID.CFG_AxElLogic, ref configs.hLmtLogic, 4); _errors.Add(PropertyID.CFG_AxElLogic, _result);
        //_result = Motion.mAcm_SetProperty(_mAxishand[axisNum], (uint)PropertyID.CFG_AxInpLogic, ref l, 4); _errors.Add(PropertyID.CFG_AxInpLogic, _result);

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
        double vel = 0;
        uint bufLength = 8;
        _result = Motion.mAcm_GetProperty(_mAxisHand[axisNum], (uint)PropertyID.PAR_AxVelHigh, ref vel, ref bufLength);
        return vel * _axPRD[axisNum].ratio;
    }

    public virtual async Task<double> MoveAxisPreciselyAsync(int axisNum, double lineCoefficient, double position, CancellationToken? cancellationToken = null)
    {
        var ct = cancellationToken ?? CancellationToken.None;
        if (ct.IsCancellationRequested) return position - CalcActualPosition(axisNum);
        ct.Register(() => StopAxis(axisNum));

        var accuracy = _tolerance;
        ushort state = default;
        var vel = 1;

        Func<double, bool> gotIt = (double delta) =>
        {
            return Math.Abs(delta) <= _tolerance;
        };

        var storedVelocity = GetAxisVelocity(axisNum);
        var diff = _tolerance + 1;
        if (ct.IsCancellationRequested) return position - CalcActualPosition(axisNum);
        var rawPos = GetRawCmd(axisNum, position);
        Motion.mAcm_AxMoveAbs(_mAxisHand[axisNum], rawPos).CheckResult(_mAxisHand[axisNum]);
        var result = await WaitAxisIsReadyAsync(_mAxisHand[axisNum]).ConfigureAwait(false);

        var actPos = CalcActualPosition(axisNum);
        diff = position - actPos;
        if (!(lineCoefficient == 0 || gotIt(diff)))
        {
            SetAxisVelocity(axisNum, vel);
            int recursion = 0;
            while (!gotIt(diff) && recursion < 50 && !ct.IsCancellationRequested)
            {
                recursion++;
                var rawDiff = GetRawCmd(axisNum, diff);
                Motion.mAcm_AxMoveRel(_mAxisHand[axisNum], rawDiff);
                result = await WaitAxisIsReadyAsync(_mAxisHand[axisNum], ct).ConfigureAwait(false);
                await Task.Delay(200, ct).ConfigureAwait(false);
                diff = position - CalcActualPosition(axisNum);
                if ((AxState)state == AxState.STA_AX_ERROR_STOP) break;
            }
        }
        SetAxisVelocity(axisNum, storedVelocity);
        return diff;
    }

    private Task<bool> WaitAxisIsReadyAsync(IntPtr axisHand, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<bool>();
        if (ct != default)
        {
            ct.Register(()=>
            {
                OnAxStateChanged -= getState;
                tcs.TrySetResult(false);
            });
        }
        OnAxStateChanged += getState;
        return tcs.Task;
        void getState(IntPtr ax, AxState state) 
        {
            if (ax == axisHand && state == AxState.STA_AX_READY)
            {
                OnAxStateChanged-=getState;
                tcs.TrySetResult(true);
            }
            else if(ax == axisHand && state == AxState.STA_AX_ERROR_STOP)
            {
                OnAxStateChanged -= getState;
                tcs.TrySetResult(false);
            }
        }
    }






    public void ResetAxisCounter(int axisNum)
    {
        Motion.mAcm_AxSetCmdPosition(_mAxisHand[axisNum], 0).CheckResult();
        Motion.mAcm_AxSetActualPosition(_mAxisHand[axisNum], 0).CheckResult();
    }

    public async Task HomeMovingAsync((int axisNum, double vel, uint mode)[] axVels)
    {
        var state = new ushort();
        foreach (var axis in axVels)
        {
            Motion.mAcm_AxGetState(_mAxisHand[axis.axisNum], ref state);
            if (state == (ushort)Advantech.Motion.AxisState.STA_AX_HOMING)
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
                //ThrowMessage?.Invoke($"{ex.StackTrace} :\n {ex.Message}", 0);
                throw new MotionException($"In the {nameof(HomeMovingAsync)} method {nameof(SetAxisVelocity)} failed for the axis number {axvel.axisNum} ", ex);
            }




            _result = Motion.mAcm_AxHome(_mAxisHand[axvel.axisNum], axvel.mode, (uint)HomeDir.NegDir);

            if (!Success(_result))
            {
                throw new MotionException($"Ось № {axvel.axisNum} прервало движение домой с ошибкой {(ErrorCode)_result}");
                //ThrowMessage?.Invoke($"Ось № {axvel.axisNum} прервало движение домой с ошибкой {(ErrorCode)_result}", 0);
            }

            homings.Add(Task.Run(() =>
            {
                while (state == (ushort)AxState.STA_AX_HOMING)
                {
                    Motion.mAcm_AxGetState(_mAxisHand[axvel.axisNum], ref state);
                }
            }));
        }
        await Task.WhenAll(homings).ConfigureAwait(false);
    }

    public async Task HomeMovingAsync((AxDir direction, HomeRst homeRst, HmMode homeMode, double velocity, int axisNum)[] axs)
    {
        ResetErrors();
        var state = new ushort();
        foreach (var axis in axs)
        {
            Motion.mAcm_AxGetState(_mAxisHand[axis.axisNum], ref state);

            if (state != (ushort)Advantech.Motion.AxisState.STA_AX_READY)
            {
                throw new MotionException($"Axis {axis.axisNum} isn't ready for homing");
            }
        }

        foreach (var axvel in axs)
        {
            SetAxisVelocity(axvel.axisNum, axvel.velocity);

            _result = Motion.mAcm_AxHome(_mAxisHand[axvel.axisNum], (uint)axvel.homeMode, (uint)axvel.direction);

            if (!Success(_result))
            {
                //ThrowMessage?.Invoke($"Ось № {axvel.axisNum} прервало движение домой с ошибкой {(ErrorCode)_result}", 0);
                throw new MotionException($"Ось № {axvel.axisNum} прервало движение домой с ошибкой {(ErrorCode)_result}");
            }
        }

        var tasks = axs.Select(ax => Task.Run(() =>
        {
            ushort state = 0;
            do
            {
                Motion.mAcm_AxGetState(_mAxisHand[ax.axisNum], ref state);
                Thread.Sleep(100);
            } while (state == (ushort)AxState.STA_AX_HOMING);
        })).ToArray();
        await Task.WhenAll(tasks).ConfigureAwait(false);
        foreach (var axis in axs.Select(a => a.axisNum))
        {
            ResetAxisCounter(axis);
        }
    }

    public async Task MoveGroupAsync(int groupNum, double[] position)
    {
        var elements = (uint)position.Length;
        var state = new ushort();

        Motion.mAcm_GpResetError(_mGpHand[groupNum]);
        Motion.mAcm_GpMoveLinearAbs(_mGpHand[groupNum], position, ref elements);
        await Task.Run(() =>
        {
            do
            {
                Thread.Sleep(100);
                Motion.mAcm_GpGetState(_mGpHand[groupNum], ref state);
            } while ((state & (ushort)GroupState.STA_Gp_Motion) > 0);
        }).ConfigureAwait(false);
    }

    public async Task MoveGroupPreciselyAsync(int groupNum, double[] position, (int axisNum, double lineCoefficient)[] gpAxes)
    {
        var buf = (uint)SwLmtEnable.SLMT_DIS;
        for (var i = 0; i < gpAxes.Length; i++)
        {
            Motion.mAcm_SetProperty(_mAxisHand[gpAxes[i].axisNum], (uint)PropertyID.CFG_AxSwPelEnable, ref buf, 4).CheckResult();
            Motion.mAcm_SetProperty(_mAxisHand[gpAxes[i].axisNum], (uint)PropertyID.CFG_AxSwMelEnable, ref buf, 4).CheckResult();
        }

        await MoveGroupAsync(groupNum, position).ConfigureAwait(false);
        for (var i = 0; i < gpAxes.Length; i++)
        {
            await MoveAxisPreciselyAsync(gpAxes[i].axisNum, gpAxes[i].lineCoefficient, position[i]).ConfigureAwait(false);
        }
    }

    public async Task MoveAxisAsync(int axisNum, double position, CancellationToken? cancellationToken = null)
    {
       
        //var ct = cancellationToken ?? CancellationToken.None;
        //ct.Register(() => StopAxis(axisNum));
        //if (ct.IsCancellationRequested) return;
        if (Math.Abs(GetAxCmd(axisNum) - position) < _tolerance) return;
        var rawPos = GetRawCmd(axisNum, position);
        Motion.mAcm_AxMoveAbs(_mAxisHand[axisNum], /*position*/rawPos);
        if (cancellationToken is not null)
        {
            if (cancellationToken.Value.IsCancellationRequested) return;
            cancellationToken.Value.Register(() => StopAxis(axisNum));
            await WaitAxisIsReadyAsync(_mAxisHand[axisNum], cancellationToken.Value).ConfigureAwait(false);
        }
        else
        {
            await WaitAxisIsReadyAsync(_mAxisHand[axisNum]).ConfigureAwait(false);
        }
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
        _result = Motion.mAcm_GetU32Property(_deviceHandle, (uint)PropertyID.FT_DevAxesCount, ref axesPerDev);

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
        var copy = _deviceHandle;

        if (copy != IntPtr.Zero)
        {
            Motion.mAcm_DevClose(ref copy);
        }
    }

    public double GetAxActual(int axNum)
    {
        var position = 0d;
        Motion.mAcm_AxGetActualPosition(_mAxisHand[axNum], ref position).CheckResult(axNum);

        //---
        position *= _axPRD[axNum].ppu * _axPRD[axNum].discrete;
        //---

        return position;
    }

    public void SetPrecision(double tolerance) => _tolerance = tolerance;

    public Task<double> MoveAxisPreciselyAsync_2(int axisNum, double lineCoefficient, double position, int rec = 0)
    {
        throw new NotImplementedException();
    }

    public double GetAxCmd(int axNum)
    {
        var position = 0d;
        Motion.mAcm_AxGetCmdPosition(_mAxisHand[axNum], ref position).CheckResult(axNum);

        //----
        position *= _axPRD[axNum].ratio;
        //----


        return position;
    }

    public void SetAxisCoordinate(int axisNum, double coordinate)
    {
        throw new NotImplementedException();
    }

    public void CloseDevice()
    {
        var result = Motion.mAcm_DevClose(ref _deviceHandle);
    }

    public bool GetAxisReady(int axNum)
    {
        var status = default(uint);
        Motion.mAcm_AxGetMotionStatus(_mAxisHand[axNum], ref status);
        return (Ax_Motion_IO)status == Ax_Motion_IO.AX_MOTION_IO_RDY;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)

                foreach (var axis in _mAxisHand)
                {
                    Motion.mAcm_AxStopEmg(axis);
                }
                _monitoringCts?.Cancel();
                try
                {
                    _monitoringTask?.Wait(1000); // Ждём завершения мониторинга
                }
                catch (Exception ex)
                {
                    // Логирование, если возможно
                }
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            ReleaseUnmanagedResources();
            // TODO: set large fields to null
            _deviceHandle = IntPtr.Zero;
            if(_mAxisHand is not null) for (int i = 0; i < _mAxisHand.Length; i++)
            {
                _mAxisHand[i] = IntPtr.Zero;
            }
            if(_mGpHand is not null) for (int i = 0; i < _mGpHand.Count; i++)
            {
                _mGpHand[i] = IntPtr.Zero;
            }
            _disposedValue = true;
        }
    }

    // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~MotionDevicePCI1240U()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}


public static class PCI1240UCalculator
{
    public static (double min, double max) GetValidRange_CFG_AxMaxVel(int cfg_AxPPU)
    {
        return (8000d/cfg_AxPPU, 
                4000_000d/cfg_AxPPU);//For pci1240 FT_AxMaxVel = 4000000
    }
    public static (double min, double max) GetValidRange_PAR_AxVelLow(double cfg_AxMaxVel)
    {
        return (cfg_AxMaxVel/8000, 
                cfg_AxMaxVel);
    }
    
    public static (double min, double max) GetValidRange_CFG_AxMaxAcc(int cfg_AxPPU, double cfg_AxMaxVel)
    {
        var max = 500_000_000d / cfg_AxPPU;
        var noGreaterThan = 125 * cfg_AxMaxVel;
        return (125d/cfg_AxPPU,
                (noGreaterThan < max) ? noGreaterThan : max);//For pci1240 FT_AxMaxAcc = 500000000
    }
    public static (double min, double max) GetValidRange_PAR_AxAcc(double cfg_AxMaxAcc, double cfg_AxMaxVel)
    {
        return (cfg_AxMaxVel / 64,
                cfg_AxMaxAcc);
    }
    
    public static (double min, double max) GetValidRange_CFG_AxMaxDec(int cfg_AxPPU, double cfg_AxMaxVel)
    {
        var max = 500_000_000d / cfg_AxPPU;
        var noGreaterThan = 125 * cfg_AxMaxVel;
        return (125d / cfg_AxPPU,
                (noGreaterThan < max) ? noGreaterThan : max);//For pci1240 FT_AxMaxAcc = 500000000
    }
    public static (double min, double max) GetValidRange_PAR_AxDec(double cfg_AxMaxAcc, double cfg_AxMaxVel)
    {
        return (cfg_AxMaxVel / 64,
                cfg_AxMaxAcc);
    }

    public static bool IsVelRangeValidWhenSCurve(double par_VelLow, double par_VelHigh, double acc, double cfg_AxMaxVel)
    {
        var diff = par_VelHigh - par_VelLow;
        if (diff < 0) return false;
        return diff > acc * acc / (78125 * cfg_AxMaxVel) && diff < 8.4 * acc * acc / cfg_AxMaxVel;
    }
}
