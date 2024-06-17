using MachineClassLibrary.Classes;
using MachineClassLibrary.Machine.MotionDevices;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MachineClassLibrary.Machine.Machines
{
    public abstract class PCI124XXMachine : IHasMotion
    {
        protected readonly IMotionDevicePCI1240U _motionDevice;

        protected Dictionary<Ax, IAxis> _axes;
        private Dictionary<Groups, (int groupNum, Ax[] axes)> _axesGroups;
        private Dictionary<MFeatures, double> _doubleFeatures;
        protected Dictionary<Ax, Dictionary<Velocity, double>> _velRegimes;
        private readonly Dictionary<Ax, (AxDir direction, HomeRst homeRst, HmMode homeMode, double velocity, double positionAfterHoming)> _homingConfigs = new();
        public PCI124XXMachine(IMotionDevicePCI1240U motionDevice)
        {
            _motionDevice = motionDevice;
            //IsMotionDeviceInit = _motionDevice.DevicesConnection();
            //if (IsMotionDeviceInit)
            //{
            //    MotionDevInitialized();
            //    _motionDevice.TransmitAxState += MotionDevice_TransmitAxState;
            //}

            _motionDevice.DevicesConnection()
                .ContinueWith(result =>
                {
                    IsMotionDeviceInit = result.Result;
                    if (IsMotionDeviceInit)
                    {
                        MotionDevInitialized();
                        _motionDevice.TransmitAxState += MotionDevice_TransmitAxState;
                        _motionDevice.StartMonitoringAsync();
                    }
                },TaskScheduler.Default);
        }

        public void StartMonitoringState()
        {
            /*_monitoringMachineState = */
            if (IsMotionDeviceInit) _ = _motionDevice.StartMonitoringAsync();
        }

        public event EventHandler<AxisStateEventArgs> OnAxisMotionStateChanged;
        protected abstract void OnVelocityRegimeChanged(Velocity velocity);

        public Velocity VelocityRegime { get; set; }
        public bool IsMotionDeviceInit { get; set; }

        public IHasMotion AddGroup(Groups group, params Ax[] axes)
        {
            if (_axesGroups is null)
            {
                _axesGroups = new();
            }

            if (!_axesGroups.ContainsKey(group))
            {
                var axesNums = _axes
                .Where(a => axes.Contains(a.Key))
                .Select(n => n.Value.AxisNum)
                .ToArray();
                _axesGroups.Add(group, (_motionDevice.FormAxesGroup(axesNums), axes));
            }
            return this;
        }

        public void EmgScenario()
        {
            throw new NotImplementedException();
        }

        public void EmgStop()
        {
            throw new NotImplementedException();
        }

        public void GoWhile(Ax axis, AxDir direction)
        {
            ResetErrors(axis);
            _axes[axis].SetMotionStarted();

            _motionDevice.MoveAxisContiniouslyAsync(_axes[axis].AxisNum, direction);
        }

        public async Task MoveAxInPosAsync(Ax axis, double position, bool precisely = false)
        {
            if (!_axes[axis].Busy)
            {
                SetAxisBusy(axis);
               
                if (precisely)
                {
                    await _motionDevice.MoveAxisPreciselyAsync(_axes[axis].AxisNum, _axes[axis].LineCoefficient, position).ConfigureAwait(false);
                    //await _motionDevice.MoveAxisPreciselyAsync_2(_axes[axis].AxisNum, _axes[axis].LineCoefficient, position).ConfigureAwait(false);

                }
                else
                {
                    await _motionDevice.MoveAxisAsync(_axes[axis].AxisNum, position).ConfigureAwait(false);
                }
                ResetAxisBusy(axis);
            }
        }

        public async Task MoveGpInPosAsync(Groups group, double[] positions, bool precisely = false)
        {
            Guard.HasSizeEqualTo(positions, _axesGroups[group].axes.Length, nameof(positions));

            if (!BusyGroup(group))
            {

                var motionDones = _axesGroups[group].axes
                    .Zip(positions, (ax, pos) => (ax, _axes[ax].CmdPosition - pos))
                    .Where(ax => ax.Item2 != 0);

                motionDones.Select(ax => _axes[ax.ax].SetMotionStarted())
                .ToList();

                var k = new double();
                try
                {
                    k = Math.Abs((positions.First() - _axes[Ax.X].CmdPosition) /
                                 (positions.Last() - _axes[Ax.Y].ActualPosition)); //ctg a
                }
                catch (DivideByZeroException)
                {
                    k = 1000;
                }

                var vx = _velRegimes[Ax.X][VelocityRegime];
                var vy = _velRegimes[Ax.Y][VelocityRegime];
                var kmax = vx / vy; // ctg a

                var v = (k / kmax) switch
                {
                    1 => Math.Sqrt(vx * vx + vy * vy),
                    < 1 => vy / Math.Sin(Math.Atan(1 / k)), // / Math.Sqrt(1 / (1 + k * k)),//yconst
                    > 1 => vx / Math.Cos(Math.Atan(1 / k)) //Math.Sqrt(k * k / (1 + k * k)) //xconst
                };
                _motionDevice.SetGroupVelocity(_axesGroups[group].groupNum, v);

                var axesNums = _axes.Where(a => _axesGroups[group].axes.Contains(a.Key))
                                        .Select(n => n.Value.AxisNum);

                var lineCoeffs = _axes.Where(a => _axesGroups[group].axes.Contains(a.Key))
                                      .Select(n => n.Value.LineCoefficient);

                var gpAxes = axesNums.Zip(lineCoeffs, (a, b) => new ValueTuple<int, double>(a, b)).ToArray();

                await _motionDevice.MoveGroupPreciselyAsync(_axesGroups[group].groupNum, positions, gpAxes);

                motionDones.Select(ax => _axes[ax.ax].SetMotionDone())
                .ToList();
            }
        }
        public async Task MoveAxRelativeAsync(Ax axis, double diffPosition, bool precisely = false)
        {
            var initialPos = precisely ? _axes[axis].ActualPosition : _axes[axis].CmdPosition; //TODO can it influence?
            var pos = initialPos + diffPosition;
            await MoveAxInPosAsync(axis, pos, precisely);
        }
        public async Task MoveGpRelativeAsync(Groups group, double[] offset, bool precisely = false)
        {
            Guard.HasSizeEqualTo(offset, _axesGroups[group].axes.Length, nameof(offset));

            var offsets = _axesGroups[group].axes.Select(ax => _axes[ax].ActualPosition).Zip(offset, (a, o) => a + o).ToArray();

            await MoveGpInPosAsync(group, offsets, precisely);

        }
        public void ResetErrors(Ax axis = Ax.All)
        {
            if (axis == Ax.All)
                _motionDevice.ResetErrors();
            else
                _motionDevice.ResetErrors(_axes[axis].AxisNum);
        }
        
        public void SetGroupConfig(int gpNum, MotionDeviceConfigs configs)
        {
            _motionDevice.SetGroupConfig(gpNum, configs);
        }

        public Velocity SetVelocity(Velocity velocity)
        {
            var oldVelocity = VelocityRegime;
            
            foreach (var axis in _axes)
            {
                if (axis.Value.VelRegimes != null)
                {
                    double vel = default;
                    if (axis.Value.VelRegimes.TryGetValue(velocity, out vel))
                    {
                        _motionDevice.SetAxisVelocity(axis.Value.AxisNum, axis.Value.VelRegimes[velocity]);
                        VelocityRegime = velocity;
                        OnVelocityRegimeChanged(VelocityRegime);
                    }
                }
                else
                {
                    throw new MotionException($"Не настроенны скоростные режимы оси {axis.Key}");
                }
            }

            return oldVelocity;
            //foreach (var group in _axesGroups.Values)
            //{
            //    //   MotionDevice.SetGroupVelocity(group.groupNum);
            //}
        }

        public void SetAxFeedSpeed(Ax axis, double feed)
        {
            _motionDevice.SetAxisVelocity(_axes[axis].AxisNum, feed);
        }

        public void EmergencyStop()
        {
            throw new NotImplementedException();
        }

        public void Stop(Ax axis)
        {
            _motionDevice.StopAxis(_axes[axis].AxisNum);
        }

        private bool BusyGroup(Groups group)
        {
            var busy = false;
            foreach (var axis in _axesGroups[group].axes)
            {
                busy |= _axes[axis].Busy;
            }

            return busy;
        }

        public async Task WaitUntilAxisStopAsync(Ax axis)
        {
            while (!_axes[axis].MotionDone) await Task.Delay(10);
        }

        public double GetAxisSetVelocity(Ax axis)
        {
            var velocity = new double();
            var regimes = new Dictionary<Velocity, double>();
            if (_velRegimes.TryGetValue(axis, out regimes))
            {
                if (!regimes.TryGetValue(VelocityRegime, out velocity))
                    throw new MachineException($"Заданный режим скорости не установлен для оси {axis}");
            }
            else
            {
                throw new MachineException($"Для оси {axis} не установленны режимы скоростные режимы");
            }

            return velocity;
        }

        public void ConfigureAxesGroups(Dictionary<Groups, Ax[]> groups)
        {
            _axesGroups = new Dictionary<Groups, (int groupNum, Ax[] axes)>();
            foreach (var group in groups)
            {
                var axesNums = _axes.Where(a => group.Value.Contains(a.Key)).Select(n => n.Value.AxisNum).ToArray();
                _axesGroups.Add(@group.Key, (_motionDevice.FormAxesGroup(axesNums), @group.Value));
            }
        }

        public void ConfigureDoubleFeatures(Dictionary<MFeatures, double> doubleFeatures)
        {
            _doubleFeatures = new Dictionary<MFeatures, double>(doubleFeatures);
        }

        public double GetFeature(MFeatures feature)
        {
            return _doubleFeatures[feature];
        }

        public async Task GoHomeAsync()
        {
            if (_homingConfigs is null)
            {
                throw new MachineException($"Homing mode is not configured. {nameof(_homingConfigs)} is null");
            }


            //_axes[Ax.X].SetMotionStarted();

            var par = _homingConfigs.Select(p =>
            (
              p.Value.direction,
              p.Value.homeRst,
              p.Value.homeMode,
              p.Value.velocity,
              _axes[p.Key].AxisNum
            )).ToArray();

            _homingConfigs.Select(a => _axes[a.Key].SetMotionStarted()).ToList();

            await _motionDevice.HomeMovingAsync(par);



            var tasks = _homingConfigs
                .Select(p =>

                MoveAxInPosAsync(p.Key, p.Value.positionAfterHoming, true)
            ).ToArray();


            await Task.WhenAll(tasks).ConfigureAwait(false);

            //_motionDevice.SetAxisCoordinate(_axes[Ax.X].AxisNum, 0d);
            //_motionDevice.SetAxisCoordinate(_axes[Ax.Y].AxisNum, 0d);


            //await MoveAxInPosAsync(Ax.X, _homingConfigs[Ax.X].positionAfterHoming, true).ConfigureAwait(false);
        }

        public IHomingBuilder ConfigureHomingForAxis(Ax axis)
        {
            return new HomingBuilder(axis, _homingConfigs);
        }

        public class HomingBuilder : IHomingBuilder
        {
            private readonly Ax _axis;
            Dictionary<Ax, (AxDir direction, HomeRst homeRst, HmMode homeMode, double velocity, double positionAfterHoming)> _homingConfigs;
            private AxDir _axDir = AxDir.Neg;
            private HomeRst _homeRst = HomeRst.HOME_RESET_EN;
            private HmMode _hmMode = HmMode.MODE2_Lmt;
            private double _velocity = 1;
            private double _positionAfterHoming = 0;
            public HomingBuilder(Ax axis, Dictionary<Ax, (AxDir direction, HomeRst homeRst, HmMode homeMode, double velocity, double positionAfterHoming)> homingConfigs)
            {
                _axis = axis;
                _homingConfigs = homingConfigs ?? throw new NullReferenceException($"{nameof(homingConfigs)}");
            }
            public void Configure()
            {
                if (!_homingConfigs.TryAdd(_axis, (_axDir, _homeRst, _hmMode, _velocity, _positionAfterHoming)))
                {
                    _homingConfigs[_axis] = (_axDir, _homeRst, _hmMode, _velocity, _positionAfterHoming);
                }
            }
            public IHomingBuilder SetHomingDirection(AxDir direction)
            {
                _axDir = direction;
                return this;
            }
            public IHomingBuilder SetHomingReset(HomeRst homeRst)
            {
                _homeRst = homeRst;
                return this;
            }
            public IHomingBuilder SetHomingMode(HmMode hmMode)
            {
                _hmMode = hmMode;
                return this;
            }
            public IHomingBuilder SetHomingVelocity(double velocity)
            {
                _velocity = velocity;
                return this;
            }
            public IHomingBuilder SetPositionAfterHoming(double position)
            {
                _positionAfterHoming = position;
                return this;
            }
        }
        

        public IAxisBuilder AddAxis(Ax ax, double lineCoefficient)
        {
            _axes ??= new();
            if (_axes.Count == _motionDevice.AxisCount && !_axes.ContainsKey(ax))
            {
                throw new ArgumentException("The motion device is already equipted with maximum amount of axes", nameof(ax));
            }
            var num = _axes.ContainsKey(ax) ? _axes.Keys.FindIndex(axis=>axis == ax) : _axes.Count;
            IAxis axis  = new Axis(lineCoefficient,num);
            _axes[ax] = axis;
            _velRegimes ??= new();
            return new AxisBuilder(ax, ref axis, num, in _motionDevice, ref _velRegimes);
        }

        public class AxisBuilder : IAxisBuilder
        {
            private readonly Ax _ax;
            private readonly IAxis _axis;
            private readonly int _axNum;
            private readonly IMotionDevicePCI1240U _motionDevice;
            private readonly Dictionary<Ax, Dictionary<Velocity, double>> _velRegimes;
            private MotionDeviceConfigs _axMotDevConfigs;
            private Dictionary<Velocity, double> _regimes = new();

            public AxisBuilder(Ax ax, ref IAxis axis, int axNum, in IMotionDevicePCI1240U motionDevice,
                ref Dictionary<Ax, Dictionary<Velocity, double>> velRegimes)
            {
                _ax = ax;
                _axis = axis;
                _axNum = axNum;
                _motionDevice = motionDevice;
                _velRegimes = velRegimes;
            }
            public AxisBuilder WithConfigs(MotionDeviceConfigs configs)
            {
                _axMotDevConfigs = configs;
                return this;
            }
            public AxisBuilder WithVelRegime(Velocity velocity, double @value)
            {
                _regimes[velocity] = value;
                return this;
            }
            public void Build()
            {
                _motionDevice.SetAxisConfig(_axNum, _axMotDevConfigs);
                _velRegimes[_ax] = _regimes;
                _axis.VelRegimes = _regimes;
            }
        }
        private void MotionDevice_TransmitAxState(object obj, AxNumEventArgs axNumEventArgs)
        {
            if (_axes is not null)
            {
                var axisNum = axNumEventArgs.AxisNum;
                var state = axNumEventArgs.AxisState;
                if (axisNum < _axes.Count && _axes.Count > 0)
                {
                    var axis = _axes.ToList().Where(a => a.Value.AxisNum == axisNum).First().Key;
                    _axes[axis].ActualPosition = _axes[axis].LineCoefficient == 0 ? state.cmdPos : state.actPos /** _axes[axis].LineCoefficient*/;
                    _axes[axis].CmdPosition = state.cmdPos;
                    _axes[axis].DIs = state.sensors;
                    _axes[axis].DOs = state.outs;
                    _axes[axis].LmtN = state.nLmt;
                    _axes[axis].LmtP = state.pLmt;
                    _axes[axis].HomeDone = state.homeDone;
                    _axes[axis].MotionDone = state.motionDone;
                    _axes[axis].VHStart = state.vhStart;
                    _axes[axis].VHEnd = state.vhEnd;

                    OnAxisMotionStateChanged?.Invoke(this,
                        new AxisStateEventArgs(
                        axis: axis,
                        position: _axes[axis].ActualPosition,
                        cmdPosition: _axes[axis].CmdPosition,
                        nLmt: _axes[axis].LmtN,
                        pLmt: _axes[axis].LmtP,
                        motionDone: _axes[axis].MotionDone,
                        motionStart: _axes[axis].VHStart,
                        eZ: state.ez,
                        org: state.org));

                    GetAxOutNIn(axis, state.outs, state.sensors);
                }
            }
        }

        protected virtual void GetAxOutNIn(Ax ax, int outs, int ins) { }


        private void SetAxisBusy(Ax axis)
        {
            _axes[axis].Busy = true;
        }

        private void ResetAxisBusy(Ax axis)
        {
            _axes[axis].Busy = false;
        }

        public double GetAxActual(Ax axis)
        {
            //return _motionDevice.GetAxActual(_axes[axis].AxisNum) * _axes[axis].LineCoefficient;
            return _motionDevice.GetAxActual(_axes[axis].AxisNum);
        }
        public double GetAxCmd(Ax axis)
        {
            //return _motionDevice.GetAxActual(_axes[axis].AxisNum) * _axes[axis].LineCoefficient;
            return _motionDevice.GetAxCmd(_axes[axis].AxisNum);
        }

        public void SetPrecision(double tolerance) => _motionDevice.SetPrecision(tolerance);

        public abstract void MotionDevInitialized();
       
    }
}
