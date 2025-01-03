using System;
using System.Net.Sockets;

namespace NetClassic
{
    public class OpFunction : Commands
    {
        public async Task HandleCommand(string adminCommand, Socket stream)
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

                Socket otherUserStream = Globals.clients[userID].playerClient;

                UpdateUserType packet = new UpdateUserType();

                await otherUserStream.SendAsync(packet.SendPacket(0x64));
                await otherUserStream.SendAsync(GameMessage.SendPacket(255, "You're now op!"));
                Globals.clients[userID].UserType = 0x64;

                FileHandle.WriteName(getName, Globals.adminsDirectory);
            }
            else
            {
                await stream.SendAsync(GameMessage.SendPacket(255, "Unknown command!"));
            }
        }
    }
}