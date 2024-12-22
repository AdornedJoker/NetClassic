using System;
using System.Net;
using System.Text;
using SuperSimpleTcp;

namespace NetClassic
{
    public class PositionAndOrientation : Packets
    {

        public short X;
        public short Y;
        public short Z;
        public byte Yaw;
        public byte Pitch;

        public PositionAndOrientation()
        {
            X = 0;
            Y = 0;
            Z = 0;
            Yaw = 0;
            Pitch = 0;
        }

        public void ReadPacket(ArraySegment<byte> data)
        {   
            X = ReadWrite.ReadShort(data.ToArray(), 2);
            Y = ReadWrite.ReadShort(data.ToArray(), 4);
            Z = ReadWrite.ReadShort(data.ToArray(), 6);
            Yaw = data[8];
            Pitch = data[9];
        }
    
        public byte[] SendPacket(sbyte PlayerID)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(0x08); //Packet ID
            ms.WriteByte((byte)PlayerID); //Player ID
            ms.Write(BitConverter.GetBytes(X)); //X
            ms.Write(BitConverter.GetBytes(Y)); //Y
            ms.Write(BitConverter.GetBytes(Z)); //Z
            ms.WriteByte(Yaw); //Yaw
            ms.WriteByte(Pitch); //Pitch
            return ms.ToArray();
        }
    }
}