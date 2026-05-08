create table if not exists matches (
    match_guid uuid not null primary key,
    arena text,
    duration integer not null,
    created_at_ms integer not null,
    ended_at_ms integer not null,
    had_overtime boolean not null,
    deleted boolean not null
);
create table if not exists teams (
    id integer not null primary key autoincrement,
    match_guid uuid not null,
    team_num integer not null,
    name text not null,
    score integer not null,
    color_primary text not null,
    color_secondary text not null,
    foreign key (match_guid) references matches(match_guid)
);
create table if not exists global_players (
    id integer not null primary key autoincrement,
    primary_id text not null unique,
    username text not null
);
create table if not exists players (
    id integer not null primary key autoincrement,
    match_guid uuid not null,
    team_id integer not null,
    global_player_id integer not null,
    display_name text not null,
    shortcut integer not null,
    score integer not null,
    goals integer not null,
    shots integer not null,
    assists integer not null,
    saves integer not null,
    touches integer not null,
    car_touches integer not null,
    demos integer not null,
    
    unique(match_guid, global_player_id),
    foreign key (match_guid) references matches(match_guid),
    foreign key (team_id) references teams(id),
    foreign key (global_player_id) references global_players(primary_id)
);
create table if not exists player_stats (
    player_id integer not null primary key,
    time_boosting integer not null,
    time_demolished integer not null,
    time_on_ground integer not null,
    time_on_wall integer not null,
    time_powersliding integer not null,
    time_supersonic integer not null,
    foreign key (player_id) references players(id)
);
create table if not exists goal_details (
    id integer not null primary key autoincrement,
    match_guid uuid not null,
    timestamp_ms integer not null,
    goal_time real not null,
    impact_x real not null,
    impact_y real not null,
    impact_z real not null,
    goal_speed real not null,
    scorer_player_id integer not null,
    assister_player_id integer,
    last_touch_player_id integer not null,
    last_touch_speed real,
    foreign key (match_guid) references matches(match_guid),
    foreign key (scorer_player_id) references players(id),
    foreign key (assister_player_id) references players(id),
    foreign key (last_touch_player_id) references players(id)
);
create table if not exists ball_hits (
    id integer not null primary key autoincrement,
    match_guid uuid not null,
    timestamp_ms integer not null,
    pre_hit_speed real not null,
    post_hit_speed real not null,
    x real not null,
    y real not null,
    z real not null,
    foreign key (match_guid) references matches(match_guid)
);
create table if not exists ball_hit_players (
    ball_hit_id integer not null,
    player_id integer not null,
    primary key (ball_hit_id, player_id),
    foreign key (ball_hit_id) references ball_hits(id),
    foreign key (player_id) references players(id)
);
create table if not exists crossbar_hits (
    id integer not null primary key autoincrement,
    match_guid uuid not null,
    timestamp_ms integer not null,
    ball_speed real not null,
    impact_force real not null,
    x real not null,
    y real not null,
    z real not null,
    last_touch_speed real not null,
    last_touch_player_id integer not null,
    foreign key (match_guid) references matches(match_guid),
    foreign key (last_touch_player_id) references players(id)
);
create table if not exists clock_samples (
    id integer not null primary key autoincrement,
    match_guid uuid not null,
    timestamp_ms integer not null,
    time_seconds integer not null,
    is_overtime boolean not null,
    foreign key (match_guid) references matches(match_guid)
);
create table if not exists statfeed_events (
    id integer not null primary key autoincrement,
    match_guid uuid not null,
    timestamp_ms integer not null,

    event_name text not null,
    event_type text not null,

    main_target_player_id integer not null,
    secondary_target_player_id integer,

    foreign key (match_guid) references matches(match_guid),
    foreign key (main_target_player_id) references players(id),
    foreign key (secondary_target_player_id) references players(id)
);

create table if not exists timeline (
    id integer not null primary key autoincrement,
    match_guid uuid not null,
    timestamp_ms integer not null,
    last_touch_team_id integer,
    ball_speed real not null,
    
    foreign key (last_touch_team_id) references teams(id),
    foreign key (match_guid) references matches(match_guid)
);