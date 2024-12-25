using System;
using System.Reflection;
using System.Text;

namespace NetClassic
{
    public class FileHandle()
    {
        public static bool isCreated(string directory)
        {
            if (File.Exists(directory))
            {
                return true;
            }

            return false;
        }

        public static void CreateServerFiles()
        {
            Console.WriteLine("Creating server files!");
            var defaultValues = new string[6] {"verify-names=true", "port=25565", "max-players=16", 
            "server-name=Minecraft Server", "public=true", "motd=Welcome to my Minecraft Server!"};
            File.AppendAllLines(Globals.serverProperties, defaultValues);
        }

        public static string readValue(string directory, string input)
        {
            if(isCreated(directory) == false)
            {
                CreateServerFiles();
            }
            
            string[] lines = File.ReadAllLines(directory);
            
            for(int i = 0; i < lines.Length; i++)
            {
                if(lines[i].Contains(input))
                {
                    string value = lines[i].Replace(input+"=", "");
                    return value;
                }
            }

            return "";
        }
    }
}