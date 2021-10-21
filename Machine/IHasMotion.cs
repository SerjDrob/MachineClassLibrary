﻿using MachineClassLibrary.Classes;
using MachineClassLibrary.Machine.MotionDevices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MachineClassLibrary.Machine
{
    public interface IHasMotion
    {
        #region Settings

        public void SetConfigs((Ax axis, MotionDeviceConfigs configs)[] axesConfigs);
        public void SetVelocity(Velocity velocity);
        public void SetAxFeedSpeed(Ax axis, double feed);
        public void ResetErrors(Ax axis);

        #endregion
        public bool MachineInit { get; set; }
        public Velocity VelocityRegime { get; set; }

        public event EventHandler<AxisStateEventArgs> OnAxisMotionStateChanged;
        public double GetGeometry(Place place, int arrNum);
        public double GetGeometry(Place place, Ax axis);
        public double GetAxisSetVelocity(Ax axis);
        public void EmergencyStop();
        public void Stop(Ax axis);
        public Task WaitUntilAxisStopAsync(Ax axis);
        public void GoWhile(Ax axis, AxDir direction);
        public void EmgStop();
        public Task GoThereAsync(Place place, bool precisely = false);
        public Task MoveGpInPosAsync(Groups group, double[] position, bool precisely = false);
        public Task MoveGpInPlaceAsync(Groups group, Place place, bool precisely = false);
        public Task MoveAxInPosAsync(Ax axis, double position, bool precisely = false);
        public Task MoveAxesInPlaceAsync(Place place);
        public (Ax, double)[] TranslateActualCoors(Place place);
        public double TranslateActualCoors(Place place, Ax axis);
        public (Ax, double)[] TranslateActualCoors(Place place, (Ax axis, double pos)[] position);
        public double TranslateSpecCoor(Place place, double position, Ax axis);
        public void EmgScenario( /*DIEventArgs eventArgs*/);
        public void ConfigureAxes((Ax axis, double linecoefficient)[] ax);
        public void ConfigureVelRegimes(Dictionary<Ax, Dictionary<Velocity, double>> velRegimes);
        public void AddGroup(Groups group, IAxis[] axes);
        public void ConfigureGeometry(Dictionary<Place, (Ax, double)[]> places);
        public void ConfigureGeometry(Dictionary<Place, double> places);
        public void ConfigureAxesGroups(Dictionary<Groups, Ax[]> groups);
        public void ConfigureDoubleFeatures(Dictionary<MFeatures, double> doubleFeatures);
        public double GetFeature(MFeatures feature);
    }
}