using BallchasingWrapper.BusinessLogic;
using BallchasingWrapper.Models;
using Player = BallchasingWrapper.Models.ReplayModels.Player;
using Rank = BallchasingWrapper.Models.ReplayModels.Rank;
using Replay = BallchasingWrapper.Models.ReplayModels.Replay;
using Team = BallchasingWrapper.Models.ReplayModels.Team;

namespace BallchasingWrapper;

public static class GrpcConvert
{
    public static APIRequestFilter ToApiRequestFilter(this Grpc.RequestFilter filter)
    {
        var requestFilter = new APIRequestFilter
        {
            ReplayCap = filter.HasReplayCap ? filter.ReplayCap : 0,
            FilterName = filter.HasFilterName ? filter.FilterName : string.Empty,
        };
        return requestFilter;
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
            Id = rank.Id ?? string.Empty,
            Tier = rank.Tier,
            Division = rank.Division,
            Name = rank.Name
        };
        return grpcRank;
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