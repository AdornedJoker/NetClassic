using System;
using System.Net;
using System.Text;
using SuperSimpleTcp;

namespace NetClassic
{
    public class Block : Packets
    {
        public short X;
        public short Y;
        public short Z;
        public byte Mode;
        public byte BlockType;
        
        public void ReadPacket(ArraySegment<byte> data)
        { 
            Mode = data[7];
            BlockType = data[8];  
            X = data[2];
            Y = data[4];
            Z = data[6];
        }


        public byte[] SendPacket(short X, short Y, short Z, byte BlockType, byte Mode)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte((byte)ServerPacketTypes.SetBlock); //Packet ID
            ms.Write(BitConverter.GetBytes(IPAddress.NetworkToHostOrder(X)));
            ms.Write(BitConverter.GetBytes(IPAddress.NetworkToHostOrder(Y)));
            ms.Write(BitConverter.GetBytes(IPAddress.NetworkToHostOrder(Z))); 
            

            if (Mode == 0x00)
            {
                ms.WriteByte(0);
                Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] = 0;
            }
            
            if (Mode == 0x01)
            {
                ms.WriteByte(BlockType);
                Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] = BlockType;
            }
            return ms.ToArray();
        } 
    }
}