using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MachineClassLibrary.SFC;

public class CommanderSK : SpindleBase<CommanderSK>
{
    private readonly SpindleParams _spindleParams;

    public CommanderSK(SerialPortSettings serialPortSettings, ILogger<CommanderSK> logger, SpindleParams spindleParams) : base(serialPortSettings, logger)
    {
        _spindleParams = spindleParams;
    }

    protected override async Task<bool> CheckIfSpindleSpinningAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, 0x43E9, 2).ConfigureAwait(false);//Pr10.02
        return data[1] != 0;
    }

    protected override async Task<int> GetCurrentAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, 0x4190, 2).ConfigureAwait(false);
        return data[1];
    }

    protected override async Task<int> GetFrequencyAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, 0x41F4, 2).ConfigureAwait(false);//Pr5.01
        return data[1];
    }

    protected override async Task<SpinStatus> GetStatusAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, 0x43ED, 2).ConfigureAwait(false);
        var onFreq = data[1] == 1;
        data = await _client.ReadHoldingRegistersAsync(1, 0x43EA, 2).ConfigureAwait(false);
        var stop = data[1] == 1;
        var acc = !stop && !onFreq;
        var dec = !stop && !onFreq;
        return new SpinStatus(onFreq, acc, dec, stop);
    }

    protected override async Task StartFWDCommandAsync() => await _client.WriteMultipleRegistersAsync(1, 0x4279, [0, 1]).ConfigureAwait(false);//Pr6.34

    protected override async Task StopCommandAsync() => await _client.WriteMultipleRegistersAsync(1, 0x4279, [0, 0]).ConfigureAwait(false);//Pr6.34

    protected override async Task WriteRPMAsync(ushort rpm)
    {
        var high = rpm;
        var low = (ushort)(rpm - 5);
        await _client.WriteMultipleRegistersAsync(1, 0x4069,
            [0, high, 0, low]).ConfigureAwait(false);//Pr01.06 - high limit of speed, Pr01.07 - low limit of speed
    }

    protected override async Task WriteSettingsAsync()
    {
        await _client.WriteMultipleRegistersAsync(1, 0x40D2, [0, (ushort)(10 * _spindleParams.Acc)]).ConfigureAwait(false);//Pr2.11
        await _client.WriteMultipleRegistersAsync(1, 0x40DC, [0, (ushort)(10 * _spindleParams.Dec)]).ConfigureAwait(false);//Pr2.21
        await _client.WriteMultipleRegistersAsync(1, 0x41FC, [0, _spindleParams.RatedVoltage]).ConfigureAwait(false);//Pr5.09
        await _client.WriteMultipleRegistersAsync(1, 0x41FA, [0, (ushort)(100 * _spindleParams.RatedCurrent)]).ConfigureAwait(false);//Pr5.07
    }
}
