namespace MSPro.CarreraDigital;

// 2001287?21
/* FBBBBBBBBGP$
    Abfrage der letzten Zieldurchfahrt: Das F ist eine 4 Bit Binärwert zwischen 1 und 8, die der Fahrzeugnummer entspricht (7 = Ghostcar, 8 = Pacecar).
   Die nachfolgenden 8 Zeichen bilden ein 32 Bit Binärwort. Dies ist der momentane Zählerstand eines Timers im Rundenzähler. Er läuft mit 1 kHz. Subtrahiert man zwei aufeinanderfolgende Werte für das selbe Fahrzeug, so erhält man die Rundenzeit in Millisekunden.
   Das G gibt die Sensorgruppe an. Die Sensoren der Gruppe '1' bilden die Ziellinie. Die Gruppen '2' und '3' können z.B. für Zwischenzeiten verwendet werden..
   Die Übertragung wird wie üblich mit Prüfsumme und $ abgeschlossen.
*/



/// <summary>
///     Represents the current state information of the control unit, including car statuses, start lights, and circuit
///     settings.
/// </summary>
/// <remarks>
///     This class provides a snapshot of the control unit's state as parsed from a data string. It includes
///     information about each car, such as tank level and pit lane status, as well as the current start light and circuit
///     configuration. Use the Parse method to create an instance from a raw data string received from the control
///     unit.
/// </remarks>
public class StateInfo
{
    private const int CAR_COUNT = 8;

    /// <summary>
    ///     Gets the collection of cars managed by this instance.
    /// </summary>
    public Car[] Cars { get; } = new Car[CAR_COUNT];

    /// <summary>
    ///     Gets the start lights controller for the current session.
    /// </summary>
    public CStartLights StartLights { get; private set; } = null!;

    /// <summary>
    ///     Gets the current configuration settings for the circuit.
    /// </summary>
    public CCircuitSettings CircuitSettings { get; private set; } = null!;


    /// <summary>
    ///     The constructor is private and is not intended to be called directly, use <see cref="Parse"/>().
    /// </summary>
    /// <remarks>
    ///     This constructor is used to restrict instantiation of the LapTime class. Instances can only
    ///     be created through designated factory methods or properties provided by the class.
    /// </remarks>
    private StateInfo() { }



    /// <summary>
    ///     Parses a data string representing the current state of all vehicles and related circuit information, and returns
    ///     a corresponding StateInfo object.
    /// </summary>
    /// <remarks>
    ///     The input string is expected to follow a specific protocol, where each character or group of
    ///     characters encodes information such as tank levels for each vehicle, pit lane status, and circuit settings. The
    ///     method does not validate the format or length of the input string; callers must ensure the input is well-formed
    ///     according to the protocol specification.
    /// </remarks>
    /// <param name="rawData">
    ///     The data string to parse. Must contain encoded information for vehicle tank levels, pit lane status, start
    ///     lights, and circuit settings. The format and length must match the expected protocol; otherwise, parsing may
    ///     fail or produce incorrect results.
    /// </param>
    /// <returns>
    ///     A StateInfo object populated with the parsed vehicle and circuit state information extracted from the input
    ///     string.
    /// </returns>
    public static StateInfo Parse(string rawData)
    {
        var result = new StateInfo();

        /*  Die folgenden 6 Zeichen (T) sind die Tankstände für die 6 Fahrzeuge jeweils als 4 Bit Binärwert.
            Obwohl das Driverdisplay nur 8 Zustände kennt, werden 16 Zustände für jedes Fahrzeug unterschieden.
            $F bedeutet der Tang des Fahrzeuges ist voll - $0 er ist leer.
            Die Tankstände werden in der Reihenfolge der Fahrzeug Ids übertragen.*/

        for (var i = 0; i < CAR_COUNT; i++)
        {
            result.Cars[i] = new Car(i)
            {
                Tank = rawData[i + 1] & 0xF
            };
        }

        ;

        /*  Die folgenden beiden Zeichen (V) sind bisher immer 0.
            Es kann sein, dass es sich um die Tankstände für Ghost- und Pacecar handelt,
            oder sie dienen einem Zweck, der sich mir noch nicht offenbart hat.*/

        result.StartLights = new CStartLights(rawData[1 + CAR_COUNT]);
        result.CircuitSettings = new CCircuitSettings(rawData[1 + CAR_COUNT + 1])
        {
            /*  Das letzte Daten-Zeichen (A) ist entweder 6 oder 8,
                je nachdem ob der Position Tower 6 oder 8 Fahrzeuge anzeigen soll. */ CarsOnTower = rawData[1 + CAR_COUNT + 3] == '6' ? 6 : 8
        };

        /*
         * Die zwei folgenden Zeichen (B) formen einen binären Bytewert, der eine Bitmaske bildet.
         * Ist das der Fahrzeug No zugehörige Bit gesetzt,
         * so befindet sich das Fahrzeug in einer Pitlane mit Pitlane Adapter und kann tanken.
         * D.h. nicht das es tankt, nur dass es tanken kann.
         */
        uint b = ControlUnitCore.ToBinary(rawData[(1 + CAR_COUNT + 2)..], 1);
        for (int carId = CAR_COUNT - 1; carId >= 0; carId--)
        {
            // Shift 1 to the left by 'i' places and check if that bit is set
            result.Cars[carId].InPitLane = (b & (1 << carId)) != 0;
        }

        return result;
    }



    public class Car(int no)
    {
        public int No { get; } = no;
        public int Tank { get; set; }
        public bool InPitLane { get; set; }
    }



    public class CCircuitSettings
    {
        public enum RefuelModeEnum
        {
            Off = 0
            , Normal = 1
            , Real = 2
        }



        public CCircuitSettings(char c)
        {
            int m = c - '0';
            RefuelMode = (RefuelModeEnum)(m & 3);
            HasPitLane = (m & 4) != 0;
            HasCounter = (m & 8) != 0;
        }



        /* Das nächste Zeichen (M) gibt den Tankmodus an.
         0  - ausgeschaltet,
         1  - normaler Modus,
         2  - Real Modus.
         Sollte in der Bahn ein Pitlane Adapter vorhanden sein, so wird 4 auf den entsprechenden Wert addiert.
         Ist ein Rundenzähler an die Rundenzählerbuchse angeschlossen, so wird 8 addiert.
         */
        public RefuelModeEnum RefuelMode { get; }
        public bool HasPitLane { get; }
        public bool HasCounter { get; }
        public int CarsOnTower { get; internal set; }
    }



    public class CStartLights(char s)
    {
        /* Das Startampelzeichen (S) nimmt die Werte 0 bis 9 an.
        0 bedeutet, dass gerade ein Rennen läuft
        Eine 1, dass die Starttaste einmal gedrückt wurde und alle Leds leuchten.
        Wird die Starttaste ein zweites Mal gedrückt, zählt der Wert von 2 (1 Led leuchtet) bis 7 hoch, um dann auf die 0 zu wechseln.
        Im Falle eines Frühstarts wird zuerst eine 8 und kurz danach eine 9 übertragen.
        Die No des frühstartenden Fahrzeuges wird nicht übertragen. */

        public bool Race { get; } = s == '0';
        public int CountDown { get; } = s >= '1' && s <= '7' ? s - '0' : 0; // 1: Initialized ... Lights:2..6 ... 7: Race Start
        public bool FalseStart { get; } = s == '8';
    }
}