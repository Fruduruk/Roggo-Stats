using Newtonsoft.Json;

using RLStats_Classes.Encryption;

using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace RLStats_Classes.MainClasses
{
    public class Compressor
    {
        private static readonly byte[] _key = Encoding.UTF8.GetBytes("Co+m/pr9e,s2[sor");
        #region bytes
        public static byte[] CompressBytes(byte[] bytes, bool encrypt = true)
        {
            byte[] buffer = bytes;
            using var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return encrypt ? AESEncryptor.EncryptByteArray(_key, gZipBuffer) : gZipBuffer;
        }
        public static byte[] DecompressBytes(byte[] compressedBytes, bool decrypt = true)
        {
            compressedBytes = decrypt ? AESEncryptor.DecryptByteArray(_key, compressedBytes) : compressedBytes;
            using (var memoryStream = new MemoryStream())
            {
                var dataLength = BitConverter.ToInt32(compressedBytes, 0);
                memoryStream.Write(compressedBytes, 4, compressedBytes.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }
                return buffer;
            }
        }
        #endregion
        #region string
        public static byte[] CompressString(string text) => CompressBytes(Encoding.UTF8.GetBytes(text));

        public static string DecompressBytesToString(byte[] compressedBytes) => Encoding.UTF8.GetString(DecompressBytes(compressedBytes));
        #endregion
        #region objects
        public static byte[] ConvertObject<T>(T obj) where T : new()
        {
            var jsonString = JsonConvert.SerializeObject(obj);
            var compressedBytes = CompressString(jsonString);
            return compressedBytes;
        }

        public static T ConvertObject<T>(byte[] bytes)
        {
            var decompressedBytesAsString = DecompressBytesToString(bytes);
            var obj = JsonConvert.DeserializeObject<T>(decompressedBytesAsString);
            return obj;
        }
        #endregion
    }
}
