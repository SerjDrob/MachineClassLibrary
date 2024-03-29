﻿using System;
using MachineClassLibrary.Machine;
using Microsoft.Extensions.DependencyInjection;

namespace MachineClassLibrary.Laser.Markers
{
    public class LaserBoardFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly LaserMachineConfiguration _machineConfiguration;

        public LaserBoardFactory(IServiceProvider serviceProvider, LaserMachineConfiguration machineConfiguration)
        {
            _serviceProvider = serviceProvider;
            _machineConfiguration = machineConfiguration;
        }
        public IMarkLaser GetLaserBoard()
        {
            if (_machineConfiguration.IsUF) return _serviceProvider.GetService<JCZLaser>() ?? throw new NullReferenceException("Cannot get service JCZLaser");
            if (_machineConfiguration.IsIR) return _serviceProvider.GetService<JCZLaser>() ?? throw new NullReferenceException("Cannot get service JCZLaser");
            if (_machineConfiguration.IsLaserMock) return _serviceProvider.GetService<MockLaser>() ?? throw new NullReferenceException("Cannot get service MockLaser");
            throw new ArgumentException("Cannot get laser board");
        }

        public IPWM GetPWM()
        {
            if (_machineConfiguration.IsUF) return _serviceProvider.GetService<PWM3>() ?? throw new NullReferenceException("Cannot get service PWM");
            if (_machineConfiguration.IsIR) return _serviceProvider.GetService<PWM2>() ?? throw new NullReferenceException("Cannot get service PWM2");
            throw new ArgumentException("Cannot get PWM");
        }
    }
}
