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
        protected readonly ExceptionsAgregator _exceptionsAgregator;
        protected readonly MotionDevicePCI1240U _motionDevice;

        protected Dictionary<Ax, IAxis> _axes;
        private Dictionary<Groups, (int groupNum, Ax[] axes)> _axesGroups;
        private Dictionary<MFeatures, double> _doubleFeatures;
        //private Dictionary<Place, (Ax axis, double pos)[]> _places;
        //private Dictionary<Place, double> _singlePlaces;
        protected Dictionary<Ax, Dictionary<Velocity, double>> _velRegimes;
        private Dictionary<Ax, (AxDir direction, HomeRst homeRst, HmMode homeMode, double velocity, double positionAfterHoming)> _homingConfigs = new();

        //private Dictionary<Sensors, (Ax axis, Di dIn, bool invertion, string name)> _sensors;
        //private Dictionary<Valves, (Ax axis, Do dOut)> _valves;

        public PCI124XXMachine(ExceptionsAgregator exceptionsAgregator, MotionDevicePCI1240U motionDevice)
        {
            _exceptionsAgregator = exceptionsAgregator;

            _motionDevice = motionDevice;
            _exceptionsAgregator.RegisterMessager(_motionDevice);

            if (_motionDevice.DevicesConnection())
            {
                _motionDevice.StartMonitoringAsync();
                _motionDevice.TransmitAxState += MotionDevice_TransmitAxState;
            }
        }

        public event EventHandler<AxisStateEventArgs> OnAxisMotionStateChanged;
        public Velocity VelocityRegime { get; set; }
        public bool MachineInit { get; set; }

        public IHasMotion AddGroup(Groups group, Ax[] axes)
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

        public void ConfigureAxes((Ax axis, double linecoefficient)[] ax)
        {
            if (ax.Length <= _motionDevice.AxisCount)
            {
                _axes = new Dictionary<Ax, IAxis>(ax.Length);
                for (var axnum = 0; axnum < ax.Length; axnum++)
                    _axes.Add(ax[axnum].axis, new Axis(ax[axnum].linecoefficient, axnum));
            }
        }

        //public void ConfigureGeometry(Dictionary<Place, (Ax, double)[]> places)
        //{
        //    if (_places is not null)
        //        places.ToList().ForEach(e =>
        //        {
        //            if (_places.ContainsKey(e.Key))
        //                _places[e.Key] = e.Value;
        //            else
        //                _places.Add(e.Key, e.Value);
        //        });
        //    else
        //        _places = new Dictionary<Place, (Ax, double)[]>(places);
        //}
        //public async Task GoThereAsync(Place place, bool precisely = false)
        //{
        //    if (place != Place.Home)
        //    {
        //        if (precisely)
        //        {
        //            var ax = new (int, double, double)[_places[place].Length];
        //            for (var i = 0; i < _places[place].Length; i++)
        //            {
        //                var axis = _places[place][i].axis;
        //                ax[i] = (_axes[axis].AxisNum, _places[place][i].pos, _axes[axis].LineCoefficient);
        //            }

        //            _motionDevice.MoveAxesByCoorsPrecAsync(ax);
        //        }
        //        else
        //        {
        //            var ax = new (int, double)[_places[place].Length];
        //            for (var i = 0; i < _places[place].Length; i++)
        //            {
        //                var axis = _places[place][i].axis;
        //                ax[i] = (_axes[axis].AxisNum, _places[place][i].pos);
        //            }

        //            _motionDevice.MoveAxesByCoorsAsync(ax);
        //        }
        //    }
        //    else
        //    {
        //        var arr = new (int, double, uint)[]
        //        {
        //            (_axes[Ax.X].AxisNum, _velRegimes[Ax.X][Velocity.Service], 1),
        //            (_axes[Ax.Y].AxisNum, _velRegimes[Ax.Y][Velocity.Service], 5),
        //            (_axes[Ax.Z].AxisNum, _velRegimes[Ax.Z][Velocity.Service], 1)
        //        };
        //        var axArr = new[] { Ax.X, Ax.Z };
        //        _motionDevice.HomeMoving(arr);
        //        foreach (var axis in axArr)
        //            Task.Run(() =>
        //            {
        //                while (!_axes[axis].LmtN) Task.Delay(10).Wait();
        //                ResetErrors(axis);
        //                _motionDevice.ResetAxisCounter(_axes[axis].AxisNum);
        //                MoveAxInPosAsync(axis, 1, true);
        //            });

        //        _motionDevice.ResetAxisCounter(_axes[Ax.U].AxisNum);
        //    }
        //}
        //public async Task MoveGpInPlaceAsync(Groups group, Place place, bool precisely = false)
        //{
        //    MoveGpInPosAsync(group, _places[place].Select(p => p.pos).ToArray(), precisely);
        //}

        //public async Task MoveAxesInPlaceAsync(Place place)
        //{
        //    foreach (var axpos in _places[place]) await MoveAxInPosAsync(axpos.axis, axpos.pos);
        //}

        ///// <summary>
        /////     Actual coordinates translation
        ///// </summary>
        ///// <param name="place"></param>
        ///// <returns>Axes actual coordinates - place coordinates relatively</returns>
        //public (Ax, double)[] TranslateActualCoors(Place place)
        //{
        //    var count = _places[place].Length;
        //    var arr = new (Ax, double)[count];
        //    for (var i = 0; i < count; i++)
        //    {
        //        var pl = _places[place][i];
        //        arr[i] = (pl.axis, _axes[pl.axis].ActualPosition - pl.pos);
        //    }

        //    return arr;
        //}

        //public double TranslateActualCoors(Place place, Ax axis)
        //{
        //    var res = new double();
        //    try
        //    {
        //        var ax = _axes[axis];
        //        res = ax.ActualPosition - _places[place].Where(a => a.axis == axis).First().pos;
        //    }
        //    catch (KeyNotFoundException)
        //    {
        //        throw new MachineException("Запрашиваемое место отсутствует");
        //    }
        //    catch (IndexOutOfRangeException)
        //    {
        //        throw new MachineException($"Для места {place} не обозначена координата {axis}");
        //    }

        //    return res;
        //}

        ///// <summary>
        /////     Coordinate translation
        ///// </summary>
        ///// <param name="place"></param>
        ///// <param name="position"></param>
        ///// <returns>place coors - position coors</returns>
        //public (Ax, double)[] TranslateActualCoors(Place place, (Ax axis, double pos)[] position)
        //{
        //    var temp = new List<(Ax, double)>();
        //    foreach (var p in position)
        //        if (_places[place].Select(a => a.axis).Contains(p.axis))
        //            temp.Add((p.axis, _places[place].GetVal(p.axis) - p.pos));
        //    var arr = temp.ToArray();
        //    return arr;
        //}

        //public double TranslateSpecCoor(Place place, double position, Ax axis)
        //{
        //    var pl = new double();

        //    try
        //    {
        //        pl = _places[place].Where(a => a.axis == axis).Select(p => p.pos).First() - position;
        //    }
        //    catch (ArgumentNullException)
        //    {
        //        throw new MachineException($"Координаты {axis} места {place} не существует");
        //    }

        //    return pl;
        //}
        //public void ConfigureGeometry(Dictionary<Place, double> places)
        //{
        //    _singlePlaces = new Dictionary<Place, double>(places);
        //}

        //public double GetGeometry(Place place, int arrNum)
        //{
        //    var pl = new double();

        //    try
        //    {
        //        pl = _places[place][arrNum].pos;
        //    }
        //    catch (KeyNotFoundException)
        //    {
        //        throw new MachineException("Запрашиваемое место отсутствует");
        //    }
        //    catch (IndexOutOfRangeException)
        //    {
        //        throw new MachineException($"Для места {place} не обозначена координата № {arrNum}");
        //    }

        //    return pl;
        //}

        //public double GetGeometry(Place place, Ax axis)
        //{
        //    var pl = new double();
        //    var arrNum = new int();

        //    if (!_places.ContainsKey(place))
        //        throw new MachineException("Запрашиваемое место отсутствует");
        //    try
        //    {
        //        pl = _places[place].Where(a => a.axis == axis).First().pos;
        //    }
        //    catch (KeyNotFoundException)
        //    {
        //        throw new MachineException($"Ось {axis} не сконфигурированна");
        //    }
        //    catch (ArgumentOutOfRangeException ex)
        //    {
        //        throw new MachineException($"Координаты в позиции {arrNum} места {place} не существует");
        //    }

        //    return pl;
        //}
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
                //if (_axes[axis].CmdPosition - position != 0)
                //{
                //    _axes[axis].SetMotionStarted();
                //}
                if (precisely)
                {
                    await _motionDevice.MoveAxisPreciselyAsync(_axes[axis].AxisNum, _axes[axis].LineCoefficient, position).ConfigureAwait(false);
                }
                else
                {
                    await Task.Run(() =>
                    {
                        _motionDevice.MoveAxisAsync(_axes[axis].AxisNum, position);
                        while (!_axes[axis].MotionDone) ;
                    }).ConfigureAwait(false);
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

                if (precisely)
                {
                    var gpNum = _axesGroups[group].groupNum;

                    var axesNums = _axes.Where(a => _axesGroups[group].axes.Contains(a.Key))
                                        .Select(n => n.Value.AxisNum);

                    var lineCoeffs = _axes.Where(a => _axesGroups[group].axes.Contains(a.Key))
                                          .Select(n => n.Value.LineCoefficient);

                    var gpAxes = axesNums.Zip(lineCoeffs, (a, b) => new ValueTuple<int, double>(a, b)).ToArray();

                    var n = _axesGroups[group].axes.FindIndex(a => a == Ax.Y);

                    positions[n] -= 0.03;

                    await _motionDevice.MoveGroupPreciselyAsync(gpNum, positions, gpAxes);

                    positions[n] += 0.03;

                    await _motionDevice.MoveAxisPreciselyAsync(_axes[Ax.Y].AxisNum, _axes[Ax.Y].LineCoefficient,
                        positions[n]);
                }
                else
                {
                    await _motionDevice.MoveGroupAsync(_axesGroups[group].groupNum, positions);
                }
                motionDones.Select(ax => _axes[ax.ax].SetMotionDone())
                .ToList();
            }
        }
        public async Task MoveAxRelativeAsync(Ax axis, double diffPosition, bool precisely = false)
        {
            var initialPos = _axes[axis].ActualPosition;
            await MoveAxInPosAsync(axis, initialPos + diffPosition, precisely);
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

        public void SetConfigs((Ax axis, MotionDeviceConfigs configs)[] axesConfigs)
        {
            var count = axesConfigs.Length <= 4 ? axesConfigs.Length : 4;
            for (var i = 0; i < count; i++)
            {
                var ax = axesConfigs[i].axis;
                var configs = axesConfigs[i].configs;
                _motionDevice.SetAxisConfig(_axes[ax].AxisNum, configs);
            }
        }

        public void SetVelocity(Velocity velocity)
        {
            VelocityRegime = velocity;
            foreach (var axis in _axes)
                if (axis.Value.VelRegimes != null)
                {
                    double vel = default;
                    if (axis.Value.VelRegimes.TryGetValue(velocity, out vel))
                    {
                        _motionDevice.SetAxisVelocity(axis.Value.AxisNum, axis.Value.VelRegimes[velocity]);
                    }
                }
                else
                    throw new MotionException($"Не настроенны скоростные режимы оси {axis.Key.ToString()}");
            foreach (var group in _axesGroups.Values)
            {
                //   MotionDevice.SetGroupVelocity(group.groupNum);
            }
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



        public void ConfigureVelRegimes(Dictionary<Ax, Dictionary<Velocity, double>> velRegimes)
        {
            _velRegimes = new Dictionary<Ax, Dictionary<Velocity, double>>(velRegimes);
            foreach (var axis in _axes)
                try
                {
                    axis.Value.VelRegimes = new Dictionary<Velocity, double>(_velRegimes[axis.Key]);
                }
                catch (KeyNotFoundException)
                {
                    throw new MotionException($"Для оси {axis.Key} не заданы скоростные режимы");
                }
        }
        public async Task WaitUntilAxisStopAsync(Ax axis)
        {
            //var status = new uint();
            //await Task.Run(() =>
            //{
            //    while (!_axes[axis].MotionDone) Task.Delay(10).Wait();
            //});

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


            _axes[Ax.X].SetMotionStarted();

            var par = _homingConfigs.Select(p =>
            (
              p.Value.direction,
              p.Value.homeRst,
              p.Value.homeMode,
              p.Value.velocity,
              _axes[p.Key].AxisNum
            )).ToArray();

            _homingConfigs.Select(a => _axes[a.Key].SetMotionStarted()).ToList();

            _motionDevice.HomeMovingAsync(par);

            var tasks = _homingConfigs.Select(async p =>
            {
                while (!_axes[p.Key].MotionDone) await Task.Delay(10).ConfigureAwait(false);
                ResetErrors(p.Key);
                await MoveAxInPosAsync(p.Key, p.Value.positionAfterHoming, true).ConfigureAwait(false);
                _motionDevice.ResetAxisCounter(_axes[p.Key].AxisNum);
            }).ToArray();

            await Task.WhenAll(tasks).ConfigureAwait(false);
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


        private void MotionDevice_TransmitAxState(object obj, AxNumEventArgs axNumEventArgs)
        {
            if (_axes is not null)
            {
                var axisNum = axNumEventArgs.AxisNum;
                var state = axNumEventArgs.AxisState;
                if (axisNum < _axes.Count())
                {
                    var axis = _axes.Where(a => a.Value.AxisNum == axisNum).First().Key;
                    _axes[axis].ActualPosition = state.actPos * _axes[axis].LineCoefficient;
                    _axes[axis].CmdPosition = state.cmdPos;
                    _axes[axis].DIs = state.sensors;
                    _axes[axis].DOs = state.outs;
                    _axes[axis].LmtN = state.nLmt;
                    _axes[axis].LmtP = state.pLmt;
                    _axes[axis].HomeDone = state.homeDone;
                    _axes[axis].MotionDone = state.motionDone;
                    _axes[axis].VHStart = state.vhStart;
                    _axes[axis].VHEnd = state.vhEnd;

                    var position = _axes[axis].ActualPosition;
                    if (_axes[axis].LineCoefficient == 0) position = state.cmdPos;

                    //OnAxisMotionStateChanged?.Invoke(this, new AxisStateEventArgs(axis, position, state.cmdPos, state.nLmt, state.pLmt, state.motionDone, state.vhStart));

                    OnAxisMotionStateChanged?.Invoke(this,
                        new AxisStateEventArgs(
                        axis: axis,
                        position: _axes[axis].LineCoefficient == 0 ? _axes[axis].CmdPosition : _axes[axis].ActualPosition,
                        cmdPosition: _axes[axis].CmdPosition,
                        nLmt: _axes[axis].LmtN,
                        pLmt: _axes[axis].LmtP,
                        motionDone: _axes[axis].MotionDone,
                        motionStart: _axes[axis].VHStart));

                }
            }
        }

        private void SetAxisBusy(Ax axis)
        {
            _axes[axis].Busy = true;
        }

        private void ResetAxisBusy(Ax axis)
        {
            _axes[axis].Busy = false;
        }


    }
}
