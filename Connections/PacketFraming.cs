namespace Meshtastic.Connections;

public static class PacketFraming
{
    public static byte[] GetPacketHeader(byte[] data) => 
        new byte[] {
            Resources.PACKET_FRAME_START[0],
            Resources.PACKET_FRAME_START[1],
            (byte)((data.Length >> 8) & 0xff),
            (byte)(data.Length & 0xff),
        };

    public static byte[] CreatePacket(byte[] data) =>
        GetPacketHeader(data)
        .Concat(data)
        .ToArray();
}

    
/*
b = self._readBytes(1)
if len(b) > 0:
    c = b[0]
    ptr = len(self._rxBuf)

    self._rxBuf = self._rxBuf + b

    if ptr == 0:  # looking for START1
        if c != START1:
            self._rxBuf = empty  # failed to find start
            if self.debugOut is not None:
                try:
                    self.debugOut.write(b.decode("utf-8"))
                except:
                    self.debugOut.write('?')

    elif ptr == 1:  # looking for START2
        if c != START2:
            self._rxBuf = empty  # failed to find start2
    elif ptr >= HEADER_LEN - 1:  # we've at least got a header
        #logging.debug('at least we received a header')
        # big endian length follows header
        packetlen = (self._rxBuf[2] << 8) + self._rxBuf[3]

        if ptr == HEADER_LEN - 1:  # we _just_ finished reading the header, validate length
            if packetlen > MAX_TO_FROM_RADIO_SIZE:
                self._rxBuf = empty  # length was out out bounds, restart

        if len(self._rxBuf) != 0 and ptr + 1 >= packetlen + HEADER_LEN:
            try:
                self._handleFromRadio(self._rxBuf[HEADER_LEN:])
            except Exception as ex:
                logging.error(f"Error while handling message from radio {ex}")
                traceback.print_exc()
            self._rxBuf = empty
*/