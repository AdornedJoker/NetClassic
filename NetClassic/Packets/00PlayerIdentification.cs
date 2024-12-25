using System;
using System.Text;
using SuperSimpleTcp;

namespace NetClassic
{
    public class PlayerIdentification : Packets
    {
        public int protocolID;
        public string? username;
        public string? key;

        public PlayerIdentification()
        {
            protocolID = 0;
            username = "";
            key = "";
        }
        
        public void ReadPacket(ArraySegment<byte> data)
        {   
            protocolID = data[1];

            username = ReadWrite.ReadString(data.ToArray(), 2);

            key = ReadWrite.ReadString(data.ToArray(), ReadWrite.GetPrevStringLength(username) + 2);
        }

        public byte[] SendPacket(int protocolID, byte UserType)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte((byte)ClientPacketTypes.PlayerIdentification); //Packet ID
            ms.WriteByte((byte)protocolID); //Protocol
            ms.Write(ReadWrite.WriteString(Globals.serverName)); //Server Name
            ms.Write(ReadWrite.WriteString(Globals.serverMotd)); //Server Motd
            ms.WriteByte(UserType); // User Type
            return ms.ToArray();
        }
    }
}