using MachineClassLibrary.Classes;
using MachineClassLibrary.Machine.MotionDevices;
using MachineClassLibrary.SFC;
using MachineClassLibrary.VideoCapture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MachineClassLibrary.Machine.Machines
{
    public abstract class PCI124XXMachine : IHasMotion
    {
        protected readonly ExceptionsAgregator _exceptionsAgregator;
        protected readonly MotionDevicePCI1240U _motionDevice;

        protected Dictionary<Ax, IAxis> _axes;
        private Dictionary<Groups, (int groupNum, Ax[] axes)> _axesGroups;
        private Dictionary<MFeatures, double> _doubleFeatures;
        private Dictionary<Place, (Ax axis, double pos)[]> _places;
        private Dictionary<Place, double> _singlePlaces;
        private Dictionary<Ax, Dictionary<Velocity, double>> _velRegimes;

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

        public void AddGroup(Groups group, IAxis[] axes)
        {
            throw new NotImplementedException();
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

        public void ConfigureGeometry(Dictionary<Place, (Ax, double)[]> places)
        {
            if (_places is not null)
                places.ToList().ForEach(e =>
                {
                    if (_places.ContainsKey(e.Key))
                        _places[e.Key] = e.Value;
                    else
                        _places.Add(e.Key, e.Value);
                });
            else
                _places = new Dictionary<Place, (Ax, double)[]>(places);
        }

        public void EmgScenario()
        {
            throw new NotImplementedException();
        }

        public void EmgStop()
        {
            throw new NotImplementedException();
        }

        public async Task GoThereAsync(Place place, bool precisely = false)
        {
            if (place != Place.Home)
            {
                if (precisely)
                {
                    var ax = new (int, double, double)[_places[place].Length];
                    for (var i = 0; i < _places[place].Length; i++)
                    {
                        var axis = _places[place][i].axis;
                        ax[i] = (_axes[axis].AxisNum, _places[place][i].pos, _axes[axis].LineCoefficient);
                    }

                    _motionDevice.MoveAxesByCoorsPrecAsync(ax);
                }
                else
                {
                    var ax = new (int, double)[_places[place].Length];
                    for (var i = 0; i < _places[place].Length; i++)
                    {
                        var axis = _places[place][i].axis;
                        ax[i] = (_axes[axis].AxisNum, _places[place][i].pos);
                    }

                    _motionDevice.MoveAxesByCoorsAsync(ax);
                }
            }
            else
            {
                var arr = new (int, double, uint)[]
                {
                    (_axes[Ax.X].AxisNum, _velRegimes[Ax.X][Velocity.Service], 1),
                    (_axes[Ax.Y].AxisNum, _velRegimes[Ax.Y][Velocity.Service], 5),
                    (_axes[Ax.Z].AxisNum, _velRegimes[Ax.Z][Velocity.Service], 1)
                };
                var axArr = new[] { Ax.X, Ax.Z };
                _motionDevice.HomeMoving(arr);
                foreach (var axis in axArr)
                    Task.Run(() =>
                    {
                        while (!_axes[axis].LmtN) Task.Delay(10).Wait();
                        ResetErrors(axis);
                        _motionDevice.ResetAxisCounter(_axes[axis].AxisNum);
                        MoveAxInPosAsync(axis, 1, true);
                    });

                _motionDevice.ResetAxisCounter(_axes[Ax.U].AxisNum);
            }
        }

        public void GoWhile(Ax axis, AxDir direction)
        {
            ResetErrors(axis);
            _motionDevice.MoveAxisContiniouslyAsync(_axes[axis].AxisNum, direction);
        }

        public async Task MoveAxInPosAsync(Ax axis, double position, bool precisely = false)
        {
            if (!_axes[axis].Busy)
            {
                SetAxisBusy(axis);
                if (precisely)
                    await _motionDevice.MoveAxisPreciselyAsync(_axes[axis].AxisNum, _axes[axis].LineCoefficient,
                        position);
                else
                    await Task.Run(() =>
                    {
                        _motionDevice.MoveAxisAsync(_axes[axis].AxisNum, position);
                        while (!_axes[axis].MotionDone) ;
                    });
                ResetAxisBusy(axis);
            }
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

        public async Task MoveGpInPosAsync(Groups group, double[] position, bool precisely = false)
        {


            if (!BusyGroup(group))
            {
                var k = new double();
                try
                {
                    k = Math.Abs((position.First() - _axes[Ax.X].CmdPosition) /
                                 (position.Last() - _axes[Ax.Y].ActualPosition)); //ctg a
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
                    var axesNums = _axes.Where(a => _axesGroups[group].axes.Contains(a.Key)).Select(n => n.Value.AxisNum);
                    var lineCoeffs = _axes.Where(a => _axesGroups[group].axes.Contains(a.Key))
                        .Select(n => n.Value.LineCoefficient);
                    var gpAxes = axesNums.Zip(lineCoeffs, (a, b) => new ValueTuple<int, double>(a, b)).ToArray();

                    var n = _axesGroups[group].axes.FindIndex(a => a == Ax.Y);

                    position[n] -= 0.03;

                    await _motionDevice.MoveGroupPreciselyAsync(gpNum, position, gpAxes);

                    position[n] += 0.03;

                    await _motionDevice.MoveAxisPreciselyAsync(_axes[Ax.Y].AxisNum, _axes[Ax.Y].LineCoefficient,
                        position[n]);
                }
                else
                {
                    await _motionDevice.MoveGroupAsync(_axesGroups[group].groupNum, position);
                }
            }
        }

        public async Task MoveGpInPlaceAsync(Groups group, Place place, bool precisely = false)
        {
            MoveGpInPosAsync(group, _places[place].Select(p => p.pos).ToArray(), precisely);
        }

        public async Task MoveAxesInPlaceAsync(Place place)
        {
            foreach (var axpos in _places[place]) await MoveAxInPosAsync(axpos.axis, axpos.pos);
        }

        /// <summary>
        ///     Actual coordinates translation
        /// </summary>
        /// <param name="place"></param>
        /// <returns>Axes actual coordinates - place coordinates relatively</returns>
        public (Ax, double)[] TranslateActualCoors(Place place)
        {
            var count = _places[place].Length;
            var arr = new (Ax, double)[count];
            for (var i = 0; i < count; i++)
            {
                var pl = _places[place][i];
                arr[i] = (pl.axis, _axes[pl.axis].ActualPosition - pl.pos);
            }

            return arr;
        }

        public double TranslateActualCoors(Place place, Ax axis)
        {
            var res = new double();
            try
            {
                var ax = _axes[axis];
                res = ax.ActualPosition - _places[place].Where(a => a.axis == axis).First().pos;
            }
            catch (KeyNotFoundException)
            {
                throw new MachineException("Запрашиваемое место отсутствует");
            }
            catch (IndexOutOfRangeException)
            {
                throw new MachineException($"Для места {place} не обозначена координата {axis}");
            }

            return res;
        }

        /// <summary>
        ///     Coordinate translation
        /// </summary>
        /// <param name="place"></param>
        /// <param name="position"></param>
        /// <returns>place coors - position coors</returns>
        public (Ax, double)[] TranslateActualCoors(Place place, (Ax axis, double pos)[] position)
        {
            var temp = new List<(Ax, double)>();
            foreach (var p in position)
                if (_places[place].Select(a => a.axis).Contains(p.axis))
                    temp.Add((p.axis, _places[place].GetVal(p.axis) - p.pos));
            var arr = temp.ToArray();
            return arr;
        }

        public double TranslateSpecCoor(Place place, double position, Ax axis)
        {
            var pl = new double();

            try
            {
                pl = _places[place].Where(a => a.axis == axis).Select(p => p.pos).First() - position;
            }
            catch (ArgumentNullException)
            {
                throw new MachineException($"Координаты {axis} места {place} не существует");
            }

            return pl;
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

        public void ConfigureGeometry(Dictionary<Place, double> places)
        {
            _singlePlaces = new Dictionary<Place, double>(places);
        }

        public double GetGeometry(Place place, int arrNum)
        {
            var pl = new double();

            try
            {
                pl = _places[place][arrNum].pos;
            }
            catch (KeyNotFoundException)
            {
                throw new MachineException("Запрашиваемое место отсутствует");
            }
            catch (IndexOutOfRangeException)
            {
                throw new MachineException($"Для места {place} не обозначена координата № {arrNum}");
            }

            return pl;
        }

        public double GetGeometry(Place place, Ax axis)
        {
            var pl = new double();
            var arrNum = new int();

            if (!_places.ContainsKey(place))
                throw new MachineException("Запрашиваемое место отсутствует");
            try
            {
                pl = _places[place].Where(a => a.axis == axis).First().pos;
            }
            catch (KeyNotFoundException)
            {
                throw new MachineException($"Ось {axis} не сконфигурированна");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new MachineException($"Координаты в позиции {arrNum} места {place} не существует");
            }

            return pl;
        }

        public async Task WaitUntilAxisStopAsync(Ax axis)
        {
            var status = new uint();
            await Task.Run(() =>
            {
                while (!_axes[axis].MotionDone) Task.Delay(10).Wait();
            });
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

        private void MotionDevice_TransmitAxState(object obj, AxNumEventArgs axNumEventArgs)
        {
            var axisNum = axNumEventArgs.AxisNum;
            var state = axNumEventArgs.AxisState;

            if (_axes != null)
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
                var position = _axes[axis].ActualPosition;
                if (_axes[axis].LineCoefficient == 0) position = state.cmdPos;

                OnAxisMotionStateChanged?.Invoke(this, new AxisStateEventArgs(axis, position, state.nLmt, state.pLmt, state.motionDone,
                    state.vhStart));



                //foreach (var sensor in Enum.GetValues(typeof(Sensors)))
                //    if (_sensors != null)
                //    {
                //        var ax = _sensors[(Sensors)sensor].axis;
                //        var condition = _axes[ax].GetDi(_sensors[(Sensors)sensor].dIn) ^
                //                        _sensors[(Sensors)sensor].invertion;
                //        if (!condition & (_spindleBlockers?.Contains((Sensors)sensor) ?? false))
                //        {
                //            StopSpindle();
                //            //throw new MachineException(
                //            //    $"Аварийное отключение шпинделя. {_sensors[(Sensors) sensor].name}");
                //        }
                //        OnSensorStateChanged?.Invoke(this, new SensorsEventArgs((Sensors)sensor, condition));
                //    }

                //foreach (var valve in Enum.GetValues(typeof(Valves)))
                //    if (_valves != null)
                //    {
                //        var ax = _valves[(Valves)valve].axis;
                //        OnValveStateChanged?.Invoke(this, new ValveEventArgs((Valves)valve, _axes[ax].GetDo(_valves[(Valves)valve].dOut)));
                //    }
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
