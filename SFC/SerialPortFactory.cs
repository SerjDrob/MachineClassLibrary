using System;
using System.IO.Ports;

namespace MachineClassLibrary.SFC;

public static class SerialPortFactory
{
    public static SerialPort Create(SerialPortSettings settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        var port = new SerialPort(
            settings.PortName,
            settings.BaudRate,
            settings.Parity,
            settings.DataBits,
            settings.StopBits);

        //port.Handshake = settings.Handshake;
        // другие свойства, которые нельзя задать через конструктор
        return port;
    }
}
