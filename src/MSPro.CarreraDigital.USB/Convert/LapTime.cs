namespace MSPro.CarreraDigital;

/*
  * FBBBBBBBBGP$	Abfrage der letzten Zieldurchfahrt:
  * Das F ist eine 4 Bit Binärwert zwischen 1 und 8, die der Fahrzeugnummer entspricht
  * (7 = Ghostcar, 8 = Pacecar).

     Die nachfolgenden 8 Zeichen bilden ein 32 Bit Binärwort.
     Dies ist der momentane Zählerstand eines Timers im Rundenzähler.

     Er läuft mit 1 kHz. Subtrahiert man zwei aufeinanderfolgende Werte für das selbe Fahrzeug,
     so erhält man die Rundenzeit in Millisekunden.

     Das G gibt die Sensorgruppe an.
     Die Sensoren der Gruppe '1' bilden die Ziellinie. Die Gruppen '2' und '3' können z.B. für Zwischenzeiten verwendet werden..
  */



/// <summary>
///     Represents the timing information for a single lap or sensor crossing event, including car number, sensor group,
///     and
///     elapsed time.
/// </summary>
/// <remarks>
///     A LapTime instance provides details about a car's passage over a sensor, such as the car's
///     identifier, the group of the sensor (e.g., finish line or intermediate timing), and the measured time interval. The
///     car number distinguishes between regular cars, ghost cars, and pace cars. The sensor group indicates the type of
///     sensor crossed, with group 1 typically representing the finish line.
/// </remarks>
public class LapTime
{
    /// <summary>
    ///     Parses a string representation (received raw-data)
    ///     of a lap time and returns a corresponding LapTime object.
    /// </summary>
    /// <remarks>
    ///     The input string is expected to contain encoded information for car number, sensor group, and
    ///     lap time in milliseconds. Supplying a string that does not conform to the expected format may result in
    ///     incorrect parsing or runtime exceptions.
    /// </remarks>
    /// <param name="rawData">
    ///     The string containing the encoded lap time data to parse. Must be at least 10 characters in length and formatted
    ///     according to the expected encoding scheme.
    /// </param>
    /// <returns>A LapTime object representing the parsed lap time data from the input string.</returns>
    public static LapTime Parse(string rawData)
    {
        var result = new LapTime
        {
            CarNo = rawData[0] - '0'
            , SensorGroup = rawData[9] - '0'
        };
        // 003404?1

        uint milliseconds = ControlUnitCore.ToBinary(rawData[1..], 4);
        Console.WriteLine($"{rawData[1..9]} - {milliseconds}");
        result.Timer =
            TimeSpan.FromMilliseconds(milliseconds);
        return result;
    }



    /// <summary>
    ///     The constructor is private and is not intended to be called directly, use <see cref="Parse" />().
    /// </summary>
    /// <remarks>
    ///     This constructor is used to restrict instantiation of the LapTime class. Instances can only
    ///     be created through designated factory methods or properties provided by the class.
    /// </remarks>
    private LapTime() { }



    public int SensorGroup { get; private init; }
    public TimeSpan Timer { get; private set; }

    
    public int CarNo { get; private init; }
    public bool IsGhostCar => CarNo == 7;
    public bool IsPaceCar => CarNo == 8;
}