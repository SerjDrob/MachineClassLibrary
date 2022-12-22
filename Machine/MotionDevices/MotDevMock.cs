using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MachineClassLibrary.Machine.MotionDevices
{
    public class MotDevMock : IMotionDevicePCI1240U
    {
        private struct AxisFeatures
        {
            public double AxVel { get; set; }
            public MotionDeviceConfigs DevConfig { get; set; }
            public double AxLength { get; set; }
            public double PositionTarget { get; set; }
            public bool IsPosTargetSet { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; }
        }

        private AxisFeatures[] _axisFeatures;

        private MockAxisState[] _axisStates;

        private const double MOVE_RESOLUTION = 0.001;

        public int AxisCount { get; private set; }
        private List<int[]> _axGroups;

        public event Action<string, int> ThrowMessage;
        public event EventHandler<AxNumEventArgs> TransmitAxState;

        public bool DevicesConnection()
        {
            AxisCount = 4;
            _axisStates = new MockAxisState[AxisCount];
            _axisFeatures = new AxisFeatures[AxisCount];
            for (int i = 0; i < AxisCount; i++)
            {
                _axisFeatures[i].AxLength = 50;
            }
            _axGroups = new();
            return true;
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public int FormAxesGroup(int[] axisNums)
        {
            _axGroups.Add(axisNums);
            return _axGroups.IndexOf(axisNums);
        }

        public bool GetAxisDout(int axisNum, ushort dOut)
        {
            throw new NotImplementedException();
        }

        public Task HomeMovingAsync((AxDir direction, HomeRst homeRst, HmMode homeMode, double velocity, int axisNum)[] axs)
        {
            throw new NotImplementedException();
        }

        public Task HomeMovingAsync((int axisNum, double vel, uint mode)[] axVels)
        {
            throw new NotImplementedException();
        }

        public void MoveAxesByCoorsAsync((int axisNum, double position)[] ax)
        {
            throw new NotImplementedException();
        }

        public Task MoveAxesByCoorsPrecAsync((int axisNum, double position, double lineCoefficient)[] ax)
        {
            throw new NotImplementedException();
        }

        public async Task MoveAxisAsync(int axisNum, double position)
        {
            _axisFeatures[axisNum].PositionTarget = position;
            _axisFeatures[axisNum].IsPosTargetSet = true;
            var dirSign = _axisStates[axisNum].actPos > position ? -1 : 1;
            await MovingAxis(axisNum, dirSign);
        }

        public async void MoveAxisContiniouslyAsync(int axisNum, AxDir dir)
        {
            var dirSign = dir switch { AxDir.Pos => 1, AxDir.Neg => -1 };
            await MovingAxis(axisNum, dirSign);
        }

        private async Task MovingAxis(int axisNum, int dirSign)
        {
            var alterResolution = MOVE_RESOLUTION;
            var delay = MOVE_RESOLUTION * 1e3 / _axisFeatures[axisNum].AxVel;

            if (delay < 16)
            {
                delay = 16;
                alterResolution = delay * _axisFeatures[axisNum].AxVel / 1e3;
            }

            _axisFeatures[axisNum].CancellationTokenSource = new();
            
            if (dirSign > 0) _axisStates[axisNum].pLmt = false;
            if (dirSign < 0) _axisStates[axisNum].nLmt = false;


            try
            {
                await Task.Run(async () =>
                   {
                       var diff = _axisStates[axisNum].actPos;
                       while (!_axisFeatures[axisNum].CancellationTokenSource.Token.IsCancellationRequested)
                       {
                           var act = _axisStates[axisNum].actPos + dirSign * MOVE_RESOLUTION;
                           var coef = _axisFeatures[axisNum].DevConfig.ppu / 1000d;
                           _axisStates[axisNum] = _axisStates[axisNum]/* with { cmdPos = act * coef, actPos = act }*/;
                           if (Math.Abs(act - diff) >= alterResolution)
                           {
                               await Task.Delay(TimeSpan.FromMilliseconds(delay));
                               diff = act;
                           }
                       }
                   }, _axisFeatures[axisNum].CancellationTokenSource.Token);
            }
            catch (Exception)
            {
                _axisFeatures[axisNum].CancellationTokenSource.Dispose();
            }
        }


        public Task MoveAxisPreciselyAsync(int axisNum, double lineCoefficient, double position, int rec = 0)
        {
            throw new NotImplementedException();
        }

        public Task MoveGroupAsync(int groupNum, double[] position)
        {
            throw new NotImplementedException();
        }

        public Task MoveGroupPreciselyAsync(int groupNum, double[] position, (int axisNum, double lineCoefficient)[] gpAxes)
        {
            throw new NotImplementedException();
        }

        public void ResetAxisCounter(int axisNum)
        {
            throw new NotImplementedException();
        }

        public void ResetErrors(int axisNum = 888)
        {
            //throw new NotImplementedException();
        }

        public void SetAxisConfig(int axisNum, MotionDeviceConfigs configs)
        {
            _axisFeatures[axisNum].DevConfig = configs;
        }

        public void SetAxisDout(int axisNum, ushort dOut, bool val)
        {
            //throw new NotImplementedException();
        }

        public void SetAxisVelocity(int axisNum, double vel)
        {
            _axisFeatures[axisNum].AxVel = vel;
        }

        public void SetBridgeOnAxisDin(int axisNum, int bitNum, bool setReset)
        {
           // throw new NotImplementedException();
        }

        public void SetGroupConfig(int gpNum, MotionDeviceConfigs configs)
        {
            throw new NotImplementedException();
        }

        public void SetGroupVelocity(int groupNum)
        {
            throw new NotImplementedException();
        }

        public void SetGroupVelocity(int groupNum, double velocity)
        {
            throw new NotImplementedException();
        }

        public Task StartMonitoringAsync()
        {
            return DeviceStateMonitorAsync();
        }

        public void StopAxis(int axisNum)
        {
            _axisFeatures[axisNum].CancellationTokenSource.Cancel();
        }

        private async Task DeviceStateMonitorAsync()
        {
            var num = 0;
            await Task.Run(()=>
            {
                while (true)
                {
                    if (_axisStates[num].cmdPos >= _axisFeatures[num].AxLength && !_axisStates[num].pLmt)
                    {
                        _axisFeatures[num].CancellationTokenSource?.Cancel();
                        _axisStates[num].pLmt = true; 
                    }
                    else if(_axisStates[num].cmdPos <= 0 && !_axisStates[num].nLmt)
                    {
                        _axisFeatures[num].CancellationTokenSource?.Cancel();
                        _axisStates[num].nLmt = true;
                    }
                    else if(_axisStates[num].cmdPos > 0 && _axisStates[num].cmdPos < _axisFeatures[num].AxLength)
                    {
                        _axisStates[num].pLmt = false;
                        _axisStates[num].nLmt = false;
                    }

                    TransmitAxState?.Invoke(this, new AxNumEventArgs(num, _axisStates[num].GetAxisState));
                    num++;
                    if (num == AxisCount) num = 0;
                }
            });
        }
    }
}
