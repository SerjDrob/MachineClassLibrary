using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MachineClassLibrary.SFC;

public class Sunfar : SpindleBase<Sunfar>
{
    private const ushort READ_BASE_STATE = 0xD000;

    private const ushort STATE_CUR_FREQ = 0x0000;
    private const ushort STATE_CURRENT = 0x0001;

    private const ushort WRITE_COMMAND = 0x1001;
    private const ushort COMMAND_STOP = 0x0003;
    private const ushort COMMAND_START_FWD = 0x0001;

    private const ushort READ_STATUS = 0x2000;
    private const ushort STATUS_ON_FREQ_FWD = 0x0001;
    private const ushort STATUS_ON_FREQ_REV = 0x0002;
    private const ushort STATUS_ACC_FWD = 0x0011;
    private const ushort STATUS_ACC_REV = 0x0012;
    private const ushort STATUS_DEC_FWD = 0x0014;
    private const ushort STATUS_DEC_REV = 0x0015;
    private const ushort STATUS_STOP = 0x0003;

    public Sunfar(string com, int baudRate, ILogger<Sunfar> logger) : base(com, baudRate, logger)
    {
    }

    protected override async Task<bool> CheckIfSpindleSpinningAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, READ_BASE_STATE, 1).ConfigureAwait(false);
        return data[0] != 0;
    }

    protected override async Task WriteRPMAsync(ushort rpm) => await _client.WriteSingleRegisterAsync(1, 0x0001, rpm).ConfigureAwait(false);

    protected override async Task StartFWDCommandAsync() => await _client.WriteSingleRegisterAsync(1, WRITE_COMMAND, COMMAND_START_FWD).ConfigureAwait(false);

    protected override async Task StopCommandAsync() => await _client.WriteSingleRegisterAsync(1, WRITE_COMMAND, COMMAND_STOP).ConfigureAwait(false);

    protected override async Task<int> GetCurrentAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, READ_BASE_STATE + STATE_CURRENT, 1).ConfigureAwait(false);
        return data[0];
    }
    protected override async Task<int> GetFrequencyAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, READ_BASE_STATE + STATE_CUR_FREQ, 1).ConfigureAwait(false);
        return data[0];
    }
    protected override async Task<SpinStatus> GetStatusAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, READ_STATUS, 1).ConfigureAwait(false);
        return new SpinStatus(
            data[0] == STATUS_ON_FREQ_FWD | data[0] == STATUS_ON_FREQ_REV,
            data[0] == STATUS_ACC_FWD | data[0] == STATUS_ACC_REV,
            data[0] == STATUS_DEC_FWD | data[0] == STATUS_DEC_REV,
            data[0] == STATUS_STOP
            );
    }

    protected override async Task WriteSettingsAsync()
    {
        await _client.WriteMultipleRegistersAsync(1, 0x0000,
        [
            0,
                5000,
                2,
                LOW_FREQ_LIMIT, //500,//lower limiting frequency/10
                HIGH_FREQ_LIMIT, //upper limiting frequency/10
                900 //acceleration time/10
        ]).ConfigureAwait(false);

        await _client.WriteMultipleRegistersAsync(1, 0x000B,
        [
            60, //torque boost/10, 0.0 - 20.0%
                5200, //basic running frequency/10
                50 //maximum output voltage 50 - 500V
        ]).ConfigureAwait(false);

        await _client.WriteMultipleRegistersAsync(1, 0x020F,
        [
            4999, //f3/10
                30 //V3
        ]).ConfigureAwait(false);

        await _client.WriteMultipleRegistersAsync(1, 0x020D,
        [
            1200, //f2/10
                20 //V2
        ]).ConfigureAwait(false);

        await _client.WriteMultipleRegistersAsync(1, 0x020B,
        [
            800, //f1/10
                10 //V1
        ]).ConfigureAwait(false);
    }
}
