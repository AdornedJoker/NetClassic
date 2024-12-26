using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

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

        public static void CreateServerFile()
        {
            Console.WriteLine("Creating server files!");
            var defaultValues = new string[7] {"verify-names=true", "port=25565", "max-players=16", 
            "server-name=Minecraft Server", "public=true", "motd=Welcome to my Minecraft Server!", "salt="+Globals.CreateSalt()};
            File.AppendAllLines(Globals.serverProperties, defaultValues);
        }

        public static void CreateOthers(string directory)
        {
            File.Create(directory);
        }

        public static string ReadValue(string directory, string input)
        {
            if(isCreated(directory) == false)
            {
                CreateServerFile();
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

        public static bool CheckName(string username, string directory)
        {
            if(isCreated(directory) == false)
            {
                CreateOthers(directory);
            }

            string[] lines = File.ReadAllLines(directory);
            
            for(int i = 0; i < lines.Length; i++)
            {
                if(Regex.IsMatch(lines[i], "^"+username+"$"))
                {
                    return true;
                }
            }

            return false;
        }

        public static void WriteName(string username, string directory)
        {
            if(isCreated(directory) == false)
            {
                CreateOthers(directory);
            }

            string[] lines = File.ReadAllLines(directory);

            for(int i = 0; i < lines.Length; i++)
            {
                if(Regex.IsMatch(lines[i], "^"+username+"$"))
                {
                    return; //Username already exists.
                }
            }

            File.AppendAllText(directory, Environment.NewLine+username);
        }


        //TO DO, GET THIS TO WORK LATER.
        public static void RemoveName(string username, string directory)
        {
            if(isCreated(directory) == false)
            {
                CreateOthers(directory);
            }

            string[] lines = File.ReadAllLines(directory);

            for(int i = 0; i < lines.Length; i++)
            {
                if(Regex.IsMatch(lines[i], "^"+username+"$"))
                {
                    lines[i].Replace(username, "");
                }
            }
            File.AppendAllText(directory, lines.ToString());
        }
    }
}