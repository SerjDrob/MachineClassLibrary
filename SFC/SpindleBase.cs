using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NModbus;
//using Modbus.Device;

//using Modbus.Device;
using NModbus.Serial;
using NModbus.SerialPortStream;
using RJCP.IO.Ports;


namespace MachineClassLibrary.SFC;


public abstract class SpindleBase<T> : ISpindle, IDisposable
{
    /// <summary>
    ///     300 Hz = 18000 rpm
    /// </summary>
    public const ushort LOW_FREQ_LIMIT = 3000;

    /// <summary>
    ///     550 Hz = 33000 rpm
    /// </summary>
    public const ushort HIGH_FREQ_LIMIT = 5500;

    private readonly SerialPortSettings _serialPortSettings;
    protected readonly ILogger<T> _logger;

    private readonly object _modbusLock = new();
    //protected ModbusSerialMaster _client;
    protected IModbusMaster _client;
    private SemaphoreSlim _semaphoreSlim = new(1, 1);
    protected int _freq;
    private bool _hasStarted = false;
    private bool _onFreq;
    private SerialPort _serialPort;
    private CancellationTokenSource _watchingStateCancellationTokenSource;
    protected ushort _tempFault;

    // TODO wait o cancel in the end, NEVER forget Tasks
    private Task _watchingStateTask;
    private SerialPortStream _serialPortStream;

    protected SpindleBase(SerialPortSettings serialPortSettings, ILogger<T> logger)
    {
        _serialPortSettings = serialPortSettings;
        _logger = logger;
    }
    public bool IsConnected { get; set; } = false;

    public event EventHandler<SpindleEventArgs> GetSpindleState;

    public async Task<bool> ChangeSpeedAsync(ushort rpm, TimeSpan delay)
    {
        if (Math.Abs(rpm - _freq * 6) < 20) return true;
        await _semaphoreSlim.WaitAsync(/*TimeSpan.FromMilliseconds(300)*/).ConfigureAwait(false);
        try
        {
            await CalculateAndSetSpeedAsync(rpm).ConfigureAwait(false);
            if (!_hasStarted)
            {
                await ClearStartAsync().ConfigureAwait(false);
                await Task.Delay(300).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in ChangeSpeedAsync");
            return false;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
        var cts = new CancellationTokenSource(delay);
        var tcs = new TaskCompletionSource<bool>();

        void ReachedFreq(object sender, SpindleEventArgs args)
        {
            if (args.OnFreq)
            {
                CleanupAndSetResult(true);
            }
            else if (!args.IsOk || args.Stop)
            {
                CleanupAndSetResult(false);
            }
        }

        void CleanupAndSetResult(bool result)
        {
            GetSpindleState -= ReachedFreq;
            cts.Dispose();
            tcs.TrySetResult(result);
        }

        GetSpindleState += ReachedFreq;

        cts.Token.Register(() =>
        {
            GetSpindleState -= ReachedFreq;
            tcs.TrySetResult(false);
        });
        var result = await tcs.Task.ConfigureAwait(false);
        return result;
    }
    /// <summary>
    /// Connects to the spindle
    /// </summary>
    /// <returns>true if success</returns>
    /// <exception cref="SpindleException"></exception>
    public async Task<bool> ConnectAsync()
    {
        if (EstablishConnection())
        {
            _watchingStateCancellationTokenSource = new CancellationTokenSource();
            _watchingStateTask = WatchingStateAsync(_watchingStateCancellationTokenSource.Token);
            var isWorking = await CheckSpindleWorkingAsync().ConfigureAwait(false);
            if (isWorking) return true;
            var paramsIsSet = await SetParamsAsync().ConfigureAwait(false);
            if (!paramsIsSet) throw new SpindleException("SetParams is failed");
        }
        else
        {
            return false;
        }
        return true;
    }

    public void Dispose()
    {
        _watchingStateCancellationTokenSource?.Cancel();
        try
        {
            _watchingStateTask?.Wait(1000);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Spindle state monitoring task did not stop gracefully.");
        }
        _serialPortStream?.Close();
        _serialPortStream?.Dispose();
        _client?.Dispose();
        //_serialPort?.Close();
        //_serialPort?.Dispose();
        _watchingStateCancellationTokenSource?.Dispose();
        _semaphoreSlim?.Dispose();
    }

    public async Task SetSpeedAsync(ushort rpm)
    {
        await _semaphoreSlim.WaitAsync(/*TimeSpan.FromMilliseconds(300)*/).ConfigureAwait(false);
        try
        {
            await CalculateAndSetSpeedAsync(rpm).ConfigureAwait(false);
            _logger.LogInformation($"Successfully set rotation speed: {rpm} rpm");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to set rotation speed: {rpm} rpm");
            throw new SpindleException($"Failed  set rotation speed: {rpm} rpm", ex);
        }
        finally
        {
            _semaphoreSlim?.Release();
        }
    }
    private async Task CalculateAndSetSpeedAsync(ushort rpm)
    {
        if (!(rpm / 6 > LOW_FREQ_LIMIT && rpm / 6 < HIGH_FREQ_LIMIT))
        {
            throw new SpindleException($"{rpm}rpm is out of ({LOW_FREQ_LIMIT * 6},{HIGH_FREQ_LIMIT * 6}) rpm range");
        }
        rpm = (ushort)Math.Abs(rpm / 6);
        await WriteRPMAsync(rpm).ConfigureAwait(false);
    }



    public async Task StartAsync()
    {
        await _semaphoreSlim.WaitAsync(/*TimeSpan.FromMilliseconds(300)*/).ConfigureAwait(false);
        try
        {
            await ClearStartAsync().ConfigureAwait(false);
            _logger.LogInformation($"Successfully wrote started forward command");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to write start forward command");
            throw new SpindleException($"Failed to start fwd", ex);
        }
        finally
        {
            _semaphoreSlim?.Release();
        }
    }

    private async Task ClearStartAsync()
    {
        await StartFWDCommandAsync().ConfigureAwait(false);
        await Task.Delay(3000).ConfigureAwait(false);
        _hasStarted = true;
    }

    public async Task StopAsync()
    {
        await _semaphoreSlim.WaitAsync(/*TimeSpan.FromMilliseconds(300)*/).ConfigureAwait(false);
        try
        {
            await StopCommandAsync().ConfigureAwait(false);
            _logger.LogInformation($"Successfully wrote stop command");
            _hasStarted = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to write stop command");
            throw;
        }
        finally
        {
            _semaphoreSlim?.Release();
        }
    }
    protected record SpinStatus(bool OnFreq, bool Acc, bool Dec, bool Stop);
    protected record SpinFault(bool IsOk, int FaultCode, string FaultDescription);
    protected abstract Task<bool> CheckIfSpindleSpinningAsync();
    protected abstract Task<int> GetCurrentAsync();
    protected abstract Task<int> GetFrequencyAsync();
    protected abstract Task<SpinStatus> GetStatusAsync();
    protected abstract Task StartFWDCommandAsync();
    protected abstract Task StopCommandAsync();
    protected abstract Task WriteRPMAsync(ushort rpm);
    protected abstract Task WriteSettingsAsync();

    protected virtual Task<SpinFault> GetSpinFaultAsync() => Task.FromResult(new SpinFault(true, -1, string.Empty));


    private async Task<bool> CheckSpindleWorkingAsync()
    {
        await _semaphoreSlim.WaitAsync(/*TimeSpan.FromMilliseconds(300)*/).ConfigureAwait(false);
        if (_client == null)
        {
            _logger.LogWarning("Attempted to check spindle state, but Modbus client is not initialized.");
            return false;
        }
        try
        {
            return await CheckIfSpindleSpinningAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to 0xD000");
            return false;
        }
        finally
        {
            _semaphoreSlim?.Release();
        }
    }
    private static RJCP.IO.Ports.Parity ToRJCPParity(System.IO.Ports.Parity parity)
    {
        return parity switch
        {
            System.IO.Ports.Parity.None => RJCP.IO.Ports.Parity.None,
            System.IO.Ports.Parity.Odd => RJCP.IO.Ports.Parity.Odd,
            System.IO.Ports.Parity.Even => RJCP.IO.Ports.Parity.Even,
            System.IO.Ports.Parity.Mark => RJCP.IO.Ports.Parity.Mark,
            System.IO.Ports.Parity.Space => RJCP.IO.Ports.Parity.Space,
            _ => throw new ArgumentOutOfRangeException(nameof(parity), parity, null)
        };
    }
    private static RJCP.IO.Ports.StopBits ToRJCPStopBits(System.IO.Ports.StopBits stopBits)
    {
        return stopBits switch
        {
            System.IO.Ports.StopBits.One => RJCP.IO.Ports.StopBits.One,
            System.IO.Ports.StopBits.OnePointFive => RJCP.IO.Ports.StopBits.One5,
            System.IO.Ports.StopBits.Two => RJCP.IO.Ports.StopBits.Two,
            System.IO.Ports.StopBits.None => RJCP.IO.Ports.StopBits.One, // fallback
            _ => throw new ArgumentOutOfRangeException(nameof(stopBits), stopBits, null)
        };
    }
    private bool EstablishConnection()
    {
        //_serialPort = SerialPortFactory.Create(_serialPortSettings);
        _logger.LogInformation("Attempting to open serial port: {PortName}", _serialPortSettings.PortName);
        
        var factory = new ModbusFactory();
        var parity = ToRJCPParity(_serialPortSettings.Parity); 
        var stopbits = ToRJCPStopBits(_serialPortSettings.StopBits);
        _serialPortStream = new SerialPortStream
                (_serialPortSettings.PortName, _serialPortSettings.BaudRate, _serialPortSettings.DataBits, parity, stopbits);
        _serialPortStream.ReadTimeout = _serialPortSettings.ReadTimeout;
        _serialPortStream.WriteTimeout = _serialPortSettings.WriteTimeout;
        //_serialPort.Open();
        _serialPortStream.Open();
        _serialPortStream.DiscardInBuffer();
        if (/*_serialPort.IsOpen*/_serialPortStream.IsOpen)
        {
            //_client = ModbusSerialMaster.CreateRtu(_serialPort);
            
            var adapter = new SerialPortStreamAdapter(_serialPortStream);
            _client = factory.CreateRtuMaster(adapter);
            _logger.LogInformation("Serial port {PortName} opened successfully. Modbus client created.", _serialPortSettings.PortName);
        }
        else
        {
            _logger.LogWarning("Failed to open serial port: {PortName}", _serialPortSettings.PortName);
            return false;
        }

        return IsConnected = true;
    }
    private async Task<bool> SetParamsAsync()
    {
        await _semaphoreSlim.WaitAsync(/*TimeSpan.FromMilliseconds(300)*/).ConfigureAwait(false);
        try
        {
            await WriteSettingsAsync().ConfigureAwait(false);
            _logger.LogInformation("Successfully set spindle params");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to set params");
            return false;
        }
        finally
        {
            _semaphoreSlim?.Release();
        }
        return true;
    }
    private async Task WatchingStateAsync(CancellationToken token)
    {
        await Task.Delay(100).ConfigureAwait(false);
        while (!token.IsCancellationRequested)
        {
            await _semaphoreSlim.WaitAsync(/*TimeSpan.FromMilliseconds(300),*/ token).ConfigureAwait(false);
            try
            {
                int current;
                //_serialPort?.DiscardInBuffer();
                current = await GetCurrentAsync().ConfigureAwait(false);
                _freq = await GetFrequencyAsync().ConfigureAwait(false);

                var status = await GetStatusAsync().ConfigureAwait(false);
                var fault = await GetSpinFaultAsync().ConfigureAwait(false);
                _onFreq = status.OnFreq;
                _hasStarted = _onFreq || status.Acc;
                GetSpindleState?.Invoke(this, new SpindleEventArgs(_freq * 6, (double)current / 10, _onFreq, status.Acc, status.Dec, status.Stop)
                {
                    IsOk = fault.IsOk,
                    FaultCode = fault.FaultCode,
                    FaultDescription = fault.FaultDescription
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed {nameof(WatchingStateAsync)}");//TODO count how many frequent
            }
            finally
            {
                _semaphoreSlim.Release();
            }
            await Task.Delay(100).ConfigureAwait(false);
        }
    }
}
