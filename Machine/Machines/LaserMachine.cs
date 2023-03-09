using MachineClassLibrary.Classes;
using MachineClassLibrary.Laser;
using MachineClassLibrary.Laser.Parameters;
using MachineClassLibrary.Machine.MotionDevices;
using MachineClassLibrary.VideoCapture;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace MachineClassLibrary.Machine.Machines
{
    public class LaserMachine : PCI124XXMachine, IHasCamera, IMarkLaser, IHasPlaces<LMPlace>, IHasValves
    {
        private readonly IMarkLaser _markLaser;
        private readonly IVideoCapture _videoCapture;
        private Dictionary<LMPlace, (Ax axis, double pos)[]> _places;
        private Dictionary<LMPlace, double> _singlePlaces;
        private Dictionary<Valves, (Ax, Do)> _valves;

        public event EventHandler<VideoCaptureEventArgs> OnBitmapChanged;
        public event EventHandler<ValveEventArgs> OnValveStateChanged;

        public LaserMachine(ExceptionsAgregator exceptionsAgregator, IMotionDevicePCI1240U motionDevice, IMarkLaser markLaser, IVideoCapture videoCapture) : base(exceptionsAgregator, motionDevice)
        {
            Guard.IsNotNull(markLaser, nameof(markLaser));
            Guard.IsNotNull(videoCapture, nameof(videoCapture));
            _markLaser = markLaser;
            _videoCapture = videoCapture;
            _videoCapture.OnBitmapChanged += _videoCapture_OnBitmapChanged;
        }
        public void ConfigureGeometry(Dictionary<LMPlace, (Ax, double)[]> places)
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
                _places = new Dictionary<LMPlace, (Ax, double)[]>(places);
        }
        public async Task GoThereAsync(LMPlace place, bool precisely = false)
        {
            if (place != LMPlace.Home)
            {
                if (precisely)
                {
                    var ax = new (int, double, double)[_places[place].Length];
                    for (var i = 0; i < _places[place].Length; i++)
                    {
                        var axis = _places[place][i].axis;
                        ax[i] = (_axes[axis].AxisNum, _places[place][i].pos, _axes[axis].LineCoefficient);
                    }

                    await _motionDevice.MoveAxesByCoorsPrecAsync(ax);
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

                    var tasks = new List<Task>(ax.Length);

                    tasks = _places[place].Select(p => WaitUntilAxisStopAsync(p.axis)).ToList();


                    //foreach (var item in _places[place])
                    //{
                    //    tasks.Add(WaitUntilAxisStopAsync(item.axis));
                    //}
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
            }
            else
            {
                var arr = new (int, double, uint)[]
                {
                    (_axes[Ax.X].AxisNum, _velRegimes[Ax.X][Velocity.Service], 5),
                    (_axes[Ax.Y].AxisNum, _velRegimes[Ax.Y][Velocity.Service], 5),
                    (_axes[Ax.Z].AxisNum, _velRegimes[Ax.Z][Velocity.Service], 1)
                };
                
                await _motionDevice.HomeMovingAsync(arr).ConfigureAwait(false);

                foreach (var axis in _axes.Keys) _motionDevice.ResetAxisCounter(_axes[axis].AxisNum);                
                
            }
        }
        public async Task MoveGpInPlaceAsync(Groups group, LMPlace place, bool precisely = false)
        {
            await MoveGpInPosAsync(group, _places[place].Select(p => p.pos).ToArray(), precisely);
        }

        public async Task MoveAxesInPlaceAsync(LMPlace place)
        {
            foreach (var axpos in _places[place]) await MoveAxInPosAsync(axpos.axis, axpos.pos);
        }

        /// <summary>
        ///     Actual coordinates translation
        /// </summary>
        /// <param name="place"></param>
        /// <returns>Axes actual coordinates - place coordinates relatively</returns>
        public (Ax, double)[] TranslateActualCoors(LMPlace place)
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

        public double TranslateActualCoors(LMPlace place, Ax axis)
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
        public (Ax, double)[] TranslateActualCoors(LMPlace place, (Ax axis, double pos)[] position)
        {
            var temp = new List<(Ax, double)>();
            foreach (var p in position)
                if (_places[place].Select(a => a.axis).Contains(p.axis))
                    temp.Add((p.axis, _places[place].GetVal(p.axis) - p.pos));
            var arr = temp.ToArray();
            return arr;
        }

        public double TranslateSpecCoor(LMPlace place, double position, Ax axis)
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
        public void ConfigureGeometry(Dictionary<LMPlace, double> places)
        {
            _singlePlaces = new Dictionary<LMPlace, double>(places);
        }

        public double GetGeometry(LMPlace place, int arrNum)
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

        public double GetGeometry(LMPlace place, Ax axis)
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
        private void _videoCapture_OnBitmapChanged(object sender, VideoCaptureEventArgs eventArgs)
        {
            var image = _videoCapture.IsVideoCaptureConnected ? eventArgs.Image : null;
            var args = new VideoCaptureEventArgs(image, _videoCapture.VideoCaptureMessage);
            OnBitmapChanged?.Invoke(this, args);
        }

        public bool IsMarkDeviceInit => _markLaser.IsMarkDeviceInit;

        public Dictionary<int, (string, string[])> AvaliableVideoCaptureDevices => _videoCapture.AvaliableVideoCaptureDevices;

        public bool IsVideoCaptureConnected => _videoCapture.IsVideoCaptureConnected;

        public string VideoCaptureMessage => _videoCapture.VideoCaptureMessage;

        //public event EventHandler<BitmapEventArgs> OnVideoSourceBmpChanged;

        public void CloseMarkDevice()
        {
            _markLaser.CloseMarkDevice();
        }

        //public void FreezeVideoCapture()
        //{
        //    _videoCapture.FreezeCameraImage();
        //}

        public async Task<bool> InitMarkDevice(string initDirPath)
        {
            return await _markLaser.InitMarkDevice(initDirPath);
        }

        public async Task<bool> PierceObjectAsync(IPerforating perforator)
        {
            return await _markLaser.PierceObjectAsync(perforator);
        }

        public void SetMarkDeviceParams()
        {
            _markLaser.SetMarkDeviceParams();
        }

        public async Task<bool> PiercePointAsync(double x = 0, double y = 0)
        {
            return await _markLaser.PiercePointAsync(x, y);
        }

        public async Task<bool> PierceLineAsync(double x1, double y1, double x2, double y2)
        {
            return await _markLaser.PierceLineAsync(x1, y1, x2, y2);
        }

        public void StartCamera(int ind, int capabilitiesInd = 0) => _videoCapture.StartCamera(ind, capabilitiesInd);

        public void FreezeCameraImage() => _videoCapture.FreezeCameraImage();

        public void StopCamera() => _videoCapture.StopCamera();

        public int GetVideoCaptureDevicesCount() => _videoCapture.GetVideoCaptureDevicesCount();

        public int GetVideoCapabilitiesCount() => _videoCapture.GetVideoCapabilitiesCount();

        public Task<bool> PierceCircleAsync(double diameter)
        {
            return _markLaser.PierceCircleAsync(diameter);
        }
        
        public void SetMarkParams(MarkLaserParams markLaserParams)
        {
            _markLaser.SetMarkParams(markLaserParams);
        }
        
        public void SetExtMarkParams(ExtParamsAdapter paramsAdapter)
        {
            _markLaser.SetExtMarkParams(paramsAdapter);
        }

        public Task<bool> PierceDxfObjectAsync(string filePath)
        {
            return _markLaser.PierceDxfObjectAsync(filePath);
        }

        public async Task<bool> CancelMarkingAsync()
        {
            return await _markLaser?.CancelMarkingAsync();
        }

        public async Task<bool> MarkTextAsync(string text, double textSize, double angle) => await _markLaser.MarkTextAsync(text, textSize, angle);

        public void ConfigureValves(Dictionary<Valves, (Ax, Do)> valves)
        {
            _valves = valves;
        }

        public void SwitchOnValve(Valves valve)
        {
            var axisNum = _axes[_valves[valve].Item1].AxisNum;
            var dOut = _valves[valve].Item2;
            
            _motionDevice.SetAxisDout(axisNum, (ushort)dOut, true);
        }

        public void SwitchOffValve(Valves valve)
        {
            var axisNum = _axes[_valves[valve].Item1].AxisNum;
            var dOut = _valves[valve].Item2;

            _motionDevice.SetAxisDout(axisNum, (ushort)dOut, false);
        }

        public bool GetValveState(Valves valve)
        {
            var axisNum = _axes[_valves[valve].Item1].AxisNum;
            var dOut = _valves[valve].Item2;
            return _motionDevice.GetAxisDout(axisNum, (ushort)dOut);
        }

        public IGeometryBuilder<LMPlace> ConfigureGeometryFor(LMPlace place)
        {
            _places ??= new();
            return new GeometryBuilder<LMPlace>(place, ref _places);
        }

        public void InvokeSettinds()
        {
            _videoCapture?.InvokeSettinds();
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



        //public void StartVideoCapture(int ind, int capabilitiesInd = 0) => _videoCapture.StartCamera(ind, capabilitiesInd);

        //public void StopVideoCapture() => _videoCapture.StopCamera();
        //public int GetCamerasCount() => _videoCapture.GetVideoCaptureDevicesCount();
        //public int GetCameraCapabilitiesCount() => _videoCapture.GetVideoCapabilitiesCount();
    }
}
