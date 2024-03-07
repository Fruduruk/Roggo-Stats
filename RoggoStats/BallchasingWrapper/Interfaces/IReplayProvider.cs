using BallchasingWrapper.BusinessLogic;
using BallchasingWrapper.Models;

namespace BallchasingWrapper.Interfaces
{
    public interface IReplayProvider : IReplayProviderBase
    {
        Task<CollectReplaysResponse> CollectReplaysAsync(ApiUrlCreator urlCreator, bool cached = false);
        void CancelDownload();
    }
}

/*Playlist.UnrankedDuels => "unranked-duels",
        Playlist.UnrankedDoubles => "unranked-doubles",
        Playlist.UnrankedStandard => "unranked-standard",
        Playlist.UnrankedChaos => "unranked-chaos",
        Playlist.PrivateGame => "private",
        Playlist.Season => "season",
        Playlist.Offline => "offline",
        Playlist.RankedDuels => "ranked-duels",
        Playlist.RankedDoubles => "ranked-doubles",
        Playlist.RankedSoloStandard => "ranked-solo-standard",
        Playlist.RankedStandard => "ranked-standard",
        Playlist.Snowday => "snowday",
        Playlist.Rocketlabs => "rocketlabs",
        Playlist.Hoops => "hoops",
        Playlist.Rumble => "rumble",
        Playlist.Tournament => "tournament",
        Playlist.Dropshot => "dropshot",
        Playlist.RankedHoops => "ranked-hoops",
        Playlist.RankedRumble => "ranked-rumble",
        Playlist.RankedDropshot => "ranked-dropshot",
        Playlist.RankedSnowday => "ranked-snowday",
        Playlist.DropshotRumble => "dropshot-rumble",
        Playlist.Heatseeker => "heatseeker",
        _ => string.Empty*/