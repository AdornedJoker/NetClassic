using System;
using System.Buffers;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SuperSimpleTcp;

namespace NetClassic
{
    public class LevelDataChunk : Packets
    {
        public async Task SendPacket(NetworkStream stream)
        {
            byte[] compressedData;
            using (MemoryStream compressedStream = new MemoryStream())
            {
                using (GZipStream gzipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                {
                    gzipStream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Globals.world.BlockData.Length)));
                    gzipStream.Write(Globals.world.BlockData, 0, Globals.world.BlockData.Length);
                    gzipStream.Close();
                }
                compressedData = compressedStream.ToArray();
            }

            for (int i = 0; i < compressedData.Length; i += 1024)
            {
                short length = (short)Math.Min(1024, compressedData.Length - i);
                ArraySegment<byte> chunk;
                ReadOnlySpan<byte> sequence;
                
                using (var packet = new MemoryStream())
                {
                    packet.WriteByte((byte)ServerPacketTypes.LevelDataChunk);
                    packet.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(length)));

                    chunk = new(compressedData, i, length);
                    sequence = chunk;
                    packet.Write(chunk);
                    for(int j = 0; j < 1024 - sequence.Length; j++)
                    {
                        packet.WriteByte(0x00);
                    }

                    packet.WriteByte((byte)Math.Ceiling((double)i * 100 / compressedData.Length));
                    
                    await stream.WriteAsync(packet.ToArray());
                }
            }
        }
    }
}