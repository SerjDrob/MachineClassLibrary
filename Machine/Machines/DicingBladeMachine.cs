using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MachineClassLibrary.Classes;
using MachineClassLibrary.Machine.MotionDevices;
using MachineClassLibrary.SFC;
using MachineClassLibrary.VideoCapture;

namespace MachineClassLibrary.Machine.Machines
{
    public class DicingBladeMachine : PCI124XXMachine, IHasCamera, IHasSCF, IHasValves, IHasSensors, IDisposable, IHasPlaces<Place>
    {

        private readonly ISpindle _spindle;
        private readonly IVideoCapture _videoCamera;
        private Dictionary<Valves, (Ax axis, Do dOut)> _valves;
        private Dictionary<Sensors, (Ax axis, Di dIn, bool invertion, string name, bool bridged)> _sensors;
        private Dictionary<Place, (Ax axis, double pos)[]> _places;
        private Dictionary<Place, double> _singlePlaces;
        private (Ax axis, bool isScanning) _scanHandle;
        private (Ax axis, Di di, bool isInverted) _emg;
        private bool disposedValue;
        public DicingBladeMachine(IMotionDevicePCI1240U motionDevice, IVideoCapture usbVideoCamera, ISpindle spindle) : base(motionDevice)
        {
            _videoCamera = usbVideoCamera;
            _videoCamera.OnBitmapChanged += _videoCamera_OnBitmapChanged;
            try
            {
                // TODO use IoC
                _spindle = spindle;
                _spindle.GetSpindleState += _spindle_GetSpindleState;
            }
            catch (SpindleException ex)
            {
                throw new MachineException($"Spindle initialization was failed with message: {ex.Message}");
            }
        }

        private void _videoCamera_OnBitmapChanged(object sender, VideoCaptureEventArgs Args)
        {
            OnBitmapChanged?.Invoke(this, Args);
        }

        public event EventHandler<ValveEventArgs> OnValveStateChanged;
        public event EventHandler<SensorsEventArgs> OnSensorStateChanged;
        public event EventHandler<SpindleEventArgs> OnSpindleStateChanging;
        public event EventHandler<VideoCaptureEventArgs> OnBitmapChanged;
        public event EventHandler CameraPlugged;
        public event EventHandler<Bitmap> OnRawBitmapChanged;
        public event EventHandler<bool> OnEMG_Pushed;


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

                    await _motionDevice.MoveAxesByCoorsPrecAsync(ax).ConfigureAwait(false);
                }
                else
                {
                    var ax = new (int, double)[_places[place].Length];
                    for (var i = 0; i < _places[place].Length; i++)
                    {
                        var axis = _places[place][i].axis;
                        ax[i] = (_axes[axis].AxisNum, _places[place][i].pos);
                    }

                    await _motionDevice.MoveAxesByCoorsAsync(ax).ConfigureAwait(false);//TODO it's not realy async
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
                await _motionDevice.HomeMovingAsync(arr).ConfigureAwait(false);
                foreach (var axis in axArr) _motionDevice.ResetAxisCounter(_axes[axis].AxisNum);
                _motionDevice.ResetAxisCounter(_axes[Ax.U].AxisNum);
            }
        }
        public async Task MoveGpInPlaceAsync(Groups group, Place place, bool precisely = false)
        {
            await MoveGpInPosAsync(group, _places[place].Select(p => p.pos).ToArray(), precisely).ConfigureAwait(false);
        }

        public async Task MoveAxesInPlaceAsync(Place place)
        {
            foreach (var axpos in _places[place]) await MoveAxInPosAsync(axpos.axis, axpos.pos).ConfigureAwait(false);
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
        //public void ConfigureSensors(Dictionary<Sensors, (Ax, Di, bool, string)> sensors)
        //{
        //    _sensors = new Dictionary<Sensors, (Ax, Di, bool, string)>(sensors);
        //}

        public void AddSensor(Sensors sensor, Ax axis, Di di, bool inverted, string name, bool bridged)
        {
            _sensors ??= new();
            if (!_sensors.TryAdd(sensor, (axis, di, inverted, name, bridged))) _sensors[sensor] = (axis, di, inverted, name, bridged);
        }

        public void AddValve(Valves valve, Ax axis, Do @do)
        {
            _valves ??= new();
            if (!_valves.TryAdd(valve, (axis, @do))) _valves[valve] = (axis, @do);
        }
        public void SetBridgeOnSensors(Sensors sensor, bool setBridge)
        {
            var s = _sensors[sensor];
            _sensors[sensor]= (s.axis,s.dIn,s.invertion,s.name,setBridge);
            //var num = _axes[_sensors[sensor].axis].AxisNum;
            //_motionDevice.SetBridgeOnAxisDin(num, (int)_sensors[sensor].dIn, setBridge);
        }

        public void SwitchOnValve(Valves valve)
        {
            _motionDevice.SetAxisDout(_axes[_valves[valve].axis].AxisNum, (ushort)_valves[valve].dOut, true);
        }

        public void SwitchOffValve(Valves valve)
        {
            _motionDevice.SetAxisDout(_axes[_valves[valve].axis].AxisNum, (ushort)_valves[valve].dOut, false);
        }

        public bool GetValveState(Valves valve)
        {
            return _motionDevice.GetAxisDout(_axes[_valves[valve].axis].AxisNum, (ushort)_valves[valve].dOut);
        }

        public string GetSensorName(Sensors sensor)
        {

            var name = "";
            try
            {
                name = _sensors[sensor].name;
            }
            catch (KeyNotFoundException)
            {
                throw new MachineException($"Датчик {sensor} не сконфигурирован");
            }

            return name;
        }

        //public void FreezeVideoCapture()
        //{
        //    _videoCamera.FreezeCameraImage();
        //}
        /// <summary>
        /// Set spindle's rpm
        /// </summary>
        /// <param name="frequency">measured in rpm</param>
        /// <exception cref="MachineException"></exception>
        public void SetSpindleFreq(int frequency)
        {
            try
            {
                _spindle.SetSpeedAsync((ushort)frequency);
            }
            catch (SpindleException ex)
            {
                throw new MachineException($"Setting of the spindle speed has fallen with {ex.Message}", ex);
            }
        }

        private Func<(bool canStart,IEnumerable<string> absentSensors)> _canStartSpindlePredicate = () => (true,[]);
        private bool _emgIsSet;

        public void SetSpindleStartBlocker(Func<(bool canStart, IEnumerable<string> absentSensors)> blocker)
        {
            _canStartSpindlePredicate = blocker;
        }

        public async Task StartSpindleAsync()
        {
            var result = _canStartSpindlePredicate.Invoke();
            if(!result.canStart) throw new MachineException($"Отсутствует: {string.Join(", ", result.absentSensors)}.");
            try
            {
                await _spindle.StartAsync().ConfigureAwait(false);
            }
            catch (SpindleException ex)
            {
                throw new MachineException($"Ошибка запуска шпинделя. {ex.Message}", ex);
            }
        }

        public Dictionary<int, (string, string[])> AvailableVideoCaptureDevices => _videoCamera.AvailableVideoCaptureDevices;

        public bool IsVideoCaptureConnected => _videoCamera.IsVideoCaptureConnected;

        public string VideoCaptureMessage => _videoCamera.VideoCaptureMessage;

        public bool AdjustWidthToHeight
        {
            get => _videoCamera.AdjustWidthToHeight;
            set
            {
                _videoCamera.AdjustWidthToHeight = value;
            }
        }
        public async Task StopSpindleAsync() => await _spindle.StopAsync().ConfigureAwait(false);

        private void _spindle_GetSpindleState(object obj, SpindleEventArgs e)
        {
            OnSpindleStateChanging?.Invoke(null, e);
        }
        public void StartCamera(int ind, int capabilitiesInd = 0)
        {
            _videoCamera.StartCamera(ind, capabilitiesInd);
        }

        public void FreezeCameraImage()
        {
            _videoCamera.FreezeCameraImage();
            IsCameraFreezed = true;
        }

        public void StopCamera()
        {
            _videoCamera.StopCamera();
        }
       

        /// <summary>
        /// Scan from current position both direction. After cancelling return to the position.
        /// </summary>
        /// <param name="ax">axis to be scanned</param>
        /// <param name="amplitude">scanning amplitude. From current position to both sides with amplitude/2 </param>
        /// <param name="speed">scanning speed. After scanning return VelocityRegime. The speed for the axis is immutable during scanning.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ScanByAxisAsync(Ax ax, double amplitude, double speed, CancellationToken cancellationToken)
        {
            if (_scanHandle.isScanning) return;
            _scanHandle = (ax, true);
            _exceptedVelAxis = ax;
            var initPosition = GetAxActual(ax);
            SetAxFeedSpeed(ax, speed);
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!cancellationToken.IsCancellationRequested) await MoveAxInPosAsync(Ax.X, initPosition + amplitude / 2).ConfigureAwait(false);
                if (!cancellationToken.IsCancellationRequested) await MoveAxInPosAsync(Ax.X, initPosition - amplitude / 2).ConfigureAwait(false);
            }
            await MoveAxInPosAsync(ax, initPosition).ConfigureAwait(false);
            _exceptedVelAxis = null;
            SetVelocity(VelocityRegime);
            _scanHandle.isScanning = false;
        }


        public int GetVideoCaptureDevicesCount() => _videoCamera.GetVideoCaptureDevicesCount();


        public int GetVideoCapabilitiesCount() => _videoCamera.GetVideoCapabilitiesCount();

        public IGeometryBuilder<Place> ConfigureGeometryFor(Place place)
        {
            _places ??= new();
            return new GeometryBuilder<Place>(place, ref _places);
        }
        public void SetEMG_In(Ax axis, Di di, bool isInverted) => _emg = (axis, di, isInverted);

        protected override void GetAxOutNIn(Ax ax, int outs, int ins)
        {
            if (ax == _emg.axis)
            {
                var emg_set = _emg.isInverted ^ ((ins & (1 << ((int)_emg.di))) != 0);
                if (emg_set && !_emgIsSet)
                {
                    EmgScenario();
                    _ = _spindle.StopAsync();
                    _emgIsSet = true;
                    OnEMG_Pushed?.Invoke(this, emg_set);
                }
            }
            if (_valves is null) return;
            foreach (var valve in _valves)
            {
                if (valve.Value.axis == ax)
                {
                    var state = (outs & (1 << (int)valve.Value.dOut)) != 0;
                    OnValveStateChanged?.Invoke(this, new(valve.Key, state));
                }
            }
            if (_sensors is null) return;
            var gap = ax switch
            {
                Ax.X => 0,
                Ax.Y => 30,
                Ax.Z => 60,
                Ax.U => 90,
                _ => 120
            };
            var line = 0;
            foreach (var sensor in _sensors)
            {            
                if (sensor.Value.axis == ax)
                {
                    var s = sensor.Value.bridged ? true : sensor.Value.invertion ^ (ins & (1 << ((int)sensor.Value.dIn))) != 0;
                    OnSensorStateChanged?.Invoke(this, new(sensor.Key, s));
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.SetCursorPosition(gap, 21 + line);
                    Console.Write("{0}:{1}: {2,-5}",ax,sensor.Value.name,s);
                    Console.ResetColor();
                    line++;
                }
            }
        }

        public void ResetEMG()
        {
            _motionDevice.ResetEMG_Regime();
            _emgIsSet = false;
        }
        public void InvokeSettings() => _videoCamera.InvokeSettings();

        public bool TryConnectSpindle()
        {
            try
            {
                _spindle.ConnectAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task<bool> ChangeSpindleFreqOnFlyAsync(ushort rpm, TimeSpan delay) => _spindle.ChangeSpeedAsync(rpm, delay);

        public override void MotionDevInitialized()
        {
            //throw new NotImplementedException();
        }

        public void SetCameraMirror(bool mirrorX, bool mirrorY)
        {
            _videoCamera.SetCameraMirror(mirrorX, mirrorY);
        }

        protected override void OnVelocityRegimeChanged(Velocity velocity)
        {
            // throw new NotImplementedException();
        }

        public float GetBlurIndex()
        {
            throw new NotImplementedException();
        }

        public class GeometryBuilder<TPlace> : IGeometryBuilder<TPlace> where TPlace : Enum
        {
            private Dictionary<TPlace, (Ax axis, double pos)[]> _places;
            private Dictionary<Ax, double> _positions = new();
            private readonly TPlace _configuringPlace;
            public GeometryBuilder(TPlace place, ref Dictionary<TPlace, (Ax axis, double pos)[]> places)
            {
                _places = places;
                _configuringPlace = place;
            }
            public IGeometryBuilder<TPlace> SetCoordinateForPlace(Ax axis, double coordinate)
            {
                _positions[axis] = coordinate;
                return this;
            }
            public void Build()
            {
                var ps = _positions.Select(p => (p.Key, p.Value)).ToArray();
                _places[_configuringPlace] = ps;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)

                    _spindle.Dispose();
                    _motionDevice.Dispose();
                    _videoCamera.OnBitmapChanged -= _videoCamera_OnBitmapChanged;
                    _videoCamera.StopCamera();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DicingBladeMachine()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void UnFreezeCamera()
        {
            _videoCamera.UnFreezeCamera();
            IsCameraFreezed = false;
        }

        public bool IsCameraFreezed { get; private set; }

        //public void StartVideoCapture(int ind, int capabilitiesInd = 0)
        //{
        //    _videoCamera.StartCamera(ind, capabilitiesInd);
        //}

        //public void StopVideoCapture()
        //{
        //    _videoCamera.StopCamera();
        //}

        //public int GetCamerasCount()=> _videoCamera.GetDevicesCount();
        //public int GetCameraCapabilitiesCount()=>_videoCamera.GetVideCapabilitiesCount();
    }
}
