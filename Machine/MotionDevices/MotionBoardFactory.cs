using System;
using Microsoft.Extensions.DependencyInjection;
using NewLaserProject.Classes;

namespace MachineClassLibrary.Machine.MotionDevices
{
    public class MotionBoardFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MachineConfiguration _machineConfiguration;

        public MotionBoardFactory(IServiceProvider serviceProvider, MachineConfiguration machineConfiguration)
        {
            _serviceProvider = serviceProvider;
            _machineConfiguration = machineConfiguration;
        }
        public IMotionDevicePCI1240U GetMotionBoard()
        {
            if (_machineConfiguration.IsPCI1240U) return _serviceProvider.GetService<MotionDevicePCI1240U>() ?? throw new NullReferenceException("Getting service MotionDevicePCI1240U returned null");
            if (_machineConfiguration.IsPCI1245E) return _serviceProvider.GetService<MotionDevicePCI1245E>() ?? throw new NullReferenceException("Getting service MotionDevicePCI1245E returned null");
            if (_machineConfiguration.IsMOCKBOARD) return _serviceProvider.GetService<MotDevMock>() ?? throw new NullReferenceException("Getting service MotDevMock returned null");
            throw new ArgumentException($"The motion board isn't defined.");
        }
    }
}
