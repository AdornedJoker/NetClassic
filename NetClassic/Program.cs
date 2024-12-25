using System;
using SuperSimpleTcp;
using ClassicWorld.NET;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;

namespace NetClassic
{
    public class Globals
    {

        private static Random RNG = new Random();

        public static string CreateSalt()
        {
            string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var builder = new StringBuilder();

            while (builder.Length < 16) 
            {
                builder.Append(ALPHABET[RNG.Next(ALPHABET.Length)]);
            }
            return builder.ToString();
        }

        public static List<Client> clients = new List<Client>();

        public static string serverProperties = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+"\\server.properties";
        
        public static Classicworld world = new Classicworld(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+ "\\serverLevel.cw");

        public static int MaxPlayers = Convert.ToInt32(FileHandle.readValue(serverProperties, "max-players"));

        public static bool nameVerfication = Convert.ToBoolean(FileHandle.readValue(serverProperties, "verify-names"));

        public static string serverName = FileHandle.readValue(serverProperties, "server-name");

        public static string serverMotd = FileHandle.readValue(serverProperties, "motd");

        public static bool isOnline = Convert.ToBoolean(FileHandle.readValue(serverProperties, "public"));
        public static string salt = FileHandle.readValue(serverProperties, "salt");

        public static TcpListener server = new TcpListener(IPAddress.Any, 25565);
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
                { "name", Globals.serverName },
                { "port", "25565" },
                { "users", "0" },
                { "max", Globals.MaxPlayers.ToString() },
                { "public", Globals.isOnline.ToString() },
                { "salt", Globals.salt },
                { "software", "&cNetClassic &av0.5" },
                { "web", false.ToString() },
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
                    { "name", Globals.serverName },
                    { "port", "25565" },
                    { "users", onlinePlayers.ToString() },
                    { "max", Globals.MaxPlayers.ToString() },
                    { "public", Globals.isOnline.ToString() },
                    { "salt", Globals.salt },
                    { "software", "&cNetClassic &av0.5" },
                    { "web", false.ToString() },
                };
                content = new FormUrlEncodedContent(values);
                await client.PostAsync(heartbeatServer, content);
                response = await client.PostAsync(heartbeatServer, content);
                responseString = await response.Content.ReadAsStringAsync();
                
                if(responseString != null)
                {
                    Console.WriteLine(responseString);
                }
                Globals.world.Save(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+ "\\serverLevel.cw");
                Console.WriteLine("Saving world...");

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

                    Console.WriteLine(client.IpAddress+ " has connected!");
                
                    client.Run();
                    break;
                }
            }
        }
    }
}