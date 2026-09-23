-- with selected_matches(match_guid) as (
--     values
--         (x'9fc700ca11f1b7235ceb499490e3c53a'),
--         (x'000456e211f1b7228e7b2e8385745828'),
--         (x'ea17d7f611f1b720208c0cad736bcbec')
-- ),

session_matches as (
    select m.*
    from matches m
        join selected_matches sm on m.match_guid = sm.match_guid
),

my_teams as (
    select t.match_id,
        t.id as team_id
    from teams t
        join players p on p.team_id = t.id
    where p.global_player_id = ?1
),

enemy_teams as (
    select t.match_id,
        t.id as team_id
    from teams t
        join session_matches sm on t.match_id = sm.id
        join my_teams mt on mt.match_id = t.match_id
    where t.id <> mt.team_id
)

select 
    sm.match_guid,
    gp.primary_id,
    gp.last_username
from session_matches sm
    join enemy_teams et on et.match_id = sm.id
    join players p on p.match_id = sm.id
    and p.team_id = et.team_id
    join global_players gp on gp.id = p.global_player_id