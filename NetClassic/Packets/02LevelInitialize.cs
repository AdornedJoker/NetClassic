using System;
using System.Text;
using SuperSimpleTcp;

namespace NetClassic
{
    public class LevelInitialize : Packets
    {
        public byte[] SendPacket()
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte((byte)ServerPacketTypes.LevelInitialize); //Packet ID
            return ms.ToArray();
        }
    }
}