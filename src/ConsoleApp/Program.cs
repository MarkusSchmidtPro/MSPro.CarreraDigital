using System.IO.Ports;
using MSPro.CarreraDigital;



namespace ConsoleApp;

/// <summary>
///     A simple console application to test the ControlUnitCore class
///     by connecting to a Carrera Digital track and sending commands to it.
///     The application will print the version information and
///     then continuously send commands to receive state information and lap times,
///     printing them to the console.
/// </summary>
internal class Program
{
    private static async Task Main(string[] args)
    {
        printSerialPorts();

        //
        // Adjust the port name in the ControlUnitCore constructor below as needed.
        //
        const string LINUX_PORT = "/dev/ttyUSB0";
        const string WINDOWS_PORT = "COM4";

        //
        // Connect to CU and get version.
        // Note: Use using so that Dispose() is called at the end and
        // that all ports are closed properly.
        //
        using var cu = new ControlUnitCore(WINDOWS_PORT);
        string cuVersion = await cu.ConnectAsync();
        if (cuVersion.Length == 0)
            throw new ApplicationException("Something went wrong. Could not get the CU Version!");

        //
        // Send and receive data until a key is pressed.
        //
        Console.Clear();
        Console.WriteLine($"Version:{cuVersion}");
        Console.WriteLine("Send & Receive, press any key to stop.");

        while (!Console.KeyAvailable)
        {
            string rawData = await cu.SendAndReceiveAsync(FinishLine.COMMAND);
            if (rawData.Length == 0) continue;
            Console.WriteLine("RAW Value: {d}");

            if (FinishLine.IsStateInfo(rawData))
            {
                StateInfo stateInfo = StateInfo.Parse(rawData);
                //Console.WriteLine(JsonConvert.SerializeObject(stateInfo, Formatting.Indented));
                Console.WriteLine($"{DateTime.Now}: StateInfo");
            }
            else
            {
                LapTime lapTime = LapTime.Parse(rawData);
                //Console.WriteLine(JsonConvert.SerializeObject(lapTime, Formatting.Indented));
                Console.WriteLine($"{DateTime.Now}: Car[{lapTime.CarNo} {lapTime.Timer}");
            }

            // Sleep for a while to avoid flooding the console.
            Thread.Sleep(1000);
        }
    }



    /// <summary>
    ///     Lists all available serial ports on the system to the console output.
    /// </summary>
    /// <remarks>
    ///     This method writes the names of detected serial ports to the standard output. It is intended
    ///     for diagnostic or informational purposes and does not return any values.
    /// </remarks>
    private static void printSerialPorts()
    {
        Console.WriteLine("Available Serial Ports:");
        string[] ports = SerialPort.GetPortNames();
        foreach (string port in ports)
        {
            Console.WriteLine(" - " + port);
        }
    }
}