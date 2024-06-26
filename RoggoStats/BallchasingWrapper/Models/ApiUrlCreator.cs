﻿using System.Text;
using BallchasingWrapper.BusinessLogic;
using BallchasingWrapper.Extensions;
using Microsoft.Extensions.Primitives;

namespace BallchasingWrapper.Models
{
    public class ApiUrlCreator
    {
        private readonly Grpc.FilterRequest _request;
        public IEnumerable<string> Urls { get; }
        public List<Grpc.Identity> Identities { get; }
        public int Cap { get; } = 0;
        public Grpc.GroupType GroupType { get; }
        public Grpc.TimeRange TimeRange { get; }

        public ApiUrlCreator(Grpc.FilterRequest request)
        {
            if (request.HasReplayCap)
                Cap = request.ReplayCap;
            Identities = request.Identities.ToList();
            GroupType = request.GroupType;
            TimeRange = request.TimeRange;
            _request = request;
            Urls = CreateUrls().ToArray();
        }

        /// <summary>
        /// NOT CONSISTENT
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            var sortedUrls = Urls.ToList();
            sortedUrls.Sort();
            foreach (var url in sortedUrls)
            {
                hashCode.Add(url.GetHashCode());
            }

            hashCode.Add(Cap);
            hashCode.Add((int)GroupType);

            return hashCode.ToHashCode();
        }

        /// <summary>
        /// This string is unique
        /// </summary>
        public override string ToString()
        {
            var builder = new StringBuilder();
            var sortedUrls = Urls.ToList();
            sortedUrls.Sort();
            foreach (var url in sortedUrls)
            {
                builder.Append(url);
            }

            builder.Append($" Cap: {Cap}");
            builder.Append($" GroupType: {GroupType.ToString()}");
            builder.Append($" TimeRange: {TimeRange.ToDateTimes()}");

            return builder.ToString();
        }

        private IEnumerable<string> CreateUrls()
        {
            var builders = CreateIndividualBuilders(_request);
            builders = builders.SelectMany(b => AddPlaylist(b, _request));
            foreach (var builder in builders)
            {
                yield return CompleteBuilder(builder, _request);
            }
        }

        private static IEnumerable<ApiRequestBuilder> AddPlaylist(ApiRequestBuilder builder, Grpc.FilterRequest request)
        {
            if (request.Playlist is Grpc.Playlist.All && request.MatchType is Grpc.MatchType.Both)
                return new[] { builder };

            var builders = new List<ApiRequestBuilder>();
            var playlists = ComputePlaylists(request);

            foreach (var playlist in playlists)
            {
                var clone = new ApiRequestBuilder(builder);
                clone.SetPlaylist(playlist);
                builders.Add(clone);
            }

            return builders;
        }

        private static IEnumerable<ApiRequestBuilder> CreateIndividualBuilders(Grpc.FilterRequest request)
        {
            foreach (var identity in request.Identities)
            {
                var builder = new ApiRequestBuilder();
                switch (identity.IdentityType)
                {
                    case Grpc.IdentityType.Name:
                        builder.SetPlayerName(identity.NameOrId);
                        break;
                    case Grpc.IdentityType.SteamId:
                        builder.SetSteamId(identity.NameOrId);
                        break;
                    case Grpc.IdentityType.EpicId:
                        builder.SetEpicId(identity.NameOrId);
                        break;
                    case Grpc.IdentityType.Ps4GamerTag:
                        builder.SetPs4GamerTag(identity.NameOrId);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                yield return builder;
            }
        }

        private static IEnumerable<string> ComputePlaylists(Grpc.FilterRequest request)
        {
            var playlists = request.MatchType == Grpc.MatchType.Both
                ? GetAllPlaylists()
                : ComputeMatchTypePlaylists(request);


            return request.Playlist switch
            {
                Grpc.Playlist.All => playlists,
                Grpc.Playlist.Duels => playlists.Where(p => p.Contains("duels")),
                Grpc.Playlist.Doubles => playlists.Where(p => p.Contains("doubles")),
                Grpc.Playlist.Standard => playlists.Where(p => p.Contains("standard")),
                Grpc.Playlist.Chaos => playlists.Where(p => p.Contains("chaos")),
                Grpc.Playlist.PrivateGame => playlists.Where(p => p.Contains("private")),
                Grpc.Playlist.Offline => playlists.Where(p => p.Contains("offline")),
                Grpc.Playlist.SnowDay => playlists.Where(p => p.Contains("snowday")),
                Grpc.Playlist.RocketLabs => playlists.Where(p => p.Contains("rocketlabs")),
                Grpc.Playlist.Hoops => playlists.Where(p => p.Contains("hoops")),
                Grpc.Playlist.Rumble => playlists.Where(p => p.Contains("rumble")),
                Grpc.Playlist.Tournament => playlists.Where(p => p.Contains("tournament")),
                Grpc.Playlist.DropShot => playlists.Where(p => p.Contains("dropshot")),
                Grpc.Playlist.DropShotRumble => playlists.Where(p => p.Contains("dropshot-rumble")),
                Grpc.Playlist.HeatSeeker => playlists.Where(p => p.Contains("heatseeker")),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static IEnumerable<string> ComputeMatchTypePlaylists(Grpc.FilterRequest request)
        {
            var playlists = GetAllPlaylists();
            if (request.MatchType is Grpc.MatchType.Ranked)
            {
                playlists = playlists
                    .Where(playlist => playlist.Contains("ranked") && !playlist.Contains("unranked"));
            }
            else
            {
                playlists = playlists
                    .Where(playlist => playlist.Contains("unranked") || !playlist.Contains("ranked"));
            }

            if (request.GroupType is Grpc.GroupType.Together && request.Identities.Count > 1)
            {
                playlists = playlists.Where(playlist => !playlist.Contains("solo"));
            }

            return playlists;
        }

        private static string CompleteBuilder(ApiRequestBuilder builder, Grpc.FilterRequest request)
        {
            if (request.HasTitle)
            {
                builder.SetTitle(request.Title);
            }

            if (request.Season is not null)
            {
                builder.SetSeason(ParseSeason(request.Season));
            }

            if (request.HasMinDate)
            {
                builder.SetMinDate(request.MinDate);
            }

            if (request.HasMaxDate)
            {
                builder.SetMaxDate(request.MaxDate);
            }

            if (request.HasMinRank)
            {
                builder.SetMinRank(ParseRank(request.MinRank));
            }

            if (request.HasMaxRank)
            {
                builder.SetMaxRank(ParseRank(request.MaxRank));
            }

            //Always sort them by upload date
            builder.SetSortByUploadDate();
            return builder.GetApiUrl();
        }

        private static string ParseRank(Grpc.RankType rank)
        {
            return rank switch
            {
                Grpc.RankType.Bronze1 => "bronze-1",
                Grpc.RankType.Bronze2 => "bronze-2",
                Grpc.RankType.Bronze3 => "bronze-3",
                Grpc.RankType.Silver1 => "silver-1",
                Grpc.RankType.Silver2 => "silver-2",
                Grpc.RankType.Silver3 => "silver-3",
                Grpc.RankType.Gold1 => "gold-1",
                Grpc.RankType.Gold2 => "gold-2",
                Grpc.RankType.Gold3 => "gold-3",
                Grpc.RankType.Platinum1 => "platinum-1",
                Grpc.RankType.Platinum2 => "platinum-2",
                Grpc.RankType.Platinum3 => "platinum-3",
                Grpc.RankType.Diamond1 => "diamond-1",
                Grpc.RankType.Diamond2 => "diamond-2",
                Grpc.RankType.Diamond3 => "diamond-3",
                Grpc.RankType.Champion1 => "champion-1",
                Grpc.RankType.Champion2 => "champion-2",
                Grpc.RankType.Champion3 => "champion-3",
                Grpc.RankType.OldGrandChampion => "grand-champion",
                Grpc.RankType.GrandChampion1 => "grand-champion-1",
                Grpc.RankType.GrandChampion2 => "grand-champion-2",
                Grpc.RankType.GrandChampion3 => "grand-champion-3",
                Grpc.RankType.SupersonicLegend => "supersonic-legend",
                Grpc.RankType.Unknown => throw new ArgumentException("Cannot parse Unknown RankType"),
                Grpc.RankType.NoRank => "unranked",
                _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, null)
            };
        }

        private static string ParseSeason(Grpc.Season season)
        {
            return (season.FreeToPlay ? "f" : string.Empty) + season.Number;
        }

        private static IEnumerable<string> GetAllPlaylists()
        {
            return new List<string>
            {
                "unranked-duels",
                "unranked-doubles",
                "unranked-standard",
                "unranked-chaos",
                "private",
                "season",
                "offline",
                "ranked-duels",
                "ranked-doubles",
                "ranked-solo-standard",
                "ranked-standard",
                "snowday",
                "rocketlabs",
                "hoops",
                "rumble",
                "tournament",
                "dropshot",
                "ranked-hoops",
                "ranked-rumble",
                "ranked-dropshot",
                "ranked-snowday",
                "dropshot-rumble",
                "heatseeker",
            };
        }

        public static ApiUrlCreator CreateSimpleIdentityFilter(Grpc.Identity identity)
        {
            return new ApiUrlCreator(new Grpc.FilterRequest
            {
                Identities = { identity },
                Playlist = Grpc.Playlist.All,
                MatchType = Grpc.MatchType.Both,
            });
        }
    }
}