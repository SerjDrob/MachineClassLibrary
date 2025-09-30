using System.IO.Ports;

namespace MachineClassLibrary.SFC;

public class SerialPortSettings
{
    public string PortName { get; set; } = "COM1";
    public int BaudRate { get; set; } = 9600;
    public Parity Parity { get; set; } = Parity.None;
    public int WriteTimeout { get; set; } = 1000;
    public int ReadTimeout { get; set; } = 1000;
    public int DataBits { get; set; } = 8;
    public StopBits StopBits { get; set; } = StopBits.One;

}
