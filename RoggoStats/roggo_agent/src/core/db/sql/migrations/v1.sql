-- 1.1: change global_players attribute last_username from integer to text

create table global_players_new (
    id integer primary key,
    primary_id text not null unique,
    last_username text not null
);

insert into global_players_new (id, primary_id, last_username)
select id, primary_id, cast(last_username as text)
from global_players;
drop table global_players;

alter table global_players_new RENAME to global_players;

-- 1.2: drop timeline table because not in use, saving 85% file size

drop table timeline;

-- 1.3: create migration history table
create table migration_history (
    version integer primary key,
    name text not null,
    applied_at_ms integer not null,
    agent_version text not null,
    execution_time_ms integer not null
);

-- 1.4: update match metadata versions to 0
update match_metadata
set schema_version = 0;