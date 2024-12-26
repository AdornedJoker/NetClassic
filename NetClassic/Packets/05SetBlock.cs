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
                if(Globals.InBounds(X, Y, Z))
                {
                    ms.WriteByte(0);
                    Globals.world.BlockData[Globals.GetBlockIndex(X, Y, Z)] = 0;
                    while(Globals.world.BlockData[Globals.GetBlockIndex(X, Y, Z)] != 0)
                    {
                        Globals.world.BlockData[Globals.GetBlockIndex(X, Y, Z)] = 0;
                    }
                }
                else
                {
                    //We return the same value.
                    ms.WriteByte(Globals.world.BlockData[Globals.GetBlockIndex(X, Y, Z)]);
                }
            }
            
            if (Mode == 0x01)
            {
                if(Globals.InBounds(X, Y, Z))
                {
                    ms.WriteByte(BlockType);
                    Globals.world.BlockData[Globals.GetBlockIndex(X, Y, Z)] = BlockType;
                    while(Globals.world.BlockData[Globals.GetBlockIndex(X, Y, Z)] != BlockType)
                    {
                        Globals.world.BlockData[Globals.GetBlockIndex(X, Y, Z)] = BlockType;
                    }
                }
                else
                {
                    //We return the same value.
                    ms.WriteByte(Globals.world.BlockData[Globals.GetBlockIndex(X, Y, Z)]);
                }
            }
            return ms.ToArray();
        } 
    }
}