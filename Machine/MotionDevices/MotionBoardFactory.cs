using System;
using Microsoft.Extensions.DependencyInjection;

namespace MachineClassLibrary.Machine.MotionDevices
{
    public class MotionBoardFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly BoardConfiguration _boardConfiguration;

        public MotionBoardFactory(IServiceProvider serviceProvider, BoardConfiguration machineConfiguration)
        {
            _serviceProvider = serviceProvider;
            _boardConfiguration = machineConfiguration;
        }
        public IMotionDevicePCI1240U GetMotionBoard()
        {
            if (_boardConfiguration.IsPCI1240U) return _serviceProvider.GetService<MotionDevicePCI1240U>() ?? throw new NullReferenceException("Getting service MotionDevicePCI1240U returned null");
            if (_boardConfiguration.IsPCI1245E) return _serviceProvider.GetService<MotionDevicePCI1245E>() ?? throw new NullReferenceException("Getting service MotionDevicePCI1245E returned null");
            //if (_boardConfiguration.IsPCIE1245) return _serviceProvider.GetService<MotionDevicePCIE1245>() ?? throw new NullReferenceException("Getting service MotionDevicePCI1245E returned null");
            if (_boardConfiguration.IsMOCKBOARD) return _serviceProvider.GetService<MotDevMock>() ?? throw new NullReferenceException("Getting service MotDevMock returned null");
            throw new ArgumentException($"The motion board isn't defined.");
        }
    }
}
