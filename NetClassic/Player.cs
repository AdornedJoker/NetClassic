using System;
using System.Net.Sockets;
using System.Text;
using SuperSimpleTcp;

namespace NetClassic
{
    public class Client
    {
        public int id;
        public string? username;
        public string? IpAddress;
        public TcpClient? playerClient;
        private MemoryStream buffer = new MemoryStream();
        public NetworkStream? stream;
        public bool inGame = false;
        public async Task Run()
        {
            while(playerClient != null && playerClient.Connected)
            {
                try
                {
                    byte[] data = new byte[1024];
                    int bytesRead = await playerClient.GetStream().ReadAsync(data, 0, data.Length);
                    stream = playerClient.GetStream();
                    if(bytesRead <= 0)
                    {
                        Console.WriteLine("Player disconnected");
                        Disconnect();
                        return;
                    }
                    buffer.Write(data, 0, bytesRead);
                    buffer.Position = 0;
                    byte[] packet = new byte[buffer.Length];
                    buffer.Read(packet, 0, packet.Length);
                    buffer.SetLength(0);
                    if(packet != null)
                    {
                        await HandlePacket(packet, playerClient.GetStream(), id);
                    }
                }
                catch
                {
                    Console.WriteLine("Player disconnected");
                    Disconnect();
                    return;
                }
            }
        }

        public void Disconnect()
        {
            playerClient?.Close();
            playerClient = null;
            IpAddress = null;
            if(inGame)
            {
                ServerHandle.SendAllPlayers(DespawnPlayer.SendPacket((sbyte)id));
            }
            //Move all players down one.
            /*
            foreach(var client in Globals.clients)
            {
                if(client.id > id)
                {
                    client.id--;
                }
            }
            */
            ServerHandle.SendAllPlayers(GameMessage.SendPacket(255, username + " left the game"));  
        }

        public async Task HandlePacket(byte[] packet, NetworkStream stream, int id)
        {
            //Switching between packet types
            switch(packet[0])
            {
                case (byte)ClientPacketTypes.PlayerIdentification:
                    await ServerHandle.PlayerIdentification(packet, stream, id);
                    break;
                case (byte)ClientPacketTypes.SetBlock:
                    await ServerHandle.SetBlock(packet, stream, id);
                    break;
                case (byte)ClientPacketTypes.PositionOrientation:
                    await ServerHandle.PositionAndOrientation(packet, stream, id);
                    break;
                case (byte)ClientPacketTypes.Message:
                    await ServerHandle.MessageHandle(packet, stream, id, username);
                    break;
                default:
                    Console.WriteLine("Unknown packet received");
                    break;
            }
        }
    }
}