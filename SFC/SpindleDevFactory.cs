using System;
using MachineClassLibrary.Machine;
using Microsoft.Extensions.DependencyInjection;

namespace MachineClassLibrary.SFC;

public class SpindleDevFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DicingMachineConfiguration _configuration;

    public SpindleDevFactory(IServiceProvider serviceProvider, DicingMachineConfiguration machineConfiguration)
    {
        _serviceProvider = serviceProvider;
        _configuration = machineConfiguration;
    }
    public ISpindle GetSpindle()
    {
        if (_configuration.IsMD520) return _serviceProvider.GetService<MD520>() ?? throw new NullReferenceException("Getting service MD520 returned null");
        if (_configuration.IsSpindle3) return _serviceProvider.GetService<Spindle3>() ?? throw new NullReferenceException("Getting service Spindle3 returned null");
        if (_configuration.IsMockSpindle) return _serviceProvider.GetService<MockSpindle>() ?? throw new NullReferenceException("Getting service MockSpindle returned null");
        throw new ArgumentException($"The spindle isn't defined.");
    }
}
