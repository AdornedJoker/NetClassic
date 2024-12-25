using System;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using SuperSimpleTcp;

namespace NetClassic
{
    public class ServerHandle
    {
        public static void SendAllPlayers(byte[] data)
        {

            foreach (var client in Globals.clients)
            {
                if (client.stream != null && client.playerClient != null)
                {
                    try
                    {
                        client.stream.WriteAsync(data);
                    }
                    catch
                    {
                        Console.WriteLine("target in  #1");
                        continue;
                    }
                }
            }
        }

        public static void SendAllPlayersExcept(int id, byte[] data)
        {
            for (int i = 0; i < Globals.clients.Count; i++)
            {
                var client = Globals.clients[i];
                if (client.id != id && client.stream != null && client.playerClient != null)
                {       
                    try
                    {
                        client.stream.WriteAsync(data);
                    }
                    catch
                    {
                        Console.WriteLine("target in sight #2");
                    }
                }
            }
        }

        public static void Ping(Stream stream, int id)
        {
            try
            {
                stream.WriteByte((byte)ServerPacketTypes.Ping);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static string CreateMD5(string input)
        {
            MD5 mD5 = MD5.Create();

            byte[] hashBytes = mD5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexStringLower(hashBytes);
        }

        public static async Task PlayerIdentification(byte[] networkPacket, NetworkStream stream, int id)
        {
            try
            {
                PlayerIdentification packet = new PlayerIdentification();
                packet.ReadPacket(networkPacket);

                int protocolID = packet.protocolID;
                string? username = packet.username;
                string? key = packet.key;

                //Console.WriteLine("Protocol ID: " + protocolID);
                //Console.WriteLine("Username: " + username);
                //Console.WriteLine("Key: " + key);

                foreach (var client in Globals.clients)
                {
                    if (client.id == id)
                    {
                        client.username = username;
                        //client.id = id;
                        client.inGame = true;
                        Console.WriteLine(client.IpAddress + " is logging on as "+username+"!");
                        break;
                    }
                }

                //If the key is empty, it's a local connection.
                if(key != "" && Globals.nameVerfication)
                {
                    //Name verification.
                    if(key == CreateMD5(Globals.salt + username))
                    {
                        Console.WriteLine(username + " logged on to ClassiCube!");
                    } 
                    else 
                    {
                        DisconnectPlayer disconnectPacket = new DisconnectPlayer();
                        await stream.WriteAsync(disconnectPacket.SendPacket("Illegal name"));
                        Globals.clients[id].Disconnect();
                    }
                }

                if (Globals.clients[id].playerClient != null)
                {
                    await stream.WriteAsync(packet.SendPacket(packet.protocolID, Globals.clients[id].UserType));

                    LevelInitialize packet2 = new LevelInitialize();

                    await stream.WriteAsync(packet2.SendPacket());

                    LevelDataChunk levelDataChunk = new LevelDataChunk();

                    await levelDataChunk.SendPacket(stream);

                    LevelFinalize levelFinalize = new LevelFinalize();

                    await stream.WriteAsync(levelFinalize.SendPacket());

                    SpawnPlayer spawnPlayer = new SpawnPlayer();

                    await stream.WriteAsync(spawnPlayer.SendPacket(-1, username));
                    //Console.WriteLine(id);
                    SendAllPlayersExcept(id, spawnPlayer.SendPacket((sbyte)id, username));
                    //Sending other players to you
                    foreach (var client in Globals.clients)
                    {
                        if(client.id != id && client.playerClient != null)
                        {
                            await stream.WriteAsync(spawnPlayer.SendPacket((sbyte)client.id, client.username));
                            //Console.WriteLine("sent player.");
                        }
                    }    

                    SendAllPlayers(GameMessage.SendPacket(255, packet.username + " joined the game"));  
                }
            } 
            catch
            {
               Globals.clients[id].Disconnect();
            }
        }

        public static async Task PositionAndOrientation(byte[] networkPacket, NetworkStream stream, int id)
        {
            try
            {
                Ping(stream, id);
                {
                    PositionAndOrientation packet = new PositionAndOrientation();
                    packet.ReadPacket(networkPacket);

                    byte playerId = (byte)id;

                    await Task.Run(() => SendAllPlayers(packet.SendPacket((sbyte)playerId)));    
                }

            }
            catch
            {
                Console.WriteLine("target in sight #3");
                Globals.clients[id].Disconnect();
            }

        }

        public static async Task MessageHandle(byte[] networkPacket, NetworkStream stream, int id, string username)
        {
            try
            {
                Ping(stream, id);
                {
                    GameMessage packet = new GameMessage();
                    packet.ReadPacket(networkPacket);

                    byte playerId = (byte)id;
                    string message = packet.message;

                    bool isCommand = message.TrimStart().StartsWith("/");

                    
                    if(isCommand == false)
                    {
                        SendAllPlayers(GameMessage.SendPacket(playerId, username+": "+ message));
                        Console.WriteLine(username+" says: "+message); 
                    }
                    else
                    {
                        if(Globals.clients[id].UserType == 0x64) //This user is operator.
                        {
                            string adminCommand = message.TrimStart().ToLower().Substring(1);
                            if(Regex.IsMatch(adminCommand, @"\bop\b"))
                            {
                                Console.WriteLine(username+" admins: "+adminCommand);
                                OpFunction handleOP = new OpFunction();
                                await handleOP.HandleCommand(adminCommand, stream);
                            }
                            else if(Regex.IsMatch(adminCommand, @"\bdeop\b"))
                            {
                                Console.WriteLine(username+" admins: "+adminCommand);
                                DeOpFunction handleDeOP = new DeOpFunction();
                                await handleDeOP.HandleCommand(adminCommand, stream);
                            }
                            else if(Regex.IsMatch(adminCommand, @"\bkick\b"))
                            {
                                Console.WriteLine(username+" admins: "+adminCommand);
                                KickFunction handleKick = new KickFunction();
                                await handleKick.HandleCommand(adminCommand, stream);
                            }
                            else if(Regex.IsMatch(adminCommand, @"\bsay\b"))
                            {
                                Console.WriteLine(username+" admins: "+adminCommand);
                                BroadcastFunction handleBroadCast = new BroadcastFunction();
                                await handleBroadCast.HandleCommand(adminCommand, true);
                            }
                            else if(Regex.IsMatch(adminCommand, @"\bbroadcast\b"))
                            {
                                Console.WriteLine(username+" admins: "+adminCommand);
                                BroadcastFunction handleBroadCast = new BroadcastFunction();
                                await handleBroadCast.HandleCommand(adminCommand, false);
                            }
                            else
                            {
                                await stream.WriteAsync(GameMessage.SendPacket(255, "Unknown command!"));
                            }
                        }
                        else
                        {
                            await stream.WriteAsync(GameMessage.SendPacket(255, "You're not a server admin!"));
                        }
                    }
                    
                }
            }
            catch
            {
                Globals.clients[id].Disconnect();
            }
        }

        public static async Task SetBlock(byte[] networkPacket, NetworkStream stream, int id)
        {
            try
            {
                Ping(stream, id);
                {
                    Vector3 blockTwoLeft = new Vector3(0, 0, 0);
                    Vector3 blockTwoRight = new Vector3(0, 0, 0);
                    Vector3 blockTwoForward = new Vector3(0, 0, 0);
                    Vector3 blockTwoBackward = new Vector3(0, 0, 0);
                    Vector3 blockTwoTop = new Vector3(0, 0, 0);
                    Vector3 blockTwoBottom = new Vector3(0, 0, 0);

                    
                    Block packet = new();
                    Physics physics = new();
                    packet.ReadPacket(networkPacket);

                    SendAllPlayers(packet.SendPacket(packet.X, packet.Y, packet.Z, packet.BlockType, packet.Mode));

                    if(packet.BlockType == 19 && packet.Mode == 0x01) //If it's a sponge
                    {
                        physics.CreateBoundingBox(packet.X, packet.Y, packet.Z, packet.BlockType, packet.Mode);
                    }

                    if(packet.BlockType == 8 || packet.BlockType == 10) //If it's water, check flood
                    {
                       physics.Flood(packet.X, packet.Y, packet.Z, packet.BlockType, packet.Mode);
                    }

                    bool isLeft = Physics.isBlock((short)(packet.X - 1), packet.Y, packet.Z);
                    bool isRight = Physics.isBlock((short)(packet.X + 1), packet.Y, packet.Z);
                    bool isForward = Physics.isBlock(packet.X, packet.Y, (short)(packet.Z + 1));
                    bool isBackward = Physics.isBlock(packet.X, packet.Y, (short)(packet.Z - 1));
                    bool isTop = Physics.isBlock(packet.X, (short)(packet.Y + 1), packet.Z);
                    bool isBottom = Physics.isBlock(packet.X, (short)(packet.Y - 1), packet.Z);

                    if(isLeft)
                    {
                        blockTwoLeft = new Vector3((short)(packet.X - 1), packet.Y, packet.Z);
                    }

                    if(isRight)
                    {
                        blockTwoRight = new Vector3((short)(packet.X + 1), packet.Y, packet.Z);
                    }

                     if(isForward)
                    {
                        blockTwoForward = new Vector3(packet.X, packet.Y, (short)(packet.Z + 1));
                    }

                    if(isBackward)
                    {
    
                        blockTwoBackward = new Vector3(packet.X, packet.Y, (short)(packet.Z - 1));
                    }

                    if(isTop)
                    {
                        blockTwoTop = new Vector3(packet.X, (short)(packet.Y + 1), packet.Z);
                    }

                    if(isBottom)
                    {
                        blockTwoBottom = new Vector3(packet.X, (short)(packet.Y - 1), packet.Z);
                    }

                    await physics.CompareTwoBlocks(new Vector3(packet.X, packet.Y, packet.Z), blockTwoLeft,
                    blockTwoRight, blockTwoForward, blockTwoBackward, blockTwoTop, blockTwoBottom);

                    //Sand/Gravel Physics
                    if(Globals.world.BlockData[(packet.Y * Globals.world.SizeZ + packet.Z) * Globals.world.SizeX + packet.X] == 12
                    || Globals.world.BlockData[(packet.Y * Globals.world.SizeZ + packet.Z) * Globals.world.SizeX + packet.X] == 13)
                    {
                        short newY = physics.BlockFall(packet.X, packet.Y, packet.Z, packet.BlockType, packet.Mode);
                        SendAllPlayers(packet.SendPacket(packet.X, newY, packet.Z, packet.BlockType, packet.Mode));
                    }

                    if(Globals.world.BlockData[((packet.Y-1) * Globals.world.SizeZ + packet.Z) * Globals.world.SizeX + packet.X] == 2)
                    {
                        physics.BlockChange(new Vector3(packet.X, packet.Y-1, packet.Z), 3, false);
                    }

                    if(Globals.world.BlockData[(packet.Y * Globals.world.SizeZ + packet.Z) * Globals.world.SizeX + packet.X] == 3)
                    {
                        physics.BlockChange(new Vector3(packet.X, packet.Y, packet.Z), 3, true);
                    }

                    int checkBlockBelow = physics.CheckBlockCoveredInt(new Vector3(packet.X, packet.Y, packet.Z));

                    if(Globals.world.BlockData[(checkBlockBelow * Globals.world.SizeZ + packet.Z) * Globals.world.SizeX + packet.X] == 2)
                    {
                        physics.BlockChange(new Vector3(packet.X, checkBlockBelow, packet.Z), 3, false);
                    }

                    if(packet.Mode == 0x00)
                    {
                        physics.MultipleBlockFall(packet.X, packet.Y, packet.Z, packet.BlockType, packet.Mode);
                    }

                }
            }
            catch
            {
                Globals.clients[id].Disconnect();
            }
        }
    }
}