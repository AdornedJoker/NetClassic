using System;
using System.Text;
using SuperSimpleTcp;

namespace NetClassic
{
    public class ReadWrite
    {
        public static string ReadString(byte[] array, int index)
        {
            return Encoding.UTF8.GetString(array.ToArray(), index, 64).TrimEnd();
        }

        public static int GetPrevStringLength(string data)
        {
            int length = data.Length;
            while(length < 64)
            {
                length++;
            }

            return length;
        }

        public static short ReadShort(byte[] array, int index)
        {
            return BitConverter.ToInt16(array, index);
        }

        public static byte[] WriteString(string Message)
        {
            byte[] message = Encoding.UTF8.GetBytes(Message);

            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(message);
            for(int i = message.Length; i < 64; i++)
            {
                memoryStream.WriteByte(0x20);
            }
            return memoryStream.ToArray();
        }
    }
}