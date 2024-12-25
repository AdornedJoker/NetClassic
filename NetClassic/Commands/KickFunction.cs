using System;
using System.Net.Sockets;

namespace NetClassic
{
    public class KickFunction : Commands
    {
        public async Task HandleCommand(string adminCommand, NetworkStream stream)
        {
            string getName = adminCommand.Substring(4).TrimStart();
            int userID = 0;

            if(Checks.IsUserExists(getName))
            {
                for (int i = 0; i < Globals.clients.Count; i++)
                {
                    var client = Globals.clients[i];
                    if(client.username.ToLower() == getName)
                    {
                        userID = client.id;
                        break;
                    }
                }

                NetworkStream otherUserStream = Globals.clients[userID].stream;

                DisconnectPlayer disconnectPacket = new DisconnectPlayer();
                await otherUserStream.WriteAsync(disconnectPacket.SendPacket("You were kicked"));
                Globals.clients[userID].Disconnect();
            }
            else
            {
                await stream.WriteAsync(GameMessage.SendPacket(255, "Unknown command!"));
            }
        }
    }
}