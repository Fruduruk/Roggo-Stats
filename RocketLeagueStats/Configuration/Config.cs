namespace RocketLeagueStats.Configuration
{
    public class Config
    {
        public string BallchasingToken { get; set; }
        public bool UseMongoDB { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
