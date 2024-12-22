using System;
using SuperSimpleTcp;
using ClassicWorld.NET;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;

namespace NetClassic
{
    public class Globals
    {
        public static List<Client> clients = new List<Client>();
        
        public static Classicworld world = new Classicworld("F:\\Project\\NetClassic\\serverLevel.cw");

        public static int MaxPlayers = 16;

        public static string salt = "92B4w7t0kF9m8G5r";

        public static bool isOnline = false;

        public static TcpListener server = new TcpListener(IPAddress.Parse("192.168.1.103"), 25565);
    }

    internal class Program
    {
        public static string heartbeatServer = "http://www.classicube.net/server/heartbeat/";
        private static readonly HttpClient client = new HttpClient();

        public class Response
        {
            public List<List<string>> errors { get; set; }
            public string response { get; set; }
            public string status { get; set; }
        }
        public static async Task SendServerHeartbeat()
        {   
            int onlinePlayers = 0;
            var values = new Dictionary<string, string>
            {
                { "name", "Tad's Classic Server" },
                { "port", "25565" },
                { "users", "0" },
                { "max", Globals.MaxPlayers.ToString() },
                { "public", Globals.isOnline.ToString() },
                { "salt", Globals.salt },
                { "software", "&cNetClassic &av0.1" },
                { "web", true.ToString() },
            };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(heartbeatServer, content);
            var responseString = await response.Content.ReadAsStringAsync();
            
            if(responseString != null)
            {
                Console.WriteLine(responseString);
            }

            while(true)
            {
                await Task.Delay(45000);
                for(int i = 0; i < Globals.MaxPlayers; i++)
                {
                    if (Globals.clients[i].playerClient != null)
                    {
                        onlinePlayers++;
                    }
                }

                values = new Dictionary<string, string>
                {
                    { "name", "Tad's Classic Server" },
                    { "port", "25565" },
                    { "users", onlinePlayers.ToString() },
                    { "max", Globals.MaxPlayers.ToString() },
                    { "public", Globals.isOnline.ToString() },
                    { "salt", Globals.salt },
                    { "software", "&cNetClassic &av0.1" },
                    { "web", true.ToString() },
                };
                content = new FormUrlEncodedContent(values);
                await client.PostAsync(heartbeatServer, content);
                response = await client.PostAsync(heartbeatServer, content);
                responseString = await response.Content.ReadAsStringAsync();
                
                if(responseString != null)
                {
                    Console.WriteLine(responseString);
                }

                onlinePlayers = 0;
            }
        }

        static async Task Main(string[] args)
        {
            Globals.server.Start();

            Console.WriteLine("Server started on "+Globals.server.LocalEndpoint.ToString());

            Globals.world.Load();

            _ = SendServerHeartbeat();

            for(int i = 0; i < Globals.MaxPlayers; i++)
            {
                Client client = new Client();
                Globals.clients.Add(client);
            }

            while(true)
            {
                var client = await Globals.server.AcceptTcpClientAsync();
                AddPlayer(client);
                Thread.Sleep(1000);
            }
        }

        static async void AddPlayer(TcpClient tcpClient)
        {
            for(int i = 0; i < Globals.MaxPlayers; i++)
            {
                var client = Globals.clients[i];
                if(client.playerClient == null)
                {
                    client.playerClient = tcpClient;
                    client.id = i;
                    client.IpAddress = client.playerClient.Client.RemoteEndPoint.ToString();
                    client.username = "Player" + client.id;

                    Console.WriteLine("Id: "+ i);
                
                    client.Run();
                    break;
                }
            }
        }
    }
}