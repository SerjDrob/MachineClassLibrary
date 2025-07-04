﻿using System;
using System.Threading.Tasks;
using System.Windows.Documents;
using MachineClassLibrary.Classes;

namespace MachineClassLibrary.Machine.MotionDevices
{


    public interface IMotionDevicePCI1240U:IDisposable
    {
        int AxisCount { get; }

        event EventHandler<AxNumEventArgs> TransmitAxState;
        double GetAxActual(int axNum);
        Task<bool> DevicesConnection();
        void Dispose();
        int FormAxesGroup(int[] axisNums);
        bool GetAxisDout(int axisNum, ushort dOut);
        Task HomeMovingAsync((AxDir direction, HomeRst homeRst, HmMode homeMode, double velocity, int axisNum)[] axs);
        Task HomeMovingAsync((int axisNum, double vel, uint mode)[] axVels);
        Task MoveAxesByCoorsAsync((int axisNum, double position)[] ax);
        Task MoveAxesByCoorsPrecAsync((int axisNum, double position, double lineCoefficient)[] ax);
        Task MoveAxisAsync(int axisNum, double position);
        void MoveAxisContiniouslyAsync(int axisNum, AxDir dir);
        Task<double> MoveAxisPreciselyAsync(int axisNum, double lineCoefficient, double position, int rec = 0);
        Task<double> MoveAxisPreciselyAsync_2(int axisNum, double lineCoefficient, double position, int rec = 0);
        Task MoveGroupAsync(int groupNum, double[] position);
        Task MoveGroupPreciselyAsync(int groupNum, double[] position, (int axisNum, double lineCoefficient)[] gpAxes);
        void ResetAxisCounter(int axisNum);
        void ResetErrors(int axisNum = 888);
        void SetAxisConfig(int axisNum, MotionDeviceConfigs configs);
        void SetAxisDout(int axisNum, ushort dOut, bool val);
        void SetAxisVelocity(int axisNum, double vel);
        void SetBridgeOnAxisDin(int axisNum, int bitNum, bool setReset);
        void SetGroupConfig(int gpNum, MotionDeviceConfigs configs);
        void SetGroupVelocity(int groupNum);
        void SetGroupVelocity(int groupNum, double velocity);
        /// <summary>
        /// Setting tollerance for a precision motion of axes
        /// </summary>
        /// <param name="tolerance">measured in mm</param>
        void SetPrecision(double tolerance);
        Task StartMonitoringAsync();
        void StopAxis(int axisNum);
        double GetAxCmd(int axNum);
        void SetAxisCoordinate(int axisNum, double coordinate);
        bool GetAxisReady(int axisNum);
        void CloseDevice();
    }
}
