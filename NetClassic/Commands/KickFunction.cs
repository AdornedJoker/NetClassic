using System;
using System.Net.Sockets;

namespace NetClassic
{
    public class KickFunction : Commands
    {
        public async Task HandleCommand(string adminCommand, Socket stream)
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

                Socket otherUserPlayer = Globals.clients[userID].playerClient;

                DisconnectPlayer disconnectPacket = new DisconnectPlayer();
                await otherUserPlayer.SendAsync(disconnectPacket.SendPacket("You were kicked"));
            }
            else
            {
                await stream.SendAsync(GameMessage.SendPacket(255, "Unknown command!"));
            }
        }
    }
}