using MachineClassLibrary.Classes;
using MachineClassLibrary.Machine.Machines;
using MachineClassLibrary.Machine.MotionDevices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MachineClassLibrary.Machine
{
    public interface IHasMotion
    {
        #region Settings

        //public void SetConfigs((Ax axis, MotionDeviceConfigs configs)[] axesConfigs);
        //public IHasMotion SetConfigs(Ax axis, MotionDeviceConfigs configs);
        public void SetGroupConfig(int gpNum, MotionDeviceConfigs configs);
        /// <summary>
        /// Set new velocity regime for the device
        /// </summary>
        /// <param name="velocity"></param>
        /// <returns>The old velocity regime</returns>
        public Velocity SetVelocity(Velocity velocity);
        public void SetAxFeedSpeed(Ax axis, double feed);
        public void ResetErrors(Ax axis);

        #endregion
        public bool IsMotionDeviceInit { get; set; }
        public Velocity VelocityRegime { get; set; }

        public event EventHandler<AxisStateEventArgs> OnAxisMotionStateChanged;
        public double GetAxisSetVelocity(Ax axis);
        public void EmergencyStop();
        public void Stop(Ax axis);
        public void StartMonitoringState();
        public Task WaitUntilAxisStopAsync(Ax axis);
        public void GoWhile(Ax axis, AxDir direction);
        public double GetAxActual(Ax axis);
        public void EmgStop();
        /// <summary>
        /// Setting precision for axes movement
        /// </summary>
        /// <param name="tolerance">measured in mm</param>
        public void SetPrecision(double tolerance);
        public Task MoveGpInPosAsync(Groups group, double[] position, bool precisely = false);
        public Task MoveGpRelativeAsync(Groups group, double[] offset, bool precisely = false);
        public Task MoveAxInPosAsync(Ax axis, double position, bool precisely = false);
        public Task MoveAxRelativeAsync(Ax axis, double diffPosition, bool precisely = false);        
        public void EmgScenario();
        public IHasMotion AddGroup(Groups group, Ax[] axes);
        public void ConfigureAxesGroups(Dictionary<Groups, Ax[]> groups);
        public void ConfigureDoubleFeatures(Dictionary<MFeatures, double> doubleFeatures);
        public double GetFeature(MFeatures feature);
        Task GoHomeAsync();
        IHomingBuilder ConfigureHomingForAxis(Ax axis);
        IAxisBuilder AddAxis(Ax ax, double lineCoefficient);


        void MotionDevInitialized();
    }
}
