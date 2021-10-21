using MachineClassLibrary.Machine.Machines;
using MachineClassLibrary.Machine.MotionDevices;
using MachineClassLibrary.SFC;
using MachineClassLibrary.VideoCapture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MachineClassLibrary.Machine.Machines
{
    public class DicingBladeMachine : PCI124XXMachine, IHasCamera, IHasSCF, IHasValves, IHasSensors, IDisposable
    {

        private readonly ISpindle _spindle;
        private readonly IVideoCapture _videoCamera;
        private Dictionary<Valves, (Ax axis, Do dOut)> _valves;
        private Dictionary<Sensors, (Ax axis, Di dIn, bool invertion, string name)> _sensors;
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

        private void _videoCamera_OnBitmapChanged(BitmapImage bitmapImage)
        {
            OnVideoSourceBmpChanged?.Invoke(this, new BitmapEventArgs(bitmapImage));
        }

        public event EventHandler<ValveEventArgs> OnValveStateChanged;

        public event EventHandler<BitmapEventArgs> OnVideoSourceBmpChanged;

        public event EventHandler<SensorsEventArgs> OnSensorStateChanged;

        public event EventHandler<SpindleEventArgs> OnSpindleStateChanging;

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

        public void StartVideoCapture(int ind)
        {
            _videoCamera.StartCamera(ind);
        }

        public void FreezeVideoCapture()
        {
            _videoCamera.FreezeCameraImage();
        }

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
    }
}