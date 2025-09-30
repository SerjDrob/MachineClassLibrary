using System;
using System.Threading.Tasks;
using MachineClassLibrary.Miscellaneous;
using Microsoft.Extensions.Logging;


namespace MachineClassLibrary.SFC;

public class MD520Q : SpindleBase<MD520Q>
{
    private const ushort READ_AC_DRIVE_STATE_1 = 0x3000;

    private const ushort READ_OUTPUT_CURRENT = 0x1004;
    private const ushort READ_RUNNING_FREQ = 0x1001;
    private const ushort READ_AC_DRIVE_STATE_2 = 0x7044;
    private const ushort READ_AC_DRIVE_FAULT = 0x8000;

    private const ushort WRITE_CONTROL_COMMAND_AC_DRIVE_2 = 0x2000;
    private const ushort WRITE_FREQ_REF_SET_2 = 0x7310;

    private const ushort STATE_1_RUNNING_FORWARD = 0x0001;
    private const ushort STATE_1_RUNNING_REVERSE = 0x0002;
    private const ushort STATE_1_STOPPED = 0x0003;
    private const ushort COMMAND_AC_DRIVE_2_RUN_FORWARD = 0x0001;
    private const ushort COMMAND_AC_DRIVE_2_DEC_STOP = 0x0006;
    public MD520Q(SerialPortSettings serialPortSettings, ILogger<MD520Q> logger) : base(serialPortSettings, logger)
    {
    }

    protected override async Task<bool> CheckIfSpindleSpinningAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, READ_AC_DRIVE_STATE_1, 1);//.ConfigureAwait(false);//when timeout throw an exception
        return data[0] == STATE_1_RUNNING_FORWARD | data[0] == STATE_1_RUNNING_REVERSE;
    }

    protected override async Task<int> GetCurrentAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, READ_OUTPUT_CURRENT, 1);//.ConfigureAwait(false);
        return data[0];
    }

    protected override async Task<int> GetFrequencyAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, READ_RUNNING_FREQ, 1);//.ConfigureAwait(false);
        return data[0];
    }

    protected override async Task<SpinStatus> GetStatusAsync()
    {
        ushort[] data;
        bool acc;
        bool dec;
        bool stop;
        int current;
        var CheckData = (ushort data, int bitNum) => (data & 1 << bitNum) != 0;
        data = await _client.ReadHoldingRegistersAsync(1, READ_AC_DRIVE_STATE_2, 1);//.ConfigureAwait(false);
        var onFreq = CheckData(data[0], 3);
        acc = onFreq ? false : data[0] > _freq;
        dec = onFreq ? false : data[0] < _freq;
        stop = data[0] == STATE_1_STOPPED;
        data = await _client.ReadHoldingRegistersAsync(1, READ_OUTPUT_CURRENT, 1);//.ConfigureAwait(false);
        current = data[0];

        return new SpinStatus(onFreq, acc, dec, stop);
    }

    protected override async Task StartFWDCommandAsync() => await _client.WriteSingleRegisterAsync(1, WRITE_CONTROL_COMMAND_AC_DRIVE_2, COMMAND_AC_DRIVE_2_RUN_FORWARD);//.ConfigureAwait(false);

    protected override async Task StopCommandAsync() => await _client.WriteSingleRegisterAsync(1, WRITE_CONTROL_COMMAND_AC_DRIVE_2, COMMAND_AC_DRIVE_2_DEC_STOP);//.ConfigureAwait(false);

    protected override async Task WriteRPMAsync(ushort rpm) => await _client.WriteSingleRegisterAsync(1, WRITE_FREQ_REF_SET_2, rpm);//.ConfigureAwait(false);

    protected override Task WriteSettingsAsync() => Task.CompletedTask;

    protected override async Task<SpinFault> GetSpinFaultAsync()
    {
        var data = await _client.ReadHoldingRegistersAsync(1, READ_AC_DRIVE_FAULT, 1);//.ConfigureAwait(false);

        if (Enum.IsDefined(typeof(MD520FaultCode), data[0]) && _tempFault != data[0])
        {
            _tempFault = data[0];
            var fault = (MD520FaultCode)_tempFault;
            if (fault == MD520FaultCode.None) return await base.GetSpinFaultAsync();//.ConfigureAwait(false);
            string faultDescription = fault.GetDescription();

            _logger.LogError(
                "Spindle fault detected! Code: {FaultCode}, Description: {Description}",
                fault,
                faultDescription
            );

            var spinFault = new SpinFault(
            false,
            (int)fault,
            faultDescription);

            return spinFault;
        }
        else
        {
            return await base.GetSpinFaultAsync();//.ConfigureAwait(false);
        }
    }
}
