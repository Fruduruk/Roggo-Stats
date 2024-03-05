using System.IO.Compression;
using System.Text;
using BallchasingWrapper.Encryption;

namespace BallchasingWrapper.DB.LegacyFileDB
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

                //OMG dotnet 6 changed the behaviour of GZipStream.Read(). It just stops the read sometimes now. WTF. I will loop it now.
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    int totalRead = 0;
                    while (totalRead < buffer.Length)
                    {
                        int bytesRead = gZipStream.Read(buffer, totalRead, buffer.Length - totalRead);
                        if (bytesRead == 0) break;
                        totalRead += bytesRead;
                    }
                    if (totalRead != buffer.Length)
                        throw new Exception("GZipStream did not read all the bytes again...");
                }
                return buffer;
            }
        }
        #endregion
        #region string
        public static byte[] CompressString(string text, bool encrypt = true) => CompressBytes(Encoding.UTF8.GetBytes(text), encrypt);

        public static string DecompressBytesToString(byte[] compressedBytes, bool decrypt = true) => Encoding.UTF8.GetString(DecompressBytes(compressedBytes, decrypt));
        #endregion
        #region objects
        public static byte[] ConvertObject<T>(T obj, bool encrypt = true) where T : new()
        {
            var jsonString = JsonConvert.SerializeObject(obj, Formatting.Indented);
            var compressedBytes = CompressString(jsonString, encrypt);
            return compressedBytes;
        }

        public static T ConvertObject<T>(byte[] bytes, bool decrypt = true)
        {
            var decompressedBytesAsString = DecompressBytesToString(bytes, decrypt);
            var obj = JsonConvert.DeserializeObject<T>(decompressedBytesAsString);
            return obj;
        }
        #endregion
    }
}
