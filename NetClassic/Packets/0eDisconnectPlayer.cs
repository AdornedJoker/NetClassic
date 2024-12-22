using System;
using System.Text;
using SuperSimpleTcp;

namespace NetClassic
{
    public class DisconnectPlayer : Packets
    {
        public byte[] SendPacket(string message)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte((byte)ServerPacketTypes.DisconnectPlayer); //Packet ID
            ms.Write(ReadWrite.WriteString(message));
            return ms.ToArray();
        }
    }
}