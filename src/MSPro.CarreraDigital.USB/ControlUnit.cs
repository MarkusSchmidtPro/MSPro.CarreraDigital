using System.IO.Ports;
using System.Text;



namespace MSPro.CarreraDigital;

/// <summary>
///     Implements the core communication logic (send and receive [raw-ASCII] data)
///     to communicate with the Carrera Digital Control Unit (CU) over a serial port.
/// </summary>
/// <seealso href="http://slotbaer.de/carrera-digital-124-132/10-cu-rundenzaehler-protokoll.html" />
public class ControlUnitCore : IDisposable
{
    private readonly SerialPort _serialPort;

    private const char GET_VERSION = '0';



    public ControlUnitCore(string portName)
    {
        _serialPort = new SerialPort(portName, 19200, Parity.None, 8, StopBits.One);
        _serialPort.Handshake = Handshake.None;
        _serialPort.WriteTimeout = 500;
        _serialPort.DtrEnable = true; // Data Terminal Ready
        _serialPort.RtsEnable = true; // Request to Send
    }



    public async Task<string> ConnectAsync()
    {
        _serialPort.Open();
        _serialPort.DiscardInBuffer(); // Delete 'old' data from the buffer, if any.
        _serialPort.DiscardOutBuffer();
        return await SendAndReceiveAsync(GET_VERSION);
    }



    /// <summary>
    ///     Send and Receive data to and from CU.
    /// </summary>
    /// <remarks>
    ///     Send a command to the CU and waits for the response.
    ///     The communication protocol is as follows:
    ///     * The computer sends a command character to the CU, prefixed by an opening quote (").
    ///     * The CU responds with<br />
    ///     1. the command character,<br />
    ///     2. followed by the response data,<br />
    ///     3. a checksum, and<br />
    ///     4. a closing dollar sign ($).
    /// </remarks>
    /// <returns>
    ///     The raw data received.
    /// </returns>
    public async Task<string> SendAndReceiveAsync(char command)
    {
        _serialPort.Write($"\"{command}");
        string answer = await receiveResponseAsync();

        char answerCommand = answer[0];
        char endTag = answer[^1];

        if (answerCommand == '#')
            throw new ApplicationException("Unknown command!");
        if (answerCommand != command)
            throw new ApplicationException("Invalid response!");
        if (answer.Length < 4) //   Payload min one character
            throw new ApplicationException("Response too short!");
        if (endTag != '$') //   Payload min one character
            throw new ApplicationException("Invalid end-tag!");

        string payload = answer[1..^2];
        char checksum = answer[^2];
        validateChecksum(payload, checksum);
        return payload;
    }



    /// <summary>
    ///     Asynchronously reads a complete response from the serial port,
    ///     returning the received data as an ASCII string (raw response).
    /// </summary>
    /// <remarks>
    ///     The method waits up to one second for the first byte of data. After the first byte is
    ///     received, it continues reading until a 20-millisecond gap occurs, which is treated as the end of the response.
    ///     This approach is suitable for burst-style serial communications where the end of a message is indicated by a
    ///     pause in data transmission. The method does not throw an exception if no data is received; instead, it returns
    ///     an empty string.
    /// </remarks>
    /// <returns>
    ///     A string containing the ASCII-decoded data received from the serial port.<br />
    ///     Returns an empty string if no data is received before the timeout.
    /// </returns>
    private async Task<string> receiveResponseAsync()
        => await Task.Run(() =>
        {
            List<byte> receivedData = [];

            try
            {
                // 1. Set the 1-second "First Character" timeout
                _serialPort.ReadTimeout = 1000;

                // Try to get the first byte
                receivedData.Add((byte)_serialPort.ReadByte());

                // At 19200 baud, characters arrive every 0.52ms.
                // If you wait 20ms and nothing arrives, you have missed approximately 40 character "slots."
                // This is a massive gap in serial terms, giving you high confidence that the device is done sending its burst.
                _serialPort.ReadTimeout = 20;
                while (true) receivedData.Add((byte)_serialPort.ReadByte());
            }
            catch (TimeoutException)
            {
                // If we have data, the timeout just means the stream ended normally.
                // If receivedData is empty, it means the device never responded.
            }

            return Encoding.ASCII.GetString(receivedData.ToArray());
        });



    /// <summary>
    ///     Validates that the provided checksum matches the calculated checksum for the specified payload.
    /// </summary>
    /// <param name="payload">
    ///     The string payload for which the checksum is to be validated. Cannot be null.
    /// </param>
    /// <param name="checksum">
    ///     The checksum character to validate against the payload.
    /// </param>
    /// <exception cref="ApplicationException">
    ///     Thrown if the checksum does not match the calculated value for the payload.
    /// </exception>
    private static void validateChecksum(string payload, char checksum)
    {
        int total = payload.Sum(c => c & 0xF);
        if ((checksum & 0xF) != (total & 0xF))
            throw new ApplicationException("Invalid Checksum!");
    }



    /// <summary>
    ///     Converts a sequence of "Carrera-Characters" to a 32-bit unsigned integer.
    /// </summary>
    /// <remarks>
    ///     The method expects the input string to contain at least twice as many characters as the
    ///     specified byte count - for example: eight character for a 32-bit DWord.
    ///     Each pair of characters is interpreted as a single byte,
    ///     with the first character representing the low nibble and
    ///     the second character representing the high nibble.
    ///     The bytes are combined in big-endian order to form the resulting integer.<br />
    ///     ### Example<br />
    ///     Char 0: B3_NL  Byte 3, Low Nibble<br />
    ///     Char 1: B3_NH  Byte 3, High Nibble<br />
    ///     ..<br />
    ///     Char 4: B1_NL  Byte 1, Low Nibble<br />
    ///     Char 5: B1_NH  Byte 1, High Nibble<br />
    ///     Char 6: B0_NL  Lowest Byte, Low Nibble<br />
    ///     Char 7: B0_NH  Lowest Byte, High Nibble<br />
    ///     --> 1|0 3|2 5|4 7|6<br />
    ///     <br />
    ///     raw-data          : "00747&lt;51"<br />
    ///     hex-representation: 00 74 7C 51<br />
    ///     low/high fixed    : 00 47 C7 15<br />
    ///     decimal           : 4.704.021<br />
    ///     Time.Span         : 01:18:24.0210000<br />
    /// </remarks>
    /// <param name="chars">
    ///     A string containing hexadecimal characters. Each byte is represented by two characters, with the low nibble
    ///     first followed by the high nibble.
    /// </param>
    /// <param name="byteCount">The number of bytes to convert from the input string. Must be between 1 and 4, inclusive.</param>
    /// <returns>A 32-bit unsigned integer representing the binary value of the specified bytes parsed from the input string.</returns>
    public static uint ToBinary(string chars, int byteCount)
    {
        uint total = 0;
        for (var i = 0; i < byteCount; i++)
        {
            var lowNibble = (byte)(chars[i * 2] & 0xF);
            var highNibble = (byte)(chars[i * 2 + 1] & 0xF);
            var b = (byte)((highNibble << 4) | lowNibble);
            int byteNo = byteCount - 1 - i;
            total += (uint)(b << (8 * byteNo));
            var x = $"{total:X}";
        }
        return total;
    }



    void IDisposable.Dispose()
    {
        _serialPort.Close();
        _serialPort.Dispose();
    }
}