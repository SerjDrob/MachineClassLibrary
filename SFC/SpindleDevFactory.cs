using System;
using MachineClassLibrary.Machine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MachineClassLibrary.SFC;

public class SpindleDevFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DicingMachineConfiguration _configuration;
    private readonly SerialPortSettings _serialPortSettings;

    public SpindleDevFactory(IServiceProvider serviceProvider, DicingMachineConfiguration machineConfiguration)
    {
        _serviceProvider = serviceProvider;
        _configuration = machineConfiguration;
        _serialPortSettings = new()
        {
            PortName = machineConfiguration.SpindlePort,
            BaudRate = machineConfiguration.SpindleBaudRate
        };
        if (machineConfiguration.IsSpindle3) _serialPortSettings.Parity = System.IO.Ports.Parity.Even;
        if (machineConfiguration.IsCommanderSK) _serialPortSettings.ReadTimeout = 500;
    }
    public ISpindle GetSpindle()
    {
        if (_configuration.IsMD520) 
            return new MD520Q(_serialPortSettings, _serviceProvider.GetRequiredService<ILogger<MD520Q>>()) ?? throw new NullReferenceException("Getting service MD520 returned null");
        if (_configuration.IsSpindle3) 
            return new Sunfar(_serialPortSettings, _serviceProvider.GetRequiredService<ILogger<Sunfar>>()) ?? throw new NullReferenceException("Getting service Spindle3 returned null");
        if (_configuration.IsCommanderSK) 
            return new CommanderSK(_serialPortSettings, _serviceProvider.GetRequiredService<ILogger<CommanderSK>>(), new SpindleParams
            {
                Acc = 5,
                Dec = 5,
                MinFreq = 100,
                MaxFreq = 600,
                RatedCurrent = 9,
                RatedVoltage = 60
            }) ?? throw new NullReferenceException("Getting service MockSpindle returned null");
        if (_configuration.IsMockSpindle) 
            return new MockSpindle(_serialPortSettings, _serviceProvider.GetRequiredService<ILogger<MockSpindle>>()) ?? throw new NullReferenceException("Getting service MockSpindle returned null");
        throw new ArgumentException($"The spindle isn't defined.");
    }
}
