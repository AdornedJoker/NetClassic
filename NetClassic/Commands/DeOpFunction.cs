using System;
using System.Net.Sockets;

namespace NetClassic
{
    public class DeOpFunction : Commands
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

                Socket otherUserStream = Globals.clients[userID].playerClient;

                UpdateUserType packet = new UpdateUserType();

                await otherUserStream.SendAsync(packet.SendPacket(0x00));
                await otherUserStream.SendAsync(GameMessage.SendPacket(255, "You're no longer op!"));
                Globals.clients[userID].UserType = 0x00;

                //TODO: GET THIS TO WORK: FileHandle.RemoveName(getName, Globals.adminsDirectory);
            }
            else
            {
                await stream.SendAsync(GameMessage.SendPacket(255, "Unknown command!"));
            }
        }
    }
}