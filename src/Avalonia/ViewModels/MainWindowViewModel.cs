using CommunityToolkit.Mvvm.ComponentModel;



namespace MyMvvmApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public string Greeting { get; } = "Welcome to Avalonia!";
}