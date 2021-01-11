namespace RocketLeagueStats.AdvancedModels
{
    public class PlayerStats
    {
        public PlayerCore PlayerCore { get; set; }
        public PlayerBoost PlayerBoost { get; set; }
        public PlayerMovement PlayerMovement { get; set; }
        public PlayerPositioning PlayerPositioning { get; set; }
        public Demo Demo { get; set; }
    }
}

