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
        public static async Task SendAllPlayers(byte[] data)
        {
            foreach (var client in Globals.clients)
            {
                if (client.playerClient != null && client.playerClient.Connected)
                {
                    try
                    {
                        await client.playerClient.SendAsync(data);
                        //await Task.Delay(5); // Small delay between clients
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed to send to client {client.id}: {e.Message}");
                        await client.Disconnect();
                    }
                }
            }
        }

        public static async Task SendAllPlayersExcept(int id, byte[] data)
        {
            foreach (var client in Globals.clients)
            {
                if (client.id != id && client.playerClient != null && client.playerClient.Connected)
                {
                    try
                    {
                        await client.playerClient.SendAsync(data);
                        //await Task.Delay(5); // Small delay between clients
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed to send to client {client.id}: {e.Message}");
                        await client.Disconnect();
                    }
                }
            }
        }

        public static async Task Ping(Socket stream)
        {
            try
            {
                await stream.SendAsync(new ArraySegment<byte>(new byte[] { (byte)ServerPacketTypes.Ping }));
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

        public static async Task PlayerIdentification(byte[] networkPacket, Socket stream, int id)
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
                        await stream.SendAsync(disconnectPacket.SendPacket("Illegal name"));
                        _ = Globals.clients[id].Disconnect();
                    }
                }

                if(FileHandle.CheckName(username, Globals.adminsDirectory) == true)
                {
                    Globals.clients[id].UserType = 0x64;
                }

                if (Globals.clients[id].playerClient != null)
                {
                    await stream.SendAsync(packet.SendPacket(packet.protocolID, Globals.clients[id].UserType));

                    LevelInitialize packet2 = new LevelInitialize();

                    await stream.SendAsync(packet2.SendPacket());

                    LevelDataChunk levelDataChunk = new LevelDataChunk();

                    await levelDataChunk.SendPacket(stream);

                    LevelFinalize levelFinalize = new LevelFinalize();

                    await stream.SendAsync(levelFinalize.SendPacket());

                    SpawnPlayer spawnPlayer = new SpawnPlayer();

                    if (username != null)
                    {
                        await stream.SendAsync(spawnPlayer.SendPacket(-1, username));
                    }
                    //Console.WriteLine(id);
                    if (username != null)
                    {
                        await SendAllPlayersExcept(id, spawnPlayer.SendPacket((sbyte)id, username));
                    }
                    //Sending other players to you
                    foreach (var client in Globals.clients)
                    {
                        if(client.id != id && client.playerClient != null)
                        {
                            if (client.username != null)
                            {
                                await stream.SendAsync(spawnPlayer.SendPacket((sbyte)client.id, client.username));
                            }
                            //Console.WriteLine("sent player.");
                        }
                    }    

                    await SendAllPlayers(GameMessage.SendPacket(255, packet.username + " joined the game"));  
                }
            } 
            catch
            {
               await Globals.clients[id].Disconnect();
            }
        }

        public static async Task PositionAndOrientation(byte[] networkPacket, Socket stream, int id)
        {
            try
            {
                await Ping(stream);
                {
                    PositionAndOrientation packet = new PositionAndOrientation();
                    packet.ReadPacket(networkPacket);

                    byte playerId = (byte)id;

                    await SendAllPlayers(packet.SendPacket((sbyte)playerId));    
                }

            }
            catch
            {
                Console.WriteLine("target in sight #3");
                await Globals.clients[id].Disconnect();
            }

        }

        public static async Task MessageHandle(byte[] networkPacket, Socket stream, int id, string username)
        {
            try
            {
                await Ping(stream);
                {
                    GameMessage packet = new GameMessage();
                    packet.ReadPacket(networkPacket);

                    byte playerId = (byte)id;
                    string message = packet.message;

                    bool isCommand = message.TrimStart().StartsWith("/");

                    
                    if(isCommand == false)
                    {
                        await SendAllPlayers(GameMessage.SendPacket(playerId, username+": "+ message));
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
                                await stream.SendAsync(GameMessage.SendPacket(255, "Unknown command!"));
                            }
                        }
                        else
                        {
                            await stream.SendAsync(GameMessage.SendPacket(255, "You're not a server admin!"));
                        }
                    }
                    
                }
            }
            catch
            {
                await Globals.clients[id].Disconnect();
            }
        }

        public static async Task SetBlock(byte[] networkPacket, Socket stream, int id)
        {
            try
            {
                await Ping(stream);
                {
                    Vector3 blockTwoLeft = new Vector3(0, 0, 0);
                    Vector3 blockTwoRight = new Vector3(0, 0, 0);
                    Vector3 blockTwoForward = new Vector3(0, 0, 0);
                    Vector3 blockTwoBackward = new Vector3(0, 0, 0);
                    Vector3 blockTwoTop = new Vector3(0, 0, 0);
                    Vector3 blockTwoBottom = new Vector3(0, 0, 0);

                    
                    Block packet = new();
                    packet.ReadPacket(networkPacket);

                    if(Globals.InBounds(packet.X, packet.Y, packet.Z))
                    {

                        await SendAllPlayers(packet.SendPacket(packet.X, packet.Y, packet.Z, packet.BlockType, packet.Mode));

                        if(packet.BlockType == 19 && packet.Mode == 0x01) //If it's a sponge
                        {
                            _ = Physics.CreateBoundingBox(packet.X, packet.Y, packet.Z, packet.BlockType, packet.Mode);
                        }

                        if(packet.BlockType == 8 || packet.BlockType == 10) //If it's water, check flood
                        {
                            _ = Physics.Flood(packet.X, packet.Y, packet.Z, packet.BlockType, packet.Mode);
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
                        

                        _ = Physics.CompareTwoBlocks(new Vector3(packet.X, packet.Y, packet.Z), blockTwoLeft,
                        blockTwoRight, blockTwoForward, blockTwoBackward, blockTwoTop, blockTwoBottom);
                        
                        //Sand/Gravel Physics
                        if(Globals.getBlockID(packet.X, packet.Y, packet.Z) == 12
                        || Globals.getBlockID(packet.X, packet.Y, packet.Z) == 13)
                        {
                            short newY = await Physics.BlockFall(packet.X, packet.Y, packet.Z, packet.BlockType, packet.Mode);
                            await SendAllPlayers(packet.SendPacket(packet.X, newY, packet.Z, packet.BlockType, packet.Mode));
                        }
                        
                        if(Globals.getBlockID(packet.X, packet.Y-1, packet.Z) == 2)
                        {
                            _ = Physics.BlockChange(new Vector3(packet.X, packet.Y-1, packet.Z), 3, false);
                        }
                        
                        if(Globals.getBlockID(packet.X, packet.Y, packet.Z) == 3)
                        {
                            _ = Physics.BlockChange(new Vector3(packet.X, packet.Y, packet.Z), 3, true);
                        }

                        int checkBlockBelow = Physics.CheckBlockCoveredInt(new Vector3(packet.X, packet.Y, packet.Z));

                        if(Globals.getBlockID(packet.X, checkBlockBelow, packet.Z) == 2)
                        {
                            _ = Physics.BlockChange(new Vector3(packet.X, checkBlockBelow, packet.Z), 3, false);
                        }

                        if(packet.Mode == 0x00)
                        {
                            _ = Physics.MultipleBlockFall(packet.X, packet.Y, packet.Z, packet.BlockType, packet.Mode);
                        }

                    }
                }
            }
            catch
            {
                await Globals.clients[id].Disconnect();
            }
        }
    }
}