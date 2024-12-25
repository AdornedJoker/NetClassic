using System;
using System.Text;
using SuperSimpleTcp;

namespace NetClassic
{
    public enum CommandTypes
    {
        OP,
        DEOP,
        KICK,
        BAN,
        UNBAN,
        BANIP,
        SAY, //Redirects to broadcast
        BROADCAST,
        SETSPAWN,
        TP,
        SOLID //Possibly unused.
        
    }

    public class Checks
    {
        public static bool IsUserExists(string username)
        {
            for (int i = 0; i < Globals.clients.Count; i++)
            {
                if(Globals.clients[i].playerClient != null && Globals.clients[i].username.ToLower() == username)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class Commands{}
}