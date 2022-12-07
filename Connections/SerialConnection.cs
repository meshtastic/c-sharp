using System.IO.Ports;
using System.Text;
using Meshtastic.Protobufs;

namespace Meshtastic.Connections;

public class SerialConnection : IConnection
{
    private readonly SerialPort serialPort;

    public SerialConnection(string port, int baudRate = Resources.DEFAULT_BAUD_RATE)
    {
        serialPort = new SerialPort(port, baudRate);
        serialPort.Handshake = Handshake.None;
        // serialPort.ReadTimeout = 500;
        // serialPort.WriteTimeout = 500;
    }

    // public void OnFromRadio(Action<byte[]> action) 
    // {
    //     serialPort.DataReceived += (sender, e) => {
    //         action.Invoke(UTF8Encoding.Default.GetBytes(serialPort.ReadExisting()));
    //     };
    // }

    public async Task Monitor() 
    {
        try
        {
            serialPort.Open();
            while (serialPort.IsOpen) 
            {
                if (serialPort.BytesToRead > 0) {
                    Console.Write(serialPort.ReadExisting());
                }
                await Task.Delay(10);
            }
            Console.WriteLine("Serial disconnected");
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex);
        }
    }

    private char[] EncodeData(byte[] data) => System.Text.Encoding.UTF8.GetString(data).ToCharArray();

    public async Task WriteToRadio(byte[] data)
    {
        try
        {
            var toRadio = PacketFraming.CreatePacket(data);
            serialPort.Open();
            // using (var writer = new StreamWriter(serialPort.BaseStream, System.Text.Encoding.UTF8, toRadio.Length, true))
            // {
            //     var wakeUp = EncodeData(Enumerable.Repeat<byte>(Resources.PACKET_FRAME_START[1], 32).ToArray());
            //     await writer.WriteAsync(wakeUp);
            //     await writer.FlushAsync();

            //     // Wait 100ms to give device time to start running
            //     await Task.Delay(100);

            //     await writer.WriteAsync(EncodeData(toRadio));
            //     await writer.FlushAsync();
            //     await Task.Delay(100);
            // }
            //await Task.Delay(3000);
            serialPort.Write(Resources.SERIAL_PREAMBLE, 0, Resources.SERIAL_PREAMBLE.Length);
            serialPort.DiscardInBuffer();
            serialPort.Write(toRadio, 0, toRadio.Length);
            await Task.Delay(100);
            await ReadPacket();
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex);
        }
        Console.WriteLine("Serial disconnected");
    }

    private async Task ReadPacket()
    {
        List<byte> buffer = new();
        var packetLength = 0;
        while (serialPort.IsOpen)
        {
            if (serialPort.BytesToRead == 0)
                continue;

            var item = (byte)serialPort.ReadByte();

            int bufferIndex = buffer.Count();
            buffer.Add(item);
            if (bufferIndex == 0) 
            {
                if (item != Resources.PACKET_FRAME_START[0])
                    buffer.Clear();
            } 
            else if (bufferIndex == 1) 
            {
                if (item != Resources.PACKET_FRAME_START[1])
                    buffer.Clear();
            }
            else if (bufferIndex >= Resources.PACKET_HEADER_LENGTH - 1) 
            {
                packetLength = (buffer[2] << 8) + buffer[3];
                // Packet fails size validation
                if (bufferIndex == Resources.PACKET_HEADER_LENGTH - 1 && packetLength > Resources.MAX_TO_FROM_RADIO_LENGTH) 
                    buffer.Clear();

                if (buffer.Count() > 0 && (bufferIndex + 1) >= (packetLength + Resources.PACKET_HEADER_LENGTH))
                {
                    try 
                    {
                        var payload = buffer.Skip(Resources.PACKET_HEADER_LENGTH).ToArray();
                        var fromRadio = FromRadio.Parser.ParseFrom(payload);
                        Console.WriteLine(fromRadio.ToString());
                    }
                    catch (Exception ex) {
                        Console.WriteLine(ex);
                    }
                    buffer.Clear();
                }
            }
            // b = self._readBytes(1)
            //     if len(b) > 0:
            //         c = b[0]
            //         ptr = len(self._rxBuf)
            //         # Assume we want to append this byte, fixme use bytearray instead
            //         self._rxBuf = self._rxBuf + b

            //         if ptr == 0:  # looking for START1
            //             if c != START1:
            //                 self._rxBuf = empty  # failed to find start
            //         elif ptr == 1:  # looking for START2
            //             if c != START2:
            //                 self._rxBuf = empty
            //         elif ptr >= HEADER_LEN - 1:  # we've at least got a header
            //             # big endian length follows header
            //             packetlen = (self._rxBuf[2] << 8) + self._rxBuf[3]
            //             if ptr == HEADER_LEN - 1:  # we _just_ finished reading the header, validate length
            //                 if packetlen > MAX_TO_FROM_RADIO_SIZE:
            //                     self._rxBuf = empty  # length was out out bounds, restart
            //             if len(self._rxBuf) != 0 and ptr + 1 >= packetlen + HEADER_LEN:
            //                 try:
            //                     self._handleFromRadio(self._rxBuf[HEADER_LEN:])
            //                 except Exception as ex:
            //                     logging.error(f"Error while handling message from radio {ex}")
            //                     traceback.print_exc()
            //                 self._rxBuf = empty


            // Console.WriteLine($"Found:{headerFound} item:{item}");
            // if (headerFound == FoundHeader.NotFound && item == Resources.PACKET_FRAME_START[0]) 
            // {
            //     headerFound = FoundHeader.FoundStart;
            // }
            // else if (headerFound == FoundHeader.FoundStart && item == Resources.PACKET_FRAME_START[1]) 
            // {
            //     headerFound = FoundHeader.FoundSecond;
            // }
            // else if (headerFound == FoundHeader.FoundSecond && item > 0) 
            // {
            //     headerFound = FoundHeader.FoundLength;
            //     packetLength = item;
            // }
            // else if (headerFound == FoundHeader.FoundLength && buffer.Count() < packetLength)
            // {
            //     buffer.Append(item);
            // }
            // else if (headerFound == FoundHeader.FoundLength && buffer.Count() == packetLength)
            // {
            //     var fromRadio = FromRadio.Parser.ParseFrom(buffer.ToArray());
            //     Console.WriteLine(fromRadio.ToString());
            //     buffer = Enumerable.Empty<byte>();
            //     packetLength = 0;
            //     headerFound = FoundHeader.NotFound;
            // }
        }
        await Task.Delay(5000);
    }
}