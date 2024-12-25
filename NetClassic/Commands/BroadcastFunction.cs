using System;
using System.Net.Sockets;

namespace NetClassic
{
    public class BroadcastFunction : Commands
    {
        public async Task HandleCommand(string adminCommand, bool isSay)
        {
            string getMessage = "";

            if(isSay)
            {
                getMessage = adminCommand.Substring(3).TrimStart();
            }
            else
            {
                getMessage = adminCommand.Substring(9).TrimStart();
            }

            ServerHandle.SendAllPlayers(GameMessage.SendPacket(255, getMessage));  
        }
    }
}