using System;
using System.IO;
using System.Text;

using RLStats_Classes.Encryption;
using RLStats_Classes.Interfaces;

namespace RLStats_Classes
{
    public class RLConstants : ISaveBallchasingToken
    {
        public static int CurrentSeason { get; private set; } = 5;

        private static readonly byte[] _key = Encoding.UTF8.GetBytes("+9.[#qr5S1;r{A2d"); //Yea, I know this is not THAT safe.

        public static string BallchasingToken
        {
            get => ReadKey(GetBallchasingKeyFilePath());
            set => WriteKey(GetBallchasingKeyFilePath(), value);
        }

        public static string RLStatsFolder => GetRLStatsFolder();

        public static string ReplayCacheFolder => GetReplayCacheFolder();

        public int GetCurrentSeason => CurrentSeason;

        private static string ReadKey(string filePath)
        {
            try
            {
                var bytes = File.ReadAllBytes(filePath);
                bytes = AESEncryptor.DecryptByteArray(_key, bytes);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void WriteKey(string filePath, string ballchasingKey)
        {
            if (string.IsNullOrEmpty(ballchasingKey) || string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException();
            var bytes = Encoding.UTF8.GetBytes(ballchasingKey);
            bytes = AESEncryptor.EncryptByteArray(_key, bytes);
            File.WriteAllBytes(filePath, bytes);
        }

        private static string GetRLStatsFolder()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Rocket League Stats");
            _ = Directory.CreateDirectory(path);
            return path;
        }

        private static string GetReplayCacheFolder()
        {
            var path = Path.Combine(GetRLStatsFolder(), "ReplayCache");
            var dir = Directory.CreateDirectory(path);
            dir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            return path;
        }

        private static string GetBallchasingKeyFilePath()
        {
            var keyPath = Path.Combine(GetRLStatsFolder(), "ballchasingKey");
            return keyPath;
        }

        public string GetBallchasingToken()
        {
            return BallchasingToken;
        }

        public void SetBallchasingToken(string ballchasingToken)
        {
            BallchasingToken = ballchasingToken;
        }
    }
}
