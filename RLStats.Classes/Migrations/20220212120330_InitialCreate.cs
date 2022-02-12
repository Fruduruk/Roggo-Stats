using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RLStats_Classes.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Ball",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Possession_time = table.Column<float>(type: "float", nullable: true),
                    Time_in_side = table.Column<float>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ball", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Boost",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Bpm = table.Column<int>(type: "int", nullable: true),
                    Bcpm = table.Column<float>(type: "float", nullable: true),
                    Avg_amount = table.Column<float>(type: "float", nullable: true),
                    Amount_collected = table.Column<int>(type: "int", nullable: true),
                    Amount_stolen = table.Column<int>(type: "int", nullable: true),
                    Amount_collected_big = table.Column<int>(type: "int", nullable: true),
                    Amount_stolen_big = table.Column<int>(type: "int", nullable: true),
                    Amount_collected_small = table.Column<int>(type: "int", nullable: true),
                    Amount_stolen_small = table.Column<int>(type: "int", nullable: true),
                    Count_collected_big = table.Column<int>(type: "int", nullable: true),
                    Count_stolen_big = table.Column<int>(type: "int", nullable: true),
                    Count_collected_small = table.Column<int>(type: "int", nullable: true),
                    Count_stolen_small = table.Column<int>(type: "int", nullable: true),
                    Amount_overfill = table.Column<int>(type: "int", nullable: true),
                    Amount_overfill_stolen = table.Column<int>(type: "int", nullable: true),
                    Amount_used_while_supersonic = table.Column<int>(type: "int", nullable: true),
                    Time_zero_boost = table.Column<float>(type: "float", nullable: true),
                    Time_full_boost = table.Column<float>(type: "float", nullable: true),
                    Time_boost_0_25 = table.Column<float>(type: "float", nullable: true),
                    Time_boost_25_50 = table.Column<float>(type: "float", nullable: true),
                    Time_boost_50_75 = table.Column<float>(type: "float", nullable: true),
                    Time_boost_75_100 = table.Column<float>(type: "float", nullable: true),
                    Discriminator = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Percent_zero_boost = table.Column<float>(type: "float", nullable: true),
                    Percent_full_boost = table.Column<float>(type: "float", nullable: true),
                    Percent_boost_0_25 = table.Column<float>(type: "float", nullable: true),
                    Percent_boost_25_50 = table.Column<float>(type: "float", nullable: true),
                    Percent_boost_50_75 = table.Column<float>(type: "float", nullable: true),
                    Percent_boost_75_100 = table.Column<float>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boost", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Camera",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fov = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    Pitch = table.Column<int>(type: "int", nullable: true),
                    Distance = table.Column<int>(type: "int", nullable: true),
                    Stiffness = table.Column<float>(type: "float", nullable: true),
                    Swivel_speed = table.Column<float>(type: "float", nullable: true),
                    Transition_speed = table.Column<float>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Camera", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Core",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Shots = table.Column<int>(type: "int", nullable: true),
                    Shots_against = table.Column<int>(type: "int", nullable: true),
                    Goals = table.Column<int>(type: "int", nullable: true),
                    Goals_against = table.Column<int>(type: "int", nullable: true),
                    Saves = table.Column<int>(type: "int", nullable: true),
                    Assists = table.Column<int>(type: "int", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: true),
                    Shooting_percentage = table.Column<float>(type: "float", nullable: true),
                    Discriminator = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Mvp = table.Column<bool>(type: "tinyint(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Demo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Inflicted = table.Column<int>(type: "int", nullable: true),
                    Taken = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Demo", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GeneralMovement",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Total_distance = table.Column<int>(type: "int", nullable: true),
                    Time_supersonic_speed = table.Column<float>(type: "float", nullable: true),
                    Time_boost_speed = table.Column<float>(type: "float", nullable: true),
                    Time_slow_speed = table.Column<float>(type: "float", nullable: true),
                    Time_ground = table.Column<float>(type: "float", nullable: true),
                    Time_low_air = table.Column<float>(type: "float", nullable: true),
                    Time_high_air = table.Column<float>(type: "float", nullable: true),
                    Time_powerslide = table.Column<float>(type: "float", nullable: true),
                    Count_powerslide = table.Column<int>(type: "int", nullable: true),
                    Discriminator = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Avg_speed = table.Column<int>(type: "int", nullable: true),
                    Avg_powerslide_duration = table.Column<float>(type: "float", nullable: true),
                    Avg_speed_percentage = table.Column<float>(type: "float", nullable: true),
                    Percent_slow_speed = table.Column<float>(type: "float", nullable: true),
                    Percent_boost_speed = table.Column<float>(type: "float", nullable: true),
                    Percent_supersonic_speed = table.Column<float>(type: "float", nullable: true),
                    Percent_ground = table.Column<float>(type: "float", nullable: true),
                    Percent_low_air = table.Column<float>(type: "float", nullable: true),
                    Percent_high_air = table.Column<float>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralMovement", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GeneralPositioning",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Time_defensive_third = table.Column<float>(type: "float", nullable: true),
                    Time_neutral_third = table.Column<float>(type: "float", nullable: true),
                    Time_offensive_third = table.Column<float>(type: "float", nullable: true),
                    Time_defensive_half = table.Column<float>(type: "float", nullable: true),
                    Time_offensive_half = table.Column<float>(type: "float", nullable: true),
                    Time_behind_ball = table.Column<float>(type: "float", nullable: true),
                    Time_infront_ball = table.Column<float>(type: "float", nullable: true),
                    Discriminator = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Avg_distance_to_ball = table.Column<int>(type: "int", nullable: true),
                    Avg_distance_to_ball_possession = table.Column<int>(type: "int", nullable: true),
                    Avg_distance_to_ball_no_possession = table.Column<int>(type: "int", nullable: true),
                    Avg_distance_to_mates = table.Column<int>(type: "int", nullable: true),
                    Time_most_back = table.Column<float>(type: "float", nullable: true),
                    Time_most_forward = table.Column<float>(type: "float", nullable: true),
                    Time_closest_to_ball = table.Column<float>(type: "float", nullable: true),
                    Time_farthest_from_ball = table.Column<float>(type: "float", nullable: true),
                    Percent_defensive_third = table.Column<float>(type: "float", nullable: true),
                    Percent_offensive_third = table.Column<float>(type: "float", nullable: true),
                    Percent_neutral_third = table.Column<float>(type: "float", nullable: true),
                    Percent_defensive_half = table.Column<float>(type: "float", nullable: true),
                    Percent_offensive_half = table.Column<float>(type: "float", nullable: true),
                    Percent_behind_ball = table.Column<float>(type: "float", nullable: true),
                    Percent_infront_ball = table.Column<float>(type: "float", nullable: true),
                    Percent_most_back = table.Column<float>(type: "float", nullable: true),
                    Percent_most_forward = table.Column<float>(type: "float", nullable: true),
                    Percent_closest_to_ball = table.Column<float>(type: "float", nullable: true),
                    Percent_farthest_from_ball = table.Column<float>(type: "float", nullable: true),
                    Goals_against_while_last_defender = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralPositioning", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Id",
                columns: table => new
                {
                    ID = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Platform = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Id", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Team",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Goals = table.Column<int>(type: "int", nullable: false),
                    InitialTeamSize = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Team", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Uploader",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Steam_id = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Profile_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Avatar = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uploader", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PlayerStats",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlayerCoreId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlayerBoostId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlayerMovementId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlayerPositioningId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DemoId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerStats_Boost_PlayerBoostId",
                        column: x => x.PlayerBoostId,
                        principalTable: "Boost",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlayerStats_Core_PlayerCoreId",
                        column: x => x.PlayerCoreId,
                        principalTable: "Core",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlayerStats_Demo_DemoId",
                        column: x => x.DemoId,
                        principalTable: "Demo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlayerStats_GeneralMovement_PlayerMovementId",
                        column: x => x.PlayerMovementId,
                        principalTable: "GeneralMovement",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlayerStats_GeneralPositioning_PlayerPositioningId",
                        column: x => x.PlayerPositioningId,
                        principalTable: "GeneralPositioning",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TeamStats",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BallId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CoreId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BoostId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MovementId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PositioningId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DemoId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamStats_Ball_BallId",
                        column: x => x.BallId,
                        principalTable: "Ball",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamStats_Boost_BoostId",
                        column: x => x.BoostId,
                        principalTable: "Boost",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamStats_Core_CoreId",
                        column: x => x.CoreId,
                        principalTable: "Core",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamStats_Demo_DemoId",
                        column: x => x.DemoId,
                        principalTable: "Demo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamStats_GeneralMovement_MovementId",
                        column: x => x.MovementId,
                        principalTable: "GeneralMovement",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamStats_GeneralPositioning_PositioningId",
                        column: x => x.PositioningId,
                        principalTable: "GeneralPositioning",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Player",
                columns: table => new
                {
                    Name = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartTime = table.Column<double>(type: "double", nullable: false),
                    EndTime = table.Column<double>(type: "double", nullable: false),
                    MVP = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Player", x => x.Name);
                    table.ForeignKey(
                        name: "FK_Player_Team_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Replays",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RocketLeagueId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SeasonType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Visibility = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Link = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Playlist = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Season = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    Uploader = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BlueId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrangeId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Replays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Replays_Team_BlueId",
                        column: x => x.BlueId,
                        principalTable: "Team",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Replays_Team_OrangeId",
                        column: x => x.OrangeId,
                        principalTable: "Team",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AdvancedTeam",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatsId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedTeam", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancedTeam_TeamStats_StatsId",
                        column: x => x.StatsId,
                        principalTable: "TeamStats",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AdvancedPlayer",
                columns: table => new
                {
                    Name = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Start_time = table.Column<int>(type: "int", nullable: true),
                    End_time = table.Column<float>(type: "float", nullable: true),
                    ID = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Car_id = table.Column<int>(type: "int", nullable: true),
                    Car_name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CameraId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Steering_sensitivity = table.Column<int>(type: "int", nullable: true),
                    StatsId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdvancedTeamId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedPlayer", x => x.Name);
                    table.ForeignKey(
                        name: "FK_AdvancedPlayer_AdvancedTeam_AdvancedTeamId",
                        column: x => x.AdvancedTeamId,
                        principalTable: "AdvancedTeam",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdvancedPlayer_Camera_CameraId",
                        column: x => x.CameraId,
                        principalTable: "Camera",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdvancedPlayer_Id_ID",
                        column: x => x.ID,
                        principalTable: "Id",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_AdvancedPlayer_PlayerStats_StatsId",
                        column: x => x.StatsId,
                        principalTable: "PlayerStats",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AdvancedReplays",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Link = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    UploaderId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rocket_league_id = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Match_guid = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Map_code = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Match_type = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Team_size = table.Column<int>(type: "int", nullable: true),
                    Playlist_id = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    Overtime = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    Overtime_seconds = table.Column<int>(type: "int", nullable: true),
                    Season = table.Column<int>(type: "int", nullable: true),
                    Season_type = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    Visibility = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BlueId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrangeId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Playlist_name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Map_name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedReplays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancedReplays_AdvancedTeam_BlueId",
                        column: x => x.BlueId,
                        principalTable: "AdvancedTeam",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdvancedReplays_AdvancedTeam_OrangeId",
                        column: x => x.OrangeId,
                        principalTable: "AdvancedTeam",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdvancedReplays_Uploader_UploaderId",
                        column: x => x.UploaderId,
                        principalTable: "Uploader",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedPlayer_AdvancedTeamId",
                table: "AdvancedPlayer",
                column: "AdvancedTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedPlayer_CameraId",
                table: "AdvancedPlayer",
                column: "CameraId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedPlayer_ID",
                table: "AdvancedPlayer",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedPlayer_StatsId",
                table: "AdvancedPlayer",
                column: "StatsId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedReplays_BlueId",
                table: "AdvancedReplays",
                column: "BlueId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedReplays_OrangeId",
                table: "AdvancedReplays",
                column: "OrangeId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedReplays_UploaderId",
                table: "AdvancedReplays",
                column: "UploaderId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedTeam_StatsId",
                table: "AdvancedTeam",
                column: "StatsId");

            migrationBuilder.CreateIndex(
                name: "IX_Player_TeamId",
                table: "Player",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStats_DemoId",
                table: "PlayerStats",
                column: "DemoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStats_PlayerBoostId",
                table: "PlayerStats",
                column: "PlayerBoostId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStats_PlayerCoreId",
                table: "PlayerStats",
                column: "PlayerCoreId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStats_PlayerMovementId",
                table: "PlayerStats",
                column: "PlayerMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStats_PlayerPositioningId",
                table: "PlayerStats",
                column: "PlayerPositioningId");

            migrationBuilder.CreateIndex(
                name: "IX_Replays_BlueId",
                table: "Replays",
                column: "BlueId");

            migrationBuilder.CreateIndex(
                name: "IX_Replays_OrangeId",
                table: "Replays",
                column: "OrangeId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamStats_BallId",
                table: "TeamStats",
                column: "BallId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamStats_BoostId",
                table: "TeamStats",
                column: "BoostId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamStats_CoreId",
                table: "TeamStats",
                column: "CoreId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamStats_DemoId",
                table: "TeamStats",
                column: "DemoId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamStats_MovementId",
                table: "TeamStats",
                column: "MovementId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamStats_PositioningId",
                table: "TeamStats",
                column: "PositioningId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvancedPlayer");

            migrationBuilder.DropTable(
                name: "AdvancedReplays");

            migrationBuilder.DropTable(
                name: "Player");

            migrationBuilder.DropTable(
                name: "Replays");

            migrationBuilder.DropTable(
                name: "Camera");

            migrationBuilder.DropTable(
                name: "Id");

            migrationBuilder.DropTable(
                name: "PlayerStats");

            migrationBuilder.DropTable(
                name: "AdvancedTeam");

            migrationBuilder.DropTable(
                name: "Uploader");

            migrationBuilder.DropTable(
                name: "Team");

            migrationBuilder.DropTable(
                name: "TeamStats");

            migrationBuilder.DropTable(
                name: "Ball");

            migrationBuilder.DropTable(
                name: "Boost");

            migrationBuilder.DropTable(
                name: "Core");

            migrationBuilder.DropTable(
                name: "Demo");

            migrationBuilder.DropTable(
                name: "GeneralMovement");

            migrationBuilder.DropTable(
                name: "GeneralPositioning");
        }
    }
}
