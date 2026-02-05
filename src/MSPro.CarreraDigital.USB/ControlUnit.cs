using System.Threading.Tasks;

namespace MSPro.CarreraDigital;

public class ControlUnit(
    Action<StateInfo> onStateInfoChanged,
    Action<LapTime> onLapTime) : IDisposable
{
    private ControlUnitCore? _cu;


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



    public void Disconnect()
    {
        if (_cu == null) return;
        _cu.Dispose();
        _cu = null;
    }



    public void Dispose() => Disconnect();
}