using MachineClassLibrary.Classes;
using MachineClassLibrary.Machine.MotionDevices;
using MachineClassLibrary.SFC;
using MachineClassLibrary.VideoCapture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MachineClassLibrary.Machine.Machines
{
    public class DicingBladeMachine : PCI124XXMachine, IHasCamera, IHasSCF, IHasValves, IHasSensors, IDisposable, IHasPlaces<Place>
    {

        private readonly ISpindle _spindle;
        private readonly IVideoCapture _videoCamera;
        private Dictionary<Valves, (Ax axis, Do dOut)> _valves;
        private Dictionary<Sensors, (Ax axis, Di dIn, bool invertion, string name)> _sensors;
        private Dictionary<Place, (Ax axis, double pos)[]> _places;
        private Dictionary<Place, double> _singlePlaces;
        public DicingBladeMachine(ExceptionsAgregator exceptionsAgregator, MotionDevicePCI1240U motionDevice, IVideoCapture usbVideoCamera, ISpindle spindle) : base(exceptionsAgregator, motionDevice)
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
                _motionDevice.HomeMovingAsync(arr);
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
        public void ConfigureSensors(Dictionary<Sensors, (Ax, Di, bool, string)> sensors)
        {
            _sensors = new Dictionary<Sensors, (Ax, Di, bool, string)>(sensors);
        }

        public void ConfigureValves(Dictionary<Valves, (Ax, Do)> valves)
        {
            _valves = new Dictionary<Valves, (Ax, Do)>(valves);
        }

        public void SetBridgeOnSensors(Sensors sensor, bool setBridge)
        {
            var num = _axes[_sensors[sensor].axis].AxisNum;
            _motionDevice.SetBridgeOnAxisDin(num, (int)_sensors[sensor].dIn, setBridge);
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

        public void SetSpindleFreq(int frequency)
        {
            _spindle.SetSpeed((ushort)frequency);
        }

        public void StartSpindle(params Sensors[] blockers)
        {
            _spindleBlockers = new(blockers);
            foreach (var blocker in blockers)
            {
                var axis = _axes[_sensors[blocker].axis];
                var di = _sensors[blocker].dIn;
                if (!axis.GetDi(di) ^ _sensors[blocker].invertion)
                {
                    throw new MachineException($"Отсутствует {_sensors[blocker].name}");
                }
            }

            _spindle.Start();
        }

        private List<Sensors> _spindleBlockers;

        public Dictionary<int, (string, string[])> AvaliableVideoCaptureDevices => _videoCamera.AvaliableVideoCaptureDevices;

        public bool IsVideoCaptureConnected => _videoCamera.IsVideoCaptureConnected;

        public string VideoCaptureMessage => _videoCamera.VideoCaptureMessage;

        public void StopSpindle()
        {
            _spindle.Stop();
        }

        private void _spindle_GetSpindleState(object obj, SpindleEventArgs e)
        {
            OnSpindleStateChanging?.Invoke(null, e);
        }
        public void Dispose()
        {
            _spindle.Dispose();
        }

        public void StartCamera(int ind, int capabilitiesInd = 0)
        {
            _videoCamera.StartCamera(ind, capabilitiesInd);
        }

        public void FreezeCameraImage()
        {
            _videoCamera.FreezeCameraImage();
        }

        public void StopCamera()
        {
            _videoCamera.StopCamera();
        }

        public int GetVideoCaptureDevicesCount() => _videoCamera.GetVideoCaptureDevicesCount();


        public int GetVideoCapabilitiesCount() => _videoCamera.GetVideoCapabilitiesCount();


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