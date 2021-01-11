using RocketLeagueStats.AdvancedModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocketLeagueStats
{
    public class AdvancedReplayAssembler
    {
        public dynamic JData { get; }
        public AdvancedReplayAssembler(dynamic jData)
        {
            JData = jData;
        }
        public AdvancedReplay Assemble()
        {
            var r = new AdvancedReplay();
            r.Id = JData.id;
            r.Link = JData.link;
            r.Created = Convert.ToDateTime(JData.created);
            r.Uploader = new Uploader()
            {
                Steam_id = JData.uploader.steam_id,
                Name = JData.uploader.name,
                Profile_url = JData.uploader.profile_url,
                Avatar = JData.uploader.avatar
            };
            r.Status = JData.status;
            r.Rocket_league_id = JData.rocket_league_id;
            r.Match_guid = JData.match_gui;
            r.Title = JData.title;
            r.Map_code = JData.map_code;
            r.Match_type = JData.match_type;
            r.Team_size = JData.team_size;
            r.Playlist_id = JData.playlist_id;
            r.Duration = JData.duration;
            r.Overtime = JData.overtime;
            r.Overtime_seconds = JData.overtime_seconds;
            r.Season = JData.season;
            r.Date = Convert.ToDateTime(JData.date);
            r.Visibility = JData.visibility;
            r.Blue = GetTeam(JData.blue);
            r.Orange = GetTeam(JData.orange);
            r.Playlist_name = JData.playlist_name;
            r.Map_name = JData.map_name;
            return r;
        }

        private AdvancedTeam GetTeam(dynamic dTeam)
        {
            var team = new AdvancedTeam();
            team.Color = dTeam.color;
            if (dTeam.players != null)
                team.Players = GetPlayers(dTeam.players);
            if (dTeam.stats != null)
                team.Stats = GetTeamStats(dTeam.stats);
            return team;
        }

        private List<AdvancedPlayer> GetPlayers(dynamic players)
        {
            List<AdvancedPlayer> playerList = new List<AdvancedPlayer>();
            foreach (var p in players)
            {
                AdvancedPlayer player = new AdvancedPlayer()
                {
                    Start_time = p.start_time,
                    End_time = p.end_time,
                    Name = p.name,
                    Id = GetPlayerID(p.id),
                    Car_id = p.car_id,
                    Car_name = p.car_name,
                    Camera = GetPlayerCamera(p.camera),
                    Steering_sensitivity = p.steering_sensitivity,
                    Stats = GetPlayerStats(p.stats)
                };
                playerList.Add(player);
            }
            return playerList;
        }

        private PlayerStats GetPlayerStats(dynamic stats)
        {
            return new PlayerStats()
            {
                PlayerCore = GetPlayerCore(stats.core),
                PlayerBoost = GetPlayerBoost(stats.boost),
                PlayerMovement = GetPlayerMovement(stats.movement),
                PlayerPositioning = GetPlayerPositioning(stats.positioning),
                Demo = GetDemo(stats.demo)
            };
        }

        private PlayerPositioning GetPlayerPositioning(dynamic pos)
        {
            return new PlayerPositioning()
            {
                Time_defensive_third = pos.time_defensive_third,
                Time_neutral_third = pos.time_neutral_third,
                Time_offensive_third = pos.time_offensive_third,
                Time_offensive_half = pos.time_offensive_half,
                Time_defensive_half = pos.time_defensive_half,
                Time_behind_ball = pos.time_behind_ball,
                Time_infront_ball = pos.time_infront_ball,
                Avg_distance_to_ball = pos.avg_distance_to_ball,
                Avg_distance_to_ball_no_possession = pos.avg_distance_to_ball_no_possession,
                Avg_distance_to_ball_possession = pos.avg_distance_to_ball_possession,
                Avg_distance_to_mates = pos.avg_distance_to_mates,
                Goals_against_while_last_defender = pos.goals_against_while_last_defender,
                Percent_behind_ball = pos.percent_behind_ball,
                Percent_closest_to_ball = pos.percent_closest_to_ball,
                Percent_defensive_half = pos.percent_defensive_half,
                Percent_defensive_third = pos.percent_defensive_third,
                Percent_farthest_from_ball = pos.percent_farthest_from_ball,
                Percent_infront_ball = pos.percent_infront_ball,
                Percent_most_back = pos.percent_most_back,
                Percent_most_forward = pos.percent_most_forward,
                Percent_neutral_third = pos.percent_neutral_third,
                Percent_offensive_half = pos.percent_offensive_half,
                Percent_offensive_third = pos.percent_offensive_third,
                Time_closest_to_ball = pos.time_closest_to_ball,
                Time_farthest_from_ball = pos.time_farthest_from_ball,
                Time_most_back = pos.time_most_back,
                Time_most_forward = pos.time_most_forward
            };
        }

        private PlayerMovement GetPlayerMovement(dynamic movement)
        {
            return new PlayerMovement()
            {
                Total_distance = movement.total_distance,
                Time_supersonic_speed = movement.time_supersonic_speed,
                Time_boost_speed = movement.time_boost_speed,
                Time_slow_speed = movement.time_slow_speed,
                Time_ground = movement.time_ground,
                Time_low_air = movement.time_low_air,
                Time_high_air = movement.time_high_air,
                Time_powerslide = movement.time_powerslide,
                Count_powerslide = movement.count_powerslide,
                Avg_powerslide_duration = movement.avg_powerslide_duration,
                Avg_speed = movement.avg_speed,
                Avg_speed_percentage = movement.avg_speed_percentage,
                Percent_boost_speed = movement.percent_boost_speed,
                Percent_ground = movement.percent_ground,
                Percent_low_air = movement.percent_low_air,
                Percent_high_air = movement.percent_high_air,
                Percent_slow_speed = movement.percent_slow_speed,
                Percent_supersonic_speed = movement.percent_supersonic_speed
            };
        }

        private PlayerBoost GetPlayerBoost(dynamic boost)
        {
            return new PlayerBoost()
            {
                Bpm = boost.bpm,
                Bcpm = boost.bcpm,
                Avg_amount = boost.avg_amount,
                Amount_collected = boost.amount_collected,
                Amount_stolen = boost.amount_stolen,
                Amount_collected_big = boost.amount_collected_big,
                Amount_stolen_big = boost.amount_stolen_big,
                Amount_collected_small = boost.amount_collected_small,
                Amount_stolen_small = boost.amount_stolen_small,
                Count_collected_big = boost.count_collected_big,
                Count_stolen_big = boost.count_stolen_big,
                Count_collected_small = boost.count_collected_small,
                Count_stolen_small = boost.count_stolen_small,
                Amount_overfill = boost.amount_overfill,
                Amount_overfill_stolen = boost.amount_overfill_stolen,
                Amount_used_while_supersonic = boost.amount_used_while_supersonic,
                Time_zero_boost = boost.time_zero_boost,
                Time_full_boost = boost.time_full_boost,
                Time_boost_0_25 = boost.time_boost_0_25,
                Time_boost_25_50 = boost.time_boost_25_50,
                Time_boost_50_75 = boost.time_boost_50_75,
                Time_boost_75_100 = boost.time_boost_75_100,
                Percent_boost_0_25 = boost.percent_boost_0_25,
                Percent_boost_25_50 = boost.percent_boost_25_50,
                Percent_boost_50_75 = boost.percent_boost_50_75,
                Percent_boost_75_100 = boost.percent_boost_75_100,
                Percent_full_boost = boost.percent_full_boost,
                Percent_zero_boost = boost.percent_zero_boost
            };
        }

        private PlayerCore GetPlayerCore(dynamic core)
        {
            return new PlayerCore()
            {
                Shots = core.shots,
                Shots_against = core.shots_against,
                Goals = core.goals,
                Goals_against = core.goals_against,
                Saves = core.saves,
                Assists = core.assists,
                Score = core.score,
                MVP = core.mvp,
                Shooting_percentage = core.shooting_percentage
            };
        }

        private Camera GetPlayerCamera(dynamic camera)
        {
            return new Camera()
            {
                Fov = camera.fov,
                Height = camera.height,
                Pitch = camera.pitch,
                Distance = camera.distance,
                Stiffness = camera.stiffness,
                Swivel_speed = camera.swivel_speed,
                Transition_speed = camera.transition_speed
            };
        }

        private Id GetPlayerID(dynamic id)
        {
            return new Id()
            {
                Platform = id.platform,
                ID = id.id
            };
        }

        private TeamStats GetTeamStats(dynamic stats)
        {
            var ts = new TeamStats();
            ts.Ball = new Ball()
            {
                Possession_time = stats.ball.possession_time,
                Time_in_side = stats.ball.time_in_side
            };
            ts.Core = GetTeamCore(stats.core);
            ts.Boost = GetTeamBoost(stats.boost);
            ts.Movement = GetTeamMovement(stats.movement);
            ts.Positioning = GetTeamPositioning(stats.positioning);
            ts.Demo = GetDemo(stats.demo);
            return ts;
        }

        private GeneralPositioning GetTeamPositioning(dynamic positioning)
        {
            return new GeneralPositioning()
            {
                Time_defensive_third = positioning.time_defensive_third,
                Time_neutral_third = positioning.time_neutral_third,
                Time_offensive_third = positioning.time_offensive_third,
                Time_offensive_half = positioning.time_offensive_half,
                Time_defensive_half = positioning.time_defensive_half,
                Time_behind_ball = positioning.time_behind_ball,
                Time_infront_ball = positioning.time_infront_ball
            };
        }

        private GeneralMovement GetTeamMovement(dynamic movement)
        {
            return new GeneralMovement()
            {
                Total_distance = movement.total_distance,
                Time_supersonic_speed = movement.time_supersonic_speed,
                Time_boost_speed = movement.time_boost_speed,
                Time_slow_speed = movement.time_slow_speed,
                Time_ground = movement.time_ground,
                Time_low_air = movement.time_low_air,
                Time_high_air = movement.time_high_air,
                Time_powerslide = movement.time_powerslide,
                Count_powerslide = movement.count_powerslide
            };
        }

        private Boost GetTeamBoost(dynamic boost)
        {
            return new Boost()
            {
                Bpm = boost.bpm,
                Bcpm = boost.bcpm,
                Avg_amount = boost.avg_amount,
                Amount_collected = boost.amount_collected,
                Amount_stolen = boost.amount_stolen,
                Amount_collected_big = boost.amount_collected_big,
                Amount_stolen_big = boost.amount_stolen_big,
                Amount_collected_small = boost.amount_collected_small,
                Amount_stolen_small = boost.amount_stolen_small,
                Count_collected_big = boost.count_collected_big,
                Count_stolen_big = boost.count_stolen_big,
                Count_collected_small = boost.count_collected_small,
                Count_stolen_small = boost.count_stolen_small,
                Amount_overfill = boost.amount_overfill,
                Amount_overfill_stolen = boost.amount_overfill_stolen,
                Amount_used_while_supersonic = boost.amount_used_while_supersonic,
                Time_zero_boost = boost.time_zero_boost,
                Time_full_boost = boost.time_full_boost,
                Time_boost_0_25 = boost.time_boost_0_25,
                Time_boost_25_50 = boost.time_boost_25_50,
                Time_boost_50_75 = boost.time_boost_50_75,
                Time_boost_75_100 = boost.time_boost_75_100
            };
        }

        private Core GetTeamCore(dynamic core)
        {
            return new Core()
            {
                Shots = core.shots,
                Shots_against = core.shots_against,
                Goals = core.goals,
                Goals_against = core.goals_against,
                Saves = core.saves,
                Assists = core.assists,
                Score = core.score,
                Shooting_percentage = core.shooting_percentage
            };
        }

        private Demo GetDemo(dynamic demo)
        {
            return new Demo()
            {
                Inflicted = demo.inflicted,
                Taken = demo.taken
            };
        }
    }
}
