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

        //Physics
        public short BlockFall(short X, short Y, short Z, byte BlockType, byte Mode)
        {
            short sucessY = Y;
            if(BlockType == 12 || BlockType == 13) //Sand or Gravel
            {
                ServerHandle.SendAllPlayers(SendPacket(X, Y, Z, 0x00, Mode));
                while(Globals.world.BlockData[(sucessY * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] == 0 
                || Globals.world.BlockData[(sucessY * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] == 8
                || Globals.world.BlockData[(sucessY * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] == 9)
                {
                    sucessY--;
                }
                return (short)(sucessY + 1);
            }
            return sucessY;
        }


        public byte[] SendPacket(short X, short Y, short Z, byte BlockType, byte Mode)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte((byte)ServerPacketTypes.SetBlock); //Packet ID
            short tempY = BlockFall(X, Y, Z, BlockType, Mode);
            ms.Write(BitConverter.GetBytes(IPAddress.NetworkToHostOrder(X)));
            ms.Write(BitConverter.GetBytes(IPAddress.NetworkToHostOrder(tempY)));
            ms.Write(BitConverter.GetBytes(IPAddress.NetworkToHostOrder(Z))); 
            

            if (Mode == 0x00)
            {
                ms.WriteByte(0);
                //Console.WriteLine(Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X]);
                //SetBlock(X, Y, Z, 0);
                //Console.WriteLine(Globals.world.BlockData[X + (Z * Globals.world.SizeX) + (Y * Globals.world.SizeX * Globals.world.SizeZ)] ); //To see the previous block
                Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] = 0;
            }
            
            if (Mode == 0x01)
            {
                ms.WriteByte(BlockType);
                //SetBlock(X, Y, Z, BlockType);
                //Console.WriteLine(Globals.world.BlockData[(tempY * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] ); //To see the previous block
                Globals.world.BlockData[(tempY * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] = BlockType;
            }
            return ms.ToArray();
        } 
    }
}