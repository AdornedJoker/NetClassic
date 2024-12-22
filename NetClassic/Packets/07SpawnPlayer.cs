using System;
using System.Net;
using System.Text;
using SuperSimpleTcp;

namespace NetClassic
{
    public class SpawnPlayer : Packets
    {
        public byte[] SendPacket(sbyte PlayerID, string PlayerName)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte((byte)ServerPacketTypes.SpawnPlayer); //Packet ID
            ms.WriteByte((byte)PlayerID); //Player ID
            ms.Write(ReadWrite.WriteString(PlayerName)); //Player Name
            ms.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)(Globals.world.SpawnX * 32.0)))); //X
            ms.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)(Globals.world.SpawnY * 32.0)))); //Y
            ms.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)(Globals.world.SpawnZ * 32.0)))); //Z
            ms.WriteByte(Globals.world.SpawnLook); //Yaw
            ms.WriteByte(Globals.world.SpawnRotation); //Pitch
            return ms.ToArray();
        }
    }
}