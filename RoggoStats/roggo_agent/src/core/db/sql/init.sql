create table if not exists matches (
    id integer primary key,
    match_guid uuid not null,
    arena text not null,
    duration integer not null,
    created_at_ms integer not null,
    ended_at_ms integer not null,
    had_overtime boolean not null,
    deleted boolean not null,
    unique(match_guid)
);

create table if not exists match_metadata (
    match_id integer primary key,
    schema_version integer not null,
    agent_version text not null,
    saved_at_ms integer not null,
    foreign key (match_id) references matches(id)
);

create table if not exists errors (
    id integer primary key,
    match_id integer not null,
    error text not null,
    foreign key (match_id) references matches(id)
);

create table if not exists teams (
    id integer primary key,
    match_id integer not null,
    team_num integer not null,
    name text not null,
    score integer not null,
    color_primary text not null,
    color_secondary text not null,
    unique(match_id, team_num),
    foreign key (match_id) references matches(id)
);

create index if not exists idx_teams_match_id
on teams(match_id);

create table if not exists global_players (
    id integer primary key,
    primary_id text not null unique,
    last_username integer not null
);

create table if not exists players (
    id integer primary key,
    match_id integer not null,
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
    
    unique(match_id, global_player_id),
    foreign key (match_id) references matches(id),
    foreign key (team_id) references teams(id),
    foreign key (global_player_id) references global_players(id)
);

create index if not exists idx_players_match_id
on players(match_id);

create index if not exists idx_players_global_player_id
on players(global_player_id);

create table if not exists player_stats (
    player_id integer primary key,
    time_boosting integer not null,
    time_demolished integer not null,
    time_on_ground integer not null,
    time_on_wall integer not null,
    time_powersliding integer not null,
    time_supersonic integer not null,
    foreign key (player_id) references players(id)
);

create index if not exists idx_player_stats_player_id
on player_stats(player_id);

create table if not exists goal_details (
    id integer primary key,
    match_id integer not null,
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
    unique(match_id,timestamp_ms),
    foreign key (match_id) references matches(id),
    foreign key (scorer_player_id) references players(id),
    foreign key (assister_player_id) references players(id),
    foreign key (last_touch_player_id) references players(id)
);

create index if not exists idx_goal_details_match_time
on goal_details(match_id, timestamp_ms);

create table if not exists ball_hits (
    id integer primary key,
    match_id integer not null,
    timestamp_ms integer not null,
    pre_hit_speed real not null,
    post_hit_speed real not null,
    x real not null,
    y real not null,
    z real not null,
    foreign key (match_id) references matches(id)
);

create index if not exists idx_ball_hits_match_time
on ball_hits(match_id, timestamp_ms);

create table if not exists ball_hit_players (
    ball_hit_id integer not null,
    player_id integer not null,
    primary key (ball_hit_id, player_id),
    foreign key (ball_hit_id) references ball_hits(id),
    foreign key (player_id) references players(id)
);

create table if not exists crossbar_hits (
    id integer primary key,
    match_id integer not null,
    timestamp_ms integer not null,
    ball_speed real not null,
    impact_force real not null,
    x real not null,
    y real not null,
    z real not null,
    last_touch_speed real not null,
    last_touch_player_id integer not null,
    foreign key (match_id) references matches(id),
    foreign key (last_touch_player_id) references players(id)
);

create index if not exists idx_crossbar_hits_match_time
on crossbar_hits(match_id, timestamp_ms);

create table if not exists clock_samples (
    id integer primary key,
    match_id integer not null,
    timestamp_ms integer not null,
    time_seconds integer not null,
    is_overtime boolean not null,
    foreign key (match_id) references matches(id)
);

create index if not exists idx_clock_samples_match_time
on clock_samples(match_id, timestamp_ms);

create table if not exists statfeed_events (
    id integer primary key,
    match_id integer not null,
    timestamp_ms integer not null,

    event_name text not null,
    event_type text not null,

    main_target_player_id integer not null,
    secondary_target_player_id integer,

    foreign key (match_id) references matches(id),
    foreign key (main_target_player_id) references players(id),
    foreign key (secondary_target_player_id) references players(id)
);

create index if not exists idx_statfeed_events_match_time
on statfeed_events(match_id, timestamp_ms);

create table if not exists timeline (
    id integer primary key,
    match_id integer not null,
    timestamp_ms integer not null,
    last_touch_team_id integer,
    ball_speed real not null,
    
    foreign key (last_touch_team_id) references teams(id),
    foreign key (match_id) references matches(id)
);

create index if not exists idx_timeline_match_time
on timeline(match_id, timestamp_ms);