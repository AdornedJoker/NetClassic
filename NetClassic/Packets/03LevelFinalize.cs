using System;
using System.Net;
using System.Text;
using SuperSimpleTcp;

namespace NetClassic
{
    public class LevelFinalize : Packets
    {
        public byte[] SendPacket()
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte((byte)ServerPacketTypes.LevelFinalize); //Packet ID
            ms.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Globals.world.SizeX)));
            ms.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Globals.world.SizeY)));
            ms.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Globals.world.SizeZ)));
            return ms.ToArray();
        }
    }
}