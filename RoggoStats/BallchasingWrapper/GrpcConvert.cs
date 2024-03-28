using BallchasingWrapper.BusinessLogic;
using BallchasingWrapper.Models.ReplayModels.Advanced;
using AdvancedReplay = BallchasingWrapper.Models.ReplayModels.Advanced.AdvancedReplay;
using Player = BallchasingWrapper.Models.ReplayModels.Player;
using Rank = BallchasingWrapper.Models.ReplayModels.Rank;
using Replay = BallchasingWrapper.Models.ReplayModels.Replay;
using Team = BallchasingWrapper.Models.ReplayModels.Team;

namespace BallchasingWrapper;

public static class GrpcConvert
{
    public static Grpc.AdvancedReplay ToGrpcAdvancedReplay(this AdvancedReplay advancedReplay)
    {
        var grpcAdvancedReplay = new Grpc.AdvancedReplay
        {
            Id = advancedReplay.Id,
            Link = advancedReplay.Link,
            Created = advancedReplay.Created.ToRfc3339String(),
            Status = advancedReplay.Status,
            RocketLeagueId = advancedReplay.RocketLeagueId,
            MatchGuid = advancedReplay.MatchGuid,
            Title = advancedReplay.Title,
            MapCode = advancedReplay.MapCode,
            ServerType = advancedReplay.MatchType,
            TeamSize = advancedReplay.TeamSize,
            MatchType = ConvertToGrpcMatchType(advancedReplay.PlaylistId),
            Playlist = ConvertToGrpcPlaylist(advancedReplay.PlaylistId),
            Duration = advancedReplay.Duration,
            Overtime = advancedReplay.Overtime,
            OvertimeSeconds = advancedReplay.OvertimeSeconds,
            Date = advancedReplay.Date.ToRfc3339String(),
            Visibility = ConvertToGrpcVisibility(advancedReplay.Visibility),
            MapName = advancedReplay.MapName,
            Blue = ConvertToGrpcAdvancedTeam(advancedReplay.TeamBlue),
            Orange = ConvertToGrpcAdvancedTeam(advancedReplay.TeamOrange),
        };
        if (advancedReplay.MinRank is not null)
            grpcAdvancedReplay.MinRank = ConvertToGrpcRank(advancedReplay.MinRank);

        if (advancedReplay.MaxRank is not null)
            grpcAdvancedReplay.MaxRank = ConvertToGrpcRank(advancedReplay.MaxRank);
        if (advancedReplay.Server is not null)
            grpcAdvancedReplay.Server = ConvertToGrpcServer(advancedReplay.Server);
        return grpcAdvancedReplay;
    }

    private static Grpc.Server ConvertToGrpcServer(Server server)
    {
        return new Grpc.Server
        {
            Name = server.Name,
            Region = server.Region
        };
    }

    private static Grpc.AdvancedTeam ConvertToGrpcAdvancedTeam(AdvancedTeam team)
    {
        return new Grpc.AdvancedTeam
        {
            Color = team.Color,
            Players = { team.Players.Select(ConvertToGrpcAdvancedPlayer) },
            Stats = ConvertToGrpcTeamStats(team.Stats)
        };
    }

    private static Grpc.TeamStats ConvertToGrpcTeamStats(TeamStats stats)
    {
        return new Grpc.TeamStats
        {
            Ball = ConvertToGrpcBall(stats.Ball),
            Core = ConvertToGrpcGeneralCore(stats.Core),
            Boost = ConvertToGrpcGeneralBoost(stats.Boost),
            Movement = ConvertToGrpcGeneralMovement(stats.Movement),
            Positioning = ConvertToGrpcGeneralPositioning(stats.Positioning),
            Demo = ConvertToGrpcDemo(stats.Demo)
        };
    }

    private static Grpc.Demo ConvertToGrpcDemo(Demo demo)
    {
        return new Grpc.Demo
        {
            Inflicted = demo.Inflicted,
            Taken = demo.Taken
        };
    }

    private static Grpc.GeneralPositioning ConvertToGrpcGeneralPositioning(GeneralPositioning positioning)
    {
        return new Grpc.GeneralPositioning
        {
            TimeDefensiveThird = positioning.TimeDefensiveThird,
            TimeNeutralThird = positioning.TimeNeutralThird,
            TimeOffensiveThird = positioning.TimeOffensiveThird,
            TimeDefensiveHalf = positioning.TimeDefensiveHalf,
            TimeOffensiveHalf = positioning.TimeOffensiveHalf,
            TimeBehindBall = positioning.TimeBehindBall,
            TimeInfrontBall = positioning.TimeInfrontBall
        };
    }

    private static Grpc.GeneralMovement ConvertToGrpcGeneralMovement(GeneralMovement movement)
    {
        return new Grpc.GeneralMovement
        {
            TotalDistance = movement.TotalDistance,
            TimeSupersonicSpeed = movement.TimeSupersonicSpeed,
            TimeBoostSpeed = movement.TimeBoostSpeed,
            TimeSlowSpeed = movement.TimeSlowSpeed,
            TimeGround = movement.TimeGround,
            TimeLowAir = movement.TimeLowAir,
            TimeHighAir = movement.TimeHighAir,
            TimePowerslide = movement.TimePowerslide,
            CountPowerslide = movement.CountPowerslide
        };
    }

    private static Grpc.GeneralBoost ConvertToGrpcGeneralBoost(GeneralBoost boost)
    {
        return new Grpc.GeneralBoost
        {
            Bpm = boost.Bpm,
            Bcpm = boost.Bcpm,
            AvgAmount = boost.AvgAmount,
            AmountCollected = boost.AmountCollected,
            AmountStolen = boost.AmountStolen,
            AmountCollectedBig = boost.AmountCollectedBig,
            AmountStolenBig = boost.AmountStolenBig,
            AmountCollectedSmall = boost.AmountCollectedSmall,
            AmountStolenSmall = boost.AmountStolenSmall,
            CountCollectedBig = boost.CountCollectedBig,
            CountStolenBig = boost.CountStolenBig,
            CountCollectedSmall = boost.CountCollectedSmall,
            CountStolenSmall = boost.CountStolenSmall,
            AmountOverfill = boost.AmountOverfill,
            AmountOverfillStolen = boost.AmountOverfillStolen,
            AmountUsedWhileSupersonic = boost.AmountUsedWhileSupersonic,
            TimeZeroBoost = boost.TimeZeroBoost,
            TimeFullBoost = boost.TimeFullBoost,
            TimeBoost0To25 = boost.TimeBoost0To25,
            TimeBoost25To50 = boost.TimeBoost25To50,
            TimeBoost50To75 = boost.TimeBoost50To75,
            TimeBoost75To100 = boost.TimeBoost75To100
        };
    }

    private static Grpc.GeneralCore ConvertToGrpcGeneralCore(GeneralCore core)
    {
        return new Grpc.GeneralCore
        {
            Shots = core.Shots,
            ShotsAgainst = core.ShotsAgainst,
            Goals = core.Goals,
            GoalsAgainst = core.GoalsAgainst,
            Saves = core.Saves,
            Assists = core.Assists,
            Score = core.Score,
            ShootingPercentage = core.ShootingPercentage
        };
    }

    private static Grpc.Ball ConvertToGrpcBall(Ball ball)
    {
        return new Grpc.Ball
        {
            PossessionTime = ball.PossessionTime,
            TimeInSide = ball.TimeInSide
        };
    }

    private static Grpc.AdvancedPlayer ConvertToGrpcAdvancedPlayer(AdvancedPlayer player)
    {
        return new Grpc.AdvancedPlayer
        {
            StartTime = player.StartTime,
            EndTime = player.EndTime,
            Name = player.Name,
            Id = new Grpc.PlayerId
            {
                Platform = player.Id.Platform,
                Id = player.Id.Id
            },
            Rank = ConvertToGrpcRank(player.Rank),
            CarId = player.CarId,
            CarName = player.CarName,
            Camera = ConvertToGrpcCamera(player.Camera),
            SteeringSensitivity = player.SteeringSensitivity,
            Stats = ConvertToGrpcPlayerStats(player.Stats)
        };
    }

    private static Grpc.PlayerStats ConvertToGrpcPlayerStats(PlayerStats stats)
    {
        return new Grpc.PlayerStats
        {
            Core = ConvertToGrpcPlayerCore(stats.Core),
            Boost = ConvertToGrpcPlayerBoost(stats.Boost),
            Movement = ConvertToGrpcPlayerMovement(stats.Movement),
            Positioning = ConvertToGrpcPlayerPositioning(stats.Positioning),
            Demo = ConvertToGrpcDemo(stats.Demo)
        };
    }

    private static Grpc.PlayerPositioning ConvertToGrpcPlayerPositioning(PlayerPositioning positioning)
    {
        return new Grpc.PlayerPositioning
        {
            GeneralPositioning = ConvertToGrpcGeneralPositioning(positioning),
            AvgDistanceToBall = positioning.AvgDistanceToBall,
            AvgDistanceToBallPossession = positioning.AvgDistanceToBallPossession,
            AvgDistanceToBallNoPossession = positioning.AvgDistanceToBallNoPossession,
            AvgDistanceToMates = positioning.AvgDistanceToMates,
            TimeMostBack = positioning.TimeMostBack,
            TimeMostForward = positioning.TimeMostForward,
            TimeClosestToBall = positioning.TimeClosestToBall,
            TimeFarthestFromBall = positioning.TimeFarthestFromBall,
            PercentDefensiveThird = positioning.PercentDefensiveThird,
            PercentOffensiveThird = positioning.PercentOffensiveThird,
            PercentNeutralThird = positioning.PercentNeutralThird,
            PercentDefensiveHalf = positioning.PercentDefensiveHalf,
            PercentOffensiveHalf = positioning.PercentOffensiveHalf,
            PercentBehindBall = positioning.PercentBehindBall,
            PercentInfrontBall = positioning.PercentInfrontBall,
            PercentMostBack = positioning.PercentMostBack,
            PercentMostForward = positioning.PercentMostForward,
            PercentClosestToBall = positioning.PercentClosestToBall,
            PercentFarthestFromBall = positioning.PercentFarthestFromBall,
            GoalsAgainstWhileLastDefender = positioning.GoalsAgainstWhileLastDefender
        };
    }

    private static Grpc.PlayerMovement ConvertToGrpcPlayerMovement(PlayerMovement movement)
    {
        return new Grpc.PlayerMovement
        {
            GeneralMovement = ConvertToGrpcGeneralMovement(movement),
            AvgSpeed = movement.AvgSpeed,
            AvgPowerslideDuration = movement.AvgPowerslideDuration,
            AvgSpeedPercentage = movement.AvgSpeedPercentage,
            PercentSlowSpeed = movement.PercentSlowSpeed,
            PercentBoostSpeed = movement.PercentBoostSpeed,
            PercentSupersonicSpeed = movement.PercentSupersonicSpeed,
            PercentGround = movement.PercentGround,
            PercentLowAir = movement.PercentLowAir,
            PercentHighAir = movement.PercentHighAir
        };
    }

    private static Grpc.PlayerBoost ConvertToGrpcPlayerBoost(PlayerBoost boost)
    {
        return new Grpc.PlayerBoost
        {
            GeneralBoost = ConvertToGrpcGeneralBoost(boost),
            PercentZeroBoost = boost.PercentZeroBoost,
            PercentFullBoost = boost.PercentFullBoost,
            PercentBoost0To25 = boost.PercentBoost0To25,
            PercentBoost25To50 = boost.PercentBoost25To50,
            PercentBoost50To75 = boost.PercentBoost50To75,
            PercentBoost75To100 = boost.PercentBoost75To100
        };
    }

    private static Grpc.PlayerCore ConvertToGrpcPlayerCore(PlayerCore core)
    {
        return new Grpc.PlayerCore
        {
            GeneralCore = ConvertToGrpcGeneralCore(core),
            Mvp = core.Mvp
        };
    }

    private static Grpc.Camera ConvertToGrpcCamera(Camera camera)
    {
        return new Grpc.Camera
        {
            Fov = camera.Fov,
            Height = camera.Height,
            Pitch = camera.Pitch,
            Distance = camera.Distance,
            Stiffness = camera.Stiffness,
            SwivelSpeed = camera.SwivelSpeed,
            TransitionSpeed = camera.TransitionSpeed
        };
    }

    private static Grpc.Visibility ConvertToGrpcVisibility(string visibility)
    {
        return visibility switch
        {
            "public" => Grpc.Visibility.Public,
            "private" => Grpc.Visibility.Private,
            "unlisted" => Grpc.Visibility.Unlisted,
            _ => Grpc.Visibility.Unknown
        };
    }

    public static Grpc.Replay ToGrpcReplay(this Replay replay)
    {
        var grpcReplay = new Grpc.Replay
        {
            Id = replay.Id,
            Title = replay.Title,
            MapName = replay.MapCode,
            Playlist = ConvertToGrpcPlaylist(replay.Playlist),
            MatchType = ConvertToGrpcMatchType(replay.Playlist),
            Duration = replay.Duration,
            Overtime = replay.Overtime,
            Date = replay.Date.ToRfc3339String(),
            Blue = ConvertToGrpcTeam(replay.TeamBlue),
            Orange = ConvertToGrpcTeam(replay.TeamOrange),
        };
        if (replay.MinRank is not null)
            grpcReplay.MinRank = ConvertToGrpcRank(replay.MinRank);

        if (replay.MaxRank is not null)
            grpcReplay.MaxRank = ConvertToGrpcRank(replay.MaxRank);

        return grpcReplay;
    }

    private static Grpc.Rank ConvertToGrpcRank(Rank rank)
    {
        var grpcRank = new Grpc.Rank
        {
            Id = ConvertToGrpcRankType(rank.Id),
            Tier = rank.Tier,
            Division = rank.Division,
            Name = rank.Name
        };
        return grpcRank;
    }

    private static Grpc.RankType ConvertToGrpcRankType(string? rankId)
    {
        if (rankId is null)
            return 0;
        return rankId switch
        {
            "" => Grpc.RankType.Unknown,
            "unranked" => Grpc.RankType.NoRank,
            "bronze-1" => Grpc.RankType.Bronze1,
            "bronze-2" => Grpc.RankType.Bronze2,
            "bronze-3" => Grpc.RankType.Bronze3,
            "silver-1" => Grpc.RankType.Silver1,
            "silver-2" => Grpc.RankType.Silver2,
            "silver-3" => Grpc.RankType.Silver3,
            "gold-1" => Grpc.RankType.Gold1,
            "gold-2" => Grpc.RankType.Gold2,
            "gold-3" => Grpc.RankType.Gold3,
            "platinum-1" => Grpc.RankType.Platinum1,
            "platinum-2" => Grpc.RankType.Platinum2,
            "platinum-3" => Grpc.RankType.Platinum3,
            "diamond-1" => Grpc.RankType.Diamond1,
            "diamond-2" => Grpc.RankType.Diamond2,
            "diamond-3" => Grpc.RankType.Diamond3,
            "champion-1" => Grpc.RankType.Champion1,
            "champion-2" => Grpc.RankType.Champion2,
            "champion-3" => Grpc.RankType.Champion3,
            "grand-champion" => Grpc.RankType.OldGrandChampion,
            "grand-champion-1" => Grpc.RankType.GrandChampion1,
            "grand-champion-2" => Grpc.RankType.GrandChampion2,
            "grand-champion-3" => Grpc.RankType.GrandChampion3,
            "supersonic-legend" => Grpc.RankType.SupersonicLegend,
            _ => throw new ArgumentOutOfRangeException(nameof(rankId), rankId, "Cannot parse rank to RankType")
        };
    }

    private static Grpc.Team ConvertToGrpcTeam(Team team)
    {
        var grpcTeam = new Grpc.Team
        {
            Goals = team.Goals,
            Players = { team.Players.Select(ConvertToGrpcPlayer) }
        };
        return grpcTeam;
    }

    private static Grpc.Player ConvertToGrpcPlayer(Player player)
    {
        var grpcPlayer = new Grpc.Player
        {
            Name = player.Name ?? string.Empty,
            Mvp = player.MVP,
            Score = player.Score,
            StartTime = player.StartTime,
            EndTime = player.EndTime,
        };

        if (player.Id.Id is not null)
            grpcPlayer.Id = new Grpc.PlayerId
            {
                Id = player.Id.Id,
                Platform = player.Id.Platform
            };
        if (player.Rank is not null)
            grpcPlayer.Rank = ConvertToGrpcRank(player.Rank);


        return grpcPlayer;
    }

    private static Grpc.MatchType ConvertToGrpcMatchType(string playlist)
    {
        switch (playlist)
        {
            case "ranked-duels":
            case "ranked-doubles":
            case "ranked-standard":
            case "ranked-solo-standard":
            case "ranked-hoops":
            case "ranked-rumble":
            case "ranked-dropshot":
            case "ranked-snowday":
                return Grpc.MatchType.Ranked;
            default:
                return Grpc.MatchType.Unranked;
        }
    }

    private static Grpc.Playlist ConvertToGrpcPlaylist(string playlist)
    {
        switch (playlist)
        {
            case "ranked-duels":
            case "unranked-duels":
                return Grpc.Playlist.Duels;
            case "ranked-doubles":
            case "unranked-doubles":
                return Grpc.Playlist.Doubles;
            case "ranked-standard":
            case "ranked-solo-standard":
            case "unranked-standard":
                return Grpc.Playlist.Standard;
            case "ranked-hoops":
            case "hoops":
                return Grpc.Playlist.Hoops;
            case "ranked-rumble":
            case "rumble":
                return Grpc.Playlist.Rumble;
            case "ranked-dropshot":
            case "dropshot":
                return Grpc.Playlist.DropShot;
            case "ranked-snowday":
            case "snowday":
                return Grpc.Playlist.SnowDay;
            case "dropshot-rumble":
                return Grpc.Playlist.DropShotRumble;
            case "heatseeker":
                return Grpc.Playlist.HeatSeeker;
            case "offline":
                return Grpc.Playlist.Offline;
            case "tournament":
                return Grpc.Playlist.Tournament;
            default:
                return Grpc.Playlist.All;
        }
    }
}