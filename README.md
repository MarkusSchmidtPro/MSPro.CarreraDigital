# Carrera Digital
A demo project for connecting a Carrera Digital to a Windows/Linux PC via USB.

# Overview

I own a Carrera Digital 124 und I am a passionate C# developer. 

At Christmas 2025, my son gave me a Raspberry PI4 as a gift, and I started learning about Linux and wanted to “talk” to my Carrera racetrack.

> **C#, .Net, Windows / Linux, USB, Carrera Digital** - visualize the data from the Control Unit and probably control what happens on the track (send commands) - to exchange data with the CU (Control Unit) and CT (Control Tower) ...

... that is mainly what I was aiming for.

The solution is split into two projects:

1. A tiny C# **Console Application**, and 
2. the **Carrera USB** project that implements to communication.

### Pre-Requisites

* [.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0), and
* Visual Studio 2026 Community or JetBrains Rider

> In addition, I have added a User Interface (built with [Avalonia](https://avaloniaui.net/)), which I do NOT want to describe any further here. We are focusing on Carrera, not on *Avalonia*! I have chosen *Avalonia* as the best choice for an OS independent User-Interface: Linux, Windows, Web, Mobile etc..

## References

The **Carrera USB** implementation is based on the Carrera protocol descriptions which can be found [here](http://slotbaer.de/carrera-digital-124-132.html). . The implementation is not complete but it can be used as a good and stable basis in C# to see how to communicate with CU and the control-tower CT.

I have tested with with Linux (Raspberry PI4) and Windows 11.

# The Author

Markus Schmidt (PRO)
D-82008 Unterhaching

### Additional Links

* [Dipl.-Ing. Peter Niehues - Der Carrera Protokolldecodierer - Carrera Hardware Hacks](http://wasserstoffe.de/carrera-hacks/protocol-decode/index.html)
* [CarreraDigitalControlUnit | Arduino Documentation](https://docs.arduino.cc/libraries/carreradigitalcontrolunit/)
