using System;
using System.Text;
using SuperSimpleTcp;

namespace NetClassic
{
    public class DespawnPlayer : Packets
    {
        public static byte[] SendPacket(sbyte PlayerID)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte((byte)ServerPacketTypes.DespawnPlayer); //Packet ID
            ms.WriteByte((byte)PlayerID); //Player ID
            return ms.ToArray();
        }
    }
}