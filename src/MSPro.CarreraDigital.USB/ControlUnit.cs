namespace MSPro.CarreraDigital;

/// <summary>
///     Represents the primary interface to communicate with the Carrera Digital Control Unit (CU).
///     This class is responsible for connecting, polling, and disconnecting from the CU,
///     providing functionality to receive updates related to the state of the CU and lap times.
///     The <see cref="ControlUnit" /> class allows clients to establish a connection through a specific serial port,
///     and it processes incoming data from the CU, either as state information or lap timing data.
/// </summary>
/// <remarks>
///     The class encapsulates the core logic to manage communication with the Control Unit (CU) hardware,
///     including the management of data exchange and hardware connection lifecycles.
///     It invokes user-defined callbacks when state information or lap timing data is received.
/// </remarks>
/// <example>
///     This class does not include example usage. The user is expected to supply callback methods
///     for state change notifications and lap timing updates when initializing the class.
/// </example>
/// <remarks>
///     This class implements <see cref="IDisposable" /> to ensure proper resource cleanup, such as the hardware
///     connection.
///     Be sure to call <see cref="Dispose" /> or use a `using` construct to release resources.
/// </remarks>
public class ControlUnit
(
    Action<StateInfo> onStateInfoChanged
    , Action<LapTime> onLapTime) : IDisposable
{
    private ControlUnitCore? _cu;



    /// <summary>
    /// Asynchronously establishes a connection to the Control Unit using the specified serial port
    /// and retrieves the version information of the Control Unit.
    /// </summary>
    /// <param name="serialPort">The name of the serial port to use for the connection.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the Control Unit version string.</exception>
    /// <exception cref="InvalidOperationException">Thrown if a connection is already established and not disconnected before reconnection.</exception>
    /// <exception cref="ApplicationException">Thrown if the connection succeeds but the Control Unit version information could not be retrieved.</exception>
    public async Task<string> ConnectAsync(string serialPort)
    {
        if (_cu != null)
            throw new InvalidOperationException("Disconnect before re-connect!");

        _cu = new ControlUnitCore(serialPort);
        string cuVersion = await _cu.ConnectAsync();
        if (cuVersion.Length == 0)
            throw new ApplicationException("Something went wrong. Could not get the CU Version!");
        return cuVersion;
    }



    private string? _lastState;



    /// <summary>
    /// Asynchronously polls the Control Unit for updated state information or lap time data.
    /// Processes the received data and triggers the corresponding action when changes are detected,
    /// either with updated state information or a new lap time entry.
    /// </summary>
    /// <returns>A task that represents the asynchronous polling operation. No return value.</returns>
    /// <exception cref="ApplicationException">Thrown if the connection has not been established by calling <c>ConnectAsync</c> prior to polling.</exception>
    public async Task PollAsync()
    {
        if (_cu == null)
            throw new ApplicationException("open Port first!");

        string rawData = await _cu.SendAndReceiveAsync(FinishLine.COMMAND);
        if (FinishLine.IsStateInfo(rawData))
        {
            if (_lastState == rawData) return; // no change
            _lastState = rawData;
            onStateInfoChanged(StateInfo.Parse(rawData));
        }
        else
        {
            onLapTime(LapTime.Parse(rawData));
        }
    }



    /// <summary>
    /// Disconnects from the currently connected Control Unit and releases any associated resources.
    /// </summary>
    public void Disconnect()
    {
        if (_cu == null) return;
        _cu.Dispose();
        _cu = null;
    }



    public void Dispose() => Disconnect();
}