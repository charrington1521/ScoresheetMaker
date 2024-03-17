using System.IO.Ports;

public class Serial 
{
    static SerialPort _serialPort = new SerialPort();

    static String _message = "";

    public void SerialOutInit()
    {
        _serialPort.PortName = SetPortName(_serialPort.PortName);
        _serialPort.BaudRate = 9600;
        _serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), "None", true);
        _serialPort.DataBits = 8;
        _serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), "One", true);
        _serialPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), "XOnXOff", true);
        _serialPort.Open();
    }

    public string readLineBlocking()
    {
        return _serialPort.ReadLine();
    }

    public void close()
    {
        _serialPort.Close();
    }

    public static string SetPortName(string defaultPortName)
    {
        string portName;

        if (SerialPort.GetPortNames().Count() == 1)
        {
            portName = SerialPort.GetPortNames()[0];
        }
        else
        {
            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter COM port value (Default: {0}): ", defaultPortName);
            portName = Console.ReadLine();

            if (portName == "" || !(portName.ToLower()).StartsWith("com"))
            {
                portName = defaultPortName;
            }
        }

        return portName;
    }
}