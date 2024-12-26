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
        public Socket? playerClient;
        private MemoryStream buffer = new MemoryStream();
        public bool inGame = false;
        public byte UserType = 0x00; //0x00 -> Not admin, 0x64 -> op
        public async Task Run()
        {
            while(playerClient != null && playerClient.Connected)
            {
                try
                {
                    byte[] data = new byte[1024];
                    int bytesRead = await playerClient.ReceiveAsync(data);
                    if(bytesRead <= 0)
                    {
                        //Console.WriteLine("Player disconnected");
                        _ = Disconnect();
                        return;
                    }
                    buffer.Write(data, 0, bytesRead);
                    buffer.Position = 0;
                    byte[] packet = new byte[buffer.Length];
                    buffer.Read(packet, 0, packet.Length);
                    buffer.SetLength(0);
                    if(packet != null)
                    {
                        await HandlePacket(packet, playerClient, id);
                    }
                }
                catch
                {
                    //Console.WriteLine("Player disconnected");
                    _ = Disconnect();
                    return;
                }
            }
        }

        public async Task Disconnect()
        {
            Console.WriteLine(username + " left the game");
            playerClient?.Close();
            playerClient = null;
            IpAddress = null;
            if(inGame)
            {
                await Task.Run(() => ServerHandle.SendAllPlayers(DespawnPlayer.SendPacket((sbyte)id)));
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
            await Task.Run(() => ServerHandle.SendAllPlayers(GameMessage.SendPacket(255, username + " left the game")));  
        }

        public async Task HandlePacket(byte[] packet, Socket client, int id)
        {
            //Switching between packet types
            switch(packet[0])
            {
                case (byte)ClientPacketTypes.PlayerIdentification:
                    await ServerHandle.PlayerIdentification(packet, client, id);
                    break;
                case (byte)ClientPacketTypes.SetBlock:
                    await ServerHandle.SetBlock(packet, client, id);
                    break;
                case (byte)ClientPacketTypes.Message:
                    await ServerHandle.MessageHandle(packet, client, id, username);
                    break;
                case (byte)ClientPacketTypes.PositionOrientation:
                    await ServerHandle.PositionAndOrientation(packet, client, id);
                    break;
                default:
                    Console.WriteLine("Unknown packet received: "+packet[0]);
                    break;
            }
        }
    }
}