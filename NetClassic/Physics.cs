using System;
using System.Numerics;

namespace NetClassic
{
    public class Vector3
    {
        public int X = 0;
        public int Y = 0;
        public int Z = 0;

        public Vector3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public class Physics
    {
        public static Block packet = new Block();
        public static List<Task> TaskList = new List<Task>();

        public static async Task<short> BlockFall(short X, short Y, short Z, byte BlockType, byte Mode)
        {
            short sucessY = Y;
            if(BlockType == 12 || BlockType == 13) //Sand or Gravel
            {
                await ServerHandle.SendAllPlayers(packet.SendPacket(X, Y, Z, 0x00, Mode));
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

        public static async Task MultipleBlockFall(short X, short Y, short Z, byte BlockType, byte Mode)
        {
            for (int i = Y + 1; i < Globals.world.SizeY; i++)
            {
                byte blockTypeTwoTop = Globals.world.BlockData[(i * Globals.world.SizeZ + Z) * Globals.world.SizeX + X];

                if(blockTypeTwoTop != 12 && blockTypeTwoTop != 13)
                {
                    short tempY = (short)(i - 1);
                    Vector3 blockTwoLeft = new Vector3(0, 0, 0);
                    Vector3 blockTwoRight = new Vector3(0, 0, 0);
                    Vector3 blockTwoForward = new Vector3(0, 0, 0);
                    Vector3 blockTwoBackward = new Vector3(0, 0, 0);
                    Vector3 blockTwoTop = new Vector3(0, 0, 0);
                    Vector3 blockTwoBottom = new Vector3(0, 0, 0);
                    bool isLeft = isBlock((short)(X - 1), tempY, Z);
                    bool isRight = isBlock((short)(X + 1), tempY, Z);
                    bool isForward = isBlock(X, tempY, (short)(Z + 1));
                    bool isBackward = isBlock(X, tempY, (short)(Z - 1));
                    bool isTop = isBlock(X, (short)(tempY + 1), Z);
                    bool isBottom = isBlock(X, (short)(tempY - 1), Z);

                    if(isLeft)
                    {
                        blockTwoLeft = new Vector3((short)(X - 1), tempY, Z);
                    }

                    if(isRight)
                    {
                        blockTwoRight = new Vector3((short)(X + 1), tempY, Z);
                    }

                     if(isForward)
                    {
                        blockTwoForward = new Vector3(X, tempY, (short)(Z + 1));
                    }

                    if(isBackward)
                    {
    
                        blockTwoBackward = new Vector3(X, tempY, (short)(Z - 1));
                    }

                    if(isTop)
                    {
                        blockTwoTop = new Vector3(X, (short)(tempY + 1), Z);
                    }

                    if(isBottom)
                    {
                        blockTwoBottom = new Vector3(X, (short)(tempY - 1), Z);
                    }
                    await ServerHandle.SendAllPlayers(packet.SendPacket(X, (short)(i - 1), Z, blockTypeTwoTop, 0x00));
                    await CompareTwoBlocks(new Vector3(X, tempY, Z), blockTwoLeft,
                    blockTwoRight, blockTwoForward, blockTwoBackward, blockTwoTop, blockTwoBottom);
                }

                if(blockTypeTwoTop == 12 || blockTypeTwoTop == 13)
                {
                    //Console.WriteLine(blockTypeTwoTop);
                    await ServerHandle.SendAllPlayers(packet.SendPacket(X, (short)(i - 1), Z, blockTypeTwoTop, 0x01));
                }
                else
                {
                    break;
                }
            }
        }

        public static async Task CreateBoundingBox(short X, short Y, short Z, byte BlockType, byte Mode)
        {
            if(BlockType == 19) //If it's a sponge
            {
                for(short tempX = (short)(X - 2); tempX <= X+2; tempX++)
                {
                    for(short tempY = (short)(Y - 2); tempY <= Y+2; tempY++)
                    {
                        for(short tempZ = (short)(Z - 2); tempZ <= Z+2; tempZ++)
                        {
                            if (Globals.world.BlockData[(tempY * Globals.world.SizeZ + tempZ) * Globals.world.SizeX + tempX] == 8 
                            || Globals.world.BlockData[(tempY * Globals.world.SizeZ + tempZ) * Globals.world.SizeX + tempX] == 9)
                            {
                                await ServerHandle.SendAllPlayers(packet.SendPacket(tempX, tempY, tempZ, 0x00, Mode));
                            }
                        }
                    }
                }
            }
        }

        public static async Task CompareTwoBlocks(Vector3 BlockOne, Vector3 blockTwoLeft,
        Vector3 blockTwoRight, Vector3 blockTwoForward, Vector3 blockTwoBackwards, Vector3 blockTwoTop, Vector3 blockTwoBottom)
        {
            byte blockTypeTwoLeft = Globals.world.BlockData[(BlockOne.Y * Globals.world.SizeZ + BlockOne.Z) * Globals.world.SizeX + (BlockOne.X - 1)];
            byte blockTypeTwoRight = Globals.world.BlockData[(BlockOne.Y * Globals.world.SizeZ + BlockOne.Z) * Globals.world.SizeX + (BlockOne.X + 1)];
            byte blockTypeTwoForward = Globals.world.BlockData[(BlockOne.Y * Globals.world.SizeZ + (BlockOne.Z + 1)) * Globals.world.SizeX + BlockOne.X];
            byte blockTypeTwoBackward = Globals.world.BlockData[(BlockOne.Y * Globals.world.SizeZ + (BlockOne.Z - 1)) * Globals.world.SizeX + BlockOne.X];
            byte blockTypeTwoTop = Globals.world.BlockData[((BlockOne.Y + 1) * Globals.world.SizeZ + BlockOne.Z) * Globals.world.SizeX + BlockOne.X];
            byte blockTypeTwoBottom = Globals.world.BlockData[((BlockOne.Y - 1) * Globals.world.SizeZ + BlockOne.Z) * Globals.world.SizeX + BlockOne.X];
            
            //Console.WriteLine("Left: " + blockTypeTwoLeft);
            //Console.WriteLine("Right: " + blockTypeTwoRight);
            //Console.WriteLine("Forward: " + blockTypeTwoForward);
            //Console.WriteLine("Backward: " + blockTypeTwoBackward);
            //Console.WriteLine("Top: " + blockTypeTwoTop);
            //Console.WriteLine("Bottom: " + blockTypeTwoBottom);

            if(blockTypeTwoLeft == 8 && !blockTwoLeft.Equals(new Vector3(0, 0, 0)) 
            || blockTypeTwoLeft == 9 && !blockTwoLeft.Equals(new Vector3(0, 0, 0))
            || blockTypeTwoLeft == 10 && !blockTwoLeft.Equals(new Vector3(0, 0, 0)) 
            || blockTypeTwoLeft == 11 && !blockTwoLeft.Equals(new Vector3(0, 0, 0)))
            {
                Task.Run(() => Flood((short)blockTwoLeft.X, (short)blockTwoLeft.Y, (short)blockTwoLeft.Z, blockTypeTwoLeft, 0x01));
            }

            if(blockTypeTwoRight == 8 && !blockTwoRight.Equals(new Vector3(0, 0, 0)) 
            || blockTypeTwoRight == 9 && !blockTwoRight.Equals(new Vector3(0, 0, 0))
            || blockTypeTwoRight == 10 && !blockTwoRight.Equals(new Vector3(0, 0, 0)) 
            || blockTypeTwoRight == 11 && !blockTwoRight.Equals(new Vector3(0, 0, 0)))
            {
                //Console.WriteLine(blockTypeTwoLeft);
                Task.Run(() => Flood((short)blockTwoRight.X, (short)blockTwoRight.Y, (short)blockTwoRight.Z, blockTypeTwoRight, 0x01));
            }
            if(blockTypeTwoForward == 8 && !blockTwoForward.Equals(new Vector3(0, 0, 0)) 
            || blockTypeTwoForward == 9 && !blockTwoForward.Equals(new Vector3(0, 0, 0))
            || blockTypeTwoForward == 10 && !blockTwoForward.Equals(new Vector3(0, 0, 0)) 
            || blockTypeTwoForward == 11 && !blockTwoForward.Equals(new Vector3(0, 0, 0)))
            {
                //Console.WriteLine(blockTypeTwoLeft);
                Task.Run(() => Flood((short)blockTwoForward.X, (short)blockTwoForward.Y, (short)blockTwoForward.Z, blockTypeTwoForward, 0x01));
            }
            if(blockTypeTwoBackward == 8 && !blockTwoBackwards.Equals(new Vector3(0, 0, 0)) 
            || blockTypeTwoBackward == 9 && !blockTwoBackwards.Equals(new Vector3(0, 0, 0))
            || blockTypeTwoBackward == 10 && !blockTwoBackwards.Equals(new Vector3(0, 0, 0)) 
            || blockTypeTwoBackward == 11 && !blockTwoBackwards.Equals(new Vector3(0, 0, 0)))
            {
                //Console.WriteLine(blockTypeTwoLeft);
                Task.Run(() => Flood((short)blockTwoBackwards.X, (short)blockTwoBackwards.Y, (short)blockTwoBackwards.Z, blockTypeTwoBackward, 0x01));
            }
            if(blockTypeTwoTop == 8 && !blockTwoTop.Equals(new Vector3(0, 0, 0)) 
            || blockTypeTwoTop == 9 && !blockTwoTop.Equals(new Vector3(0, 0, 0))
            || blockTypeTwoTop == 10 && !blockTwoTop.Equals(new Vector3(0, 0, 0)) 
            || blockTypeTwoTop == 11 && !blockTwoTop.Equals(new Vector3(0, 0, 0)))
            {
                //Console.WriteLine(blockTypeTwoLeft);
                Task.Run(() => Flood((short)blockTwoTop.X, (short)blockTwoTop.Y, (short)blockTwoTop.Z, blockTypeTwoTop, 0x01));
            }
            if(blockTypeTwoBottom == 8 && !blockTwoBottom.Equals(new Vector3(0, 0, 0)) 
            || blockTypeTwoBottom == 9 && !blockTwoBottom.Equals(new Vector3(0, 0, 0))
            || blockTypeTwoBottom == 10 && !blockTwoBottom.Equals(new Vector3(0, 0, 0)) 
            || blockTypeTwoBottom == 11 && !blockTwoBottom.Equals(new Vector3(0, 0, 0)))
            {
                //Console.WriteLine(blockTypeTwoLeft);
                Task.Run(() => Flood((short)blockTwoBottom.X, (short)blockTwoBottom.Y, (short)blockTwoBottom.Z, blockTypeTwoBottom, 0x01));
            }

        }

        public static async Task BlockChange(Vector3 BlockTwo, byte newBlockID, bool loopNow)
        {
            Random milliSeconds = new Random();
            Vector3 blockTwoTop = new Vector3(BlockTwo.X, (short)(BlockTwo.Y + 1), BlockTwo.Z);

            if(loopNow == false)
            {
                await Task.Delay(milliSeconds.Next(0, 30000));  
            }

            if(CheckBlockCovered(BlockTwo))
            {
                await ServerHandle.SendAllPlayers(packet.SendPacket((short)BlockTwo.X, (short)BlockTwo.Y, (short)BlockTwo.Z, newBlockID, 0x01));  
           
                while(true)
                {

                    ///THIS FUNCTION WILL BE SEPERATE AND INSTEAD WILL COUNT THE BLOCKS IN THE AIR
                    await Task.Delay(10000);
                    byte blockTypeTwoTop = Globals.world.BlockData[(blockTwoTop.Y * Globals.world.SizeZ + blockTwoTop.Z) * Globals.world.SizeX + blockTwoTop.X];
                    
                    if(CheckBlockCovered(BlockTwo))
                    {
                        //Console.WriteLine(blockTypeTwoTop);
                        continue;
                    } else {
                        await ChangeDirt2Grass(BlockTwo);
                        break;
                    }
                }
            }
            else
            {
                //Console.WriteLine("Cancling grass to dirt.");
                byte blockType = Globals.world.BlockData[(BlockTwo.Y * Globals.world.SizeZ + BlockTwo.Z) * Globals.world.SizeX + BlockTwo.X];
                if(blockType == 3)
                {
                    await ChangeDirt2Grass(BlockTwo);
                }
                return; 
            }


        }

        public static bool CheckBlockCovered(Vector3 BlockOne)
        {
            for (int i = BlockOne.Y + 1; i < Globals.world.SizeY; i++)
            {
                byte blockTypeTwoTop = Globals.world.BlockData[(i * Globals.world.SizeZ + BlockOne.Z) * Globals.world.SizeX + BlockOne.X];

                if(blockTypeTwoTop != 0)
                {
                    if(blockTypeTwoTop == 18)
                    {
                        continue;
                    }
                    else if(blockTypeTwoTop == 20)
                    {
                        continue;
                    }
                    else
                    {
                        return true;
                    }
                    
                }
                else
                {
                    continue;
                }
            }

            return false;
        }

        public static int CheckBlockCoveredInt(Vector3 BlockOne)
        {
            int failSafe = 0;
            for (int i = BlockOne.Y - 1; i > 0; i--)
            {
                failSafe = i;
                byte blockTypeTwoTop = Globals.world.BlockData[(i * Globals.world.SizeZ + BlockOne.Z) * Globals.world.SizeX + BlockOne.X];

                if(blockTypeTwoTop != 0)
                {
                    if(blockTypeTwoTop == 18)
                    {
                        continue;
                    }
                    else if(blockTypeTwoTop == 20)
                    {
                        continue;
                    }
                    else
                    {
                        return i; //Return Y
                    }
                    
                }
                else
                {
                    continue;
                }
            } 

            return failSafe;
        }

        public static async Task ChangeDirt2Grass(Vector3 BlockTwo)
        {
            Random milliSeconds = new Random();

            await Task.Delay(milliSeconds.Next(0, 30000));

            if(!CheckBlockCovered(BlockTwo) && Globals.world.BlockData[(BlockTwo.Y * Globals.world.SizeZ + BlockTwo.Z) * Globals.world.SizeX + BlockTwo.X] != 0)
            {
                await ServerHandle.SendAllPlayers(packet.SendPacket((short)BlockTwo.X, (short)BlockTwo.Y, (short)BlockTwo.Z, 2, 0x01));
            }
            else
            {
                if(Globals.world.BlockData[(BlockTwo.Y * Globals.world.SizeZ + BlockTwo.Z) * Globals.world.SizeX + BlockTwo.X] != 0)
                {
                    await BlockChange(new Vector3(BlockTwo.X, BlockTwo.Y, BlockTwo.Z), 3, false);
                }
            }
        }

        public static async Task<bool> isAir(short X, short Y, short Z, bool isLava)
        {
            if (Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] == 0 && 
            Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] != 8
            && Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] != 9 
            || Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] == 37
            || Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] == 38
            || Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] == 39
            || Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] == 40)
            {
                return true;
            }

            //Turn lava to stone
            if (Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] == 10 && isLava == false
            || Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] == 11
            && isLava == false)
            {
                await ServerHandle.SendAllPlayers(packet.SendPacket(X, Y, Z, 1, 0x01));
                return false;
            }

            if (Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] == 8 && isLava == true
            || Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] == 9
            && isLava == true)
            {
                await ServerHandle.SendAllPlayers(packet.SendPacket(X, Y, Z, 1, 0x01));
                return false;
            }

            return false;
        }

       public static bool isBlock(short X, short Y, short Z)
        {
            if (Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] != 0)
            {
                return true;
            }

            return false;
        }
        

        //Information: Flood methods should be removed from the list. Not sure how to do that though. Will be resolved later down
        // -the line.
        public static async Task Flood(short X, short Y, short Z, byte BlockType, byte Mode)
        {
           if (Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] == 8 
            || Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] == 9
            || Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] == 10
            || Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X] == 11)
            {
                bool isLava = false;
                bool continueFlood = true;

                short tempX = X;
                short tempZ = Z;
                short tempY = Y;
                int milliSeconds = 300;

                byte tempType = Globals.world.BlockData[(Y * Globals.world.SizeZ + Z) * Globals.world.SizeX + X];

                //It's water
                if(tempType == 9)
                {
                   isLava= false;
                   tempType = 8; 
                }
                //It's lava
                if(tempType == 11 || tempType == 10)
                {
                    isLava = true;
                    tempType = 10; 
                    milliSeconds = 1300;     
                }

                //Console.WriteLine("Preparing.");

                while(continueFlood)
                {
                    await Task.Delay(milliSeconds);
                    bool isLeft =  await isAir((short)(tempX - 1), tempY, tempZ, isLava);
                    bool isRight =  await isAir((short)(tempX + 1), tempY, tempZ, isLava);
                    bool isForward =  await isAir(tempX, tempY, (short)(tempZ + 1), isLava);
                    bool isBackward =  await isAir(tempX, tempY, (short)(tempZ - 1), isLava);
                    bool isBottom =  await isAir(tempX, (short)(tempY - 1), tempZ, isLava);
                    
                    Vector3 blockTwoBottom = new Vector3(tempX, (short)(tempY - 1), tempZ);

                    if(!isLeft && !isRight && !isForward && !isBackward && !isBottom)
                    {
                        //Console.WriteLine("Stopping Flood.");
                        break;
                    }


                    //REDO CHECK OF MAP BORDER AGAIN LATER
                    if(tempX > Globals.world.SizeX)
                    {
                        //Console.WriteLine("Stopping Flood due to reached map border.");
                        break;
                    }
                    else if(tempY >= Globals.world.SizeY)
                    {
                        break;
                    }
                    else if( tempZ >= Globals.world.SizeZ)
                    {
                        break;
                    }
                    else if (tempX <= 0)
                    {
                        break;
                    }
                    else if (tempY <= 0)
                    {
                        break;
                    }
                    else if (tempZ <= 0)
                    {
                        break;
                    }
                    else if (tempZ <= 0 && tempX <= 0 || tempZ >= Globals.world.SizeZ && tempX >= Globals.world.SizeX)
                    {
                        break;
                    }

                    if(isLeft){tempX = (short)(tempX - 1); await ServerHandle.SendAllPlayers(packet.SendPacket(tempX, tempY, tempZ, tempType, Mode)); var LastTask = Flood(tempX, tempY, tempZ, tempType, Mode); TaskList.Add(LastTask); tempX = X;}
                    if(isRight){tempX = (short)(tempX + 1); await ServerHandle.SendAllPlayers(packet.SendPacket(tempX, tempY, tempZ, tempType, Mode)); var LastTask = Flood(tempX, tempY, tempZ, tempType, Mode); TaskList.Add(LastTask); tempX = X;}
                    if(isForward){tempZ = (short)(tempZ + 1); await ServerHandle.SendAllPlayers(packet.SendPacket(tempX, tempY, tempZ, tempType, Mode)); var LastTask = Flood(tempX, tempY, tempZ, tempType, Mode); TaskList.Add(LastTask); tempZ = Z;}
                    if(isBackward){tempZ = (short)(tempZ - 1); await ServerHandle.SendAllPlayers(packet.SendPacket(tempX, tempY, tempZ, tempType, Mode)); var LastTask = Flood(tempX, tempY, tempZ, tempType, Mode); TaskList.Add(LastTask); tempZ = Z;}
                    if(isBottom){tempY = (short)(tempY - 1); await ServerHandle.SendAllPlayers(packet.SendPacket(tempX, tempY, tempZ, tempType, Mode)); var LastTask = Flood(tempX, tempY, tempZ, tempType, Mode); TaskList.Add(LastTask); tempY = Y;}
                    
                }
            }
        }    
    }
}