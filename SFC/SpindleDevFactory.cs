using System;
using MachineClassLibrary.Machine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        //if (_configuration.IsMD520) return _serviceProvider.GetService<MD520>() ?? throw new NullReferenceException("Getting service MD520 returned null");
        //if (_configuration.IsSpindle3) return _serviceProvider.GetService<Spindle3>() ?? throw new NullReferenceException("Getting service Spindle3 returned null");
        //if (_configuration.IsCommanderSK) return _serviceProvider.GetService<CommanderSK>() ?? throw new NullReferenceException("Getting service MockSpindle returned null");
        //if (_configuration.IsMockSpindle) return _serviceProvider.GetService<MockSpindle>() ?? throw new NullReferenceException("Getting service MockSpindle returned null");
        //throw new ArgumentException($"The spindle isn't defined.");
        
        if (_configuration.IsMD520) 
            return new MD520Q(_configuration.SpindlePort, _configuration.SpindleBaudRate, _serviceProvider.GetRequiredService<ILogger<MD520Q>>()) ?? throw new NullReferenceException("Getting service MD520 returned null");
        if (_configuration.IsSpindle3) 
            return new Sunfar(_configuration.SpindlePort, _configuration.SpindleBaudRate, _serviceProvider.GetRequiredService<ILogger<Sunfar>>()) ?? throw new NullReferenceException("Getting service Spindle3 returned null");
        if (_configuration.IsCommanderSK) 
            return new CommanderSK(_configuration.SpindlePort, _configuration.SpindleBaudRate, _serviceProvider.GetRequiredService<ILogger<CommanderSK>>(), new SpindleParams
            {
                Acc = 5,
                Dec = 5,
                MinFreq = 100,
                MaxFreq = 600,
                RatedCurrent = 9,
                RatedVoltage = 60
            }) ?? throw new NullReferenceException("Getting service MockSpindle returned null");
        if (_configuration.IsMockSpindle) 
            return new MockSpindle(_configuration.SpindlePort, _configuration.SpindleBaudRate, _serviceProvider.GetRequiredService<ILogger<MockSpindle>>()) ?? throw new NullReferenceException("Getting service MockSpindle returned null");
        throw new ArgumentException($"The spindle isn't defined.");

    }
}
