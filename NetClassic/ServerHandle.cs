using System;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
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

                Console.WriteLine("Protocol ID: " + protocolID);
                Console.WriteLine("Username: " + username);
                Console.WriteLine("Key: " + key);

                foreach (var client in Globals.clients)
                {
                    if (client.id == id)
                    {
                        client.username = username;
                        //client.id = id;
                        client.inGame = true;
                        Console.WriteLine(client.id);
                        break;
                    }
                }

                //If the key is empty, it's a local connection.
                if(key != "" && Globals.nameVerfication)
                {
                    //Name verification.
                    if(key == CreateMD5(Globals.salt + username))
                    {
                        Console.WriteLine("This person logged on to ClassiCube!");
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
                    await stream.WriteAsync(packet.SendPacket(packet.protocolID));

                    LevelInitialize packet2 = new LevelInitialize();

                    await stream.WriteAsync(packet2.SendPacket());

                    LevelDataChunk levelDataChunk = new LevelDataChunk();

                    await levelDataChunk.SendPacket(stream);

                    LevelFinalize levelFinalize = new LevelFinalize();

                    await stream.WriteAsync(levelFinalize.SendPacket());

                    SpawnPlayer spawnPlayer = new SpawnPlayer();

                    await stream.WriteAsync(spawnPlayer.SendPacket(-1, username));
                    Console.WriteLine(id);
                    SendAllPlayersExcept(id, spawnPlayer.SendPacket((sbyte)id, username));
                    //Sending other players to you
                    foreach (var client in Globals.clients)
                    {
                        if(client.id != id && client.playerClient != null)
                        {
                            await stream.WriteAsync(spawnPlayer.SendPacket((sbyte)client.id, client.username));
                            Console.WriteLine("sent player.");
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

                    SendAllPlayers(GameMessage.SendPacket(playerId, username+": "+ message));
                    Console.WriteLine(username+" says: "+message);
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
                    Block packet = new();
                    packet.ReadPacket(networkPacket);

                    SendAllPlayers(packet.SendPacket(packet.X, packet.Y, packet.Z, packet.BlockType, packet.Mode));
                }
            }
            catch
            {
                Globals.clients[id].Disconnect();
            }
        }
    }
}