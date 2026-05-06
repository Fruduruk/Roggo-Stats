create table if not exists matches (
    match_guid uuid not null primary key,
    arena text,
    created_at_ms integer not null,
    ended_at_ms integer not null,
    had_overtime boolean not null
);

create table if not exists teams (
    match_guid uuid not null,
    team_num integer not null,
    name text not null,
    score integer not null,
    color_primary text not null,
    color_secondary text not null,
    primary key (match_guid, team_num),
    foreign key (match_guid) references matches(match_guid) on delete cascade
);

create table if not exists players (
    primary_id text not null primary key,
    username text not null
);

create table if not exists player_stats (
    match_guid uuid not null,
    team_num integer not null,
    player_id text not null,

    shortcut integer not null,

    score integer not null,
    goals integer not null,
    shots integer not null,
    assists integer not null,
    saves integer not null,
    touches integer not null,
    car_touches integer not null,
    demos integer not null,

    time_boosting integer,
    time_demolished integer,
    time_on_ground integer,
    time_on_wall integer,
    time_powersliding integer,
    time_supersonic integer,

    primary key (match_guid, team_num, player_id),
    foreign key (match_guid) references matches(match_guid) on delete cascade,
    foreign key (match_guid, team_num) references teams(match_guid, team_num) on delete cascade,
    foreign key (player_id) references players(primary_id) on delete cascade
);