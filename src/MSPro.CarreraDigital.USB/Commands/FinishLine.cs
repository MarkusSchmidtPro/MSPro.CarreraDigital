namespace MSPro.CarreraDigital;

/// <summary>
/// Request last finish line passage or state info,
/// depending on the first char of the response.
/// </summary>
public class FinishLine
{
    public const char COMMAND = '?';


    /// <summary>
    ///     Depending on the first char either a new <see cref="LapTime">lap time</see>
    ///     or <see cref="StateInfo">state info</see>.
    /// </summary>
    /// <example>
    /// <code>
    ///     string rawData = await cu.SendAndReceiveAsync(FinishLine.COMMAND);
    ///     if (FinishLine.IsStateInfo(rawData))
    ///     {
    ///         StateInfo stateInfo = StateInfo.Parse(rawData);
    ///         //Console.WriteLine(JsonConvert.SerializeObject(stateInfo, Formatting.Indented));
    ///         Console.WriteLine($"{DateTime.Now}: StateInfo");
    ///     }
    ///     else
    ///     {
    ///         LapTime lapTime = LapTime.Parse(rawData);
    ///         //Console.WriteLine(JsonConvert.SerializeObject(lapTime, Formatting.Indented));
    ///         Console.WriteLine($"{DateTime.Now}: Car[{lapTime.CarNo} {lapTime.Timer}");
    ///     }
    /// </code>
    /// </example>
    public static bool IsStateInfo(string d) => d[0] == ':';
}