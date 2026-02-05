using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MSPro.CarreraDigital;



namespace MyMvvmApp.ViewModels;

/*
    This is just a sample that shows how to use the ControlUnit class.
    It is not a complete implementation of a Carrera Digital app!

    The UI functionality is absolutely basic - and quick & dirty implemented.
    The main purpose is to show how to use the ControlUnit class in an MVVM app.
 */



public partial class MainViewModel : ObservableObject
{
    private readonly ControlUnit _cu;
    private readonly TimeSpan[] _lastFinishTimes = new TimeSpan[ControlUnitCore.CAR_COUNT];



    public MainViewModel()
        => _cu = new ControlUnit(OnStateInfoChanged, OnLapTime);



    [ObservableProperty] private string _lapTime0 = "---";
    [ObservableProperty] private string _lapTime1 = "---";
    [ObservableProperty] private string _lapTime2 = "---";
    [ObservableProperty] private string _lapTime3 = "---";

    [ObservableProperty] private int _lapCount0;
    [ObservableProperty] private int _lapCount1;
    [ObservableProperty] private int _lapCount2;
    [ObservableProperty] private int _lapCount3;


    private void OnLapTime(LapTime arg)
    {
        switch (arg.CarNo)
        {
            case 0:
                LapCount0++;
                break;
            case 1:
                LapCount1++;
                break;
            case 2:
                LapCount2++;
                break;
            case 3:
                LapCount3++;
                break;
        }


        if (_lastFinishTimes[arg.CarNo] == TimeSpan.Zero)
        {
            _lastFinishTimes[arg.CarNo] = arg.Timer;
            return;
        }


        TimeSpan lapTime = arg.Timer - _lastFinishTimes[arg.CarNo];
        var lapTimeS = lapTime.ToString(@"ss\,ff");
        switch (arg.CarNo)
        {
            case 0:
                LapTime0 = lapTimeS;
                break;
            case 1:
                LapTime1 = lapTimeS;

                break;
            case 2:
                LapTime2 = lapTimeS;
                break;
            case 3:
                LapTime3 = lapTimeS;
                break;
        }

        _lastFinishTimes[arg.CarNo] = arg.Timer;
    }



    private void OnStateInfoChanged(StateInfo arg) { ErrorMessage = "StateInfo Change!"; }


    [ObservableProperty] private ObservableCollection<string> _availablePorts = new(SerialPort.GetPortNames());

    [ObservableProperty] private string? _selectedPort;

    [ObservableProperty] private string _cuVersion = "---";

    [ObservableProperty] private string _errorMessage = "---";



    partial void OnSelectedPortChanged(string? value)
        => _ = OnSelectedPortChangedAsync(value);



    private async Task OnSelectedPortChangedAsync(string? value)
    {
        try
        {
            _cu.Disconnect();
            CuVersion = "---";
            CuVersion = await _cu.ConnectAsync(value!);
            while (true) await _cu.PollAsync();
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }
}