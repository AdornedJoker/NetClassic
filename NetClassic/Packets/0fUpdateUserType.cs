using System;
using System.Net;
using System.Text;
using SuperSimpleTcp;

namespace NetClassic
{
    public class UpdateUserType : Packets
    {
        public byte[] SendPacket(byte toUserType)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte((byte)ServerPacketTypes.UpdateUserType); //Packet ID
            ms.WriteByte(toUserType);
            return ms.ToArray();
        }
    }
}