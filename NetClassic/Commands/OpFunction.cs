using System;
using System.Net.Sockets;

namespace NetClassic
{
    public class OpFunction : Commands
    {
        public async Task HandleCommand(string adminCommand, NetworkStream stream)
        {
            string getName = adminCommand.Substring(2).TrimStart();
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

                UpdateUserType packet = new UpdateUserType();

                await otherUserStream.WriteAsync(packet.SendPacket(0x64));
                await otherUserStream.WriteAsync(GameMessage.SendPacket(255, "You're now op!"));
                Globals.clients[userID].UserType = 0x64;
            }
            else
            {
                await stream.WriteAsync(GameMessage.SendPacket(255, "Unknown command!"));
            }
        }
    }
}