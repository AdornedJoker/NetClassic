using System;
using System.Text;
using SuperSimpleTcp;

namespace NetClassic
{
    public class GameMessage : Packets
    {
        public sbyte PlayerID;
        public string message;

        public GameMessage()
        {
            PlayerID = -1;
            message = "";
        }
        public void ReadPacket(ArraySegment<byte> data)
        {   
            PlayerID = (sbyte)data[1];

            message = ReadWrite.ReadString(data.ToArray(), 2).TrimEnd();
        }

        public static byte[] SendPacket(byte PlayerID, string message)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte((byte)ClientPacketTypes.Message); //Packet ID
            ms.WriteByte((byte)PlayerID); //Player ID
            ms.Write(ReadWrite.WriteString(message)); //Message
            return ms.ToArray();
        }
    }
}