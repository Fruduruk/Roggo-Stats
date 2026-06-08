-- explain query plan

with selected_matches(match_guid) as (
    values
        (x'81dabd4e11f151359589109f47dddbaa'),
        (x'64d6ede811f151367c9419a03b5c4973'),
        (x'05a1348611f1513730f887b4613b6737'),
        (x'11ea0bf411f15138d2dc8c9ba58fab1b'),
        (x'1810750811f151390fd7cdae1b760370'),
        (x'08dcdf6c11f1513add4551b80ba4e52f'),
        (x'1561ab1811f1513b17758ab36d034fcf'),
        (x'1ecfc4ea11f1513c4d7aa685f8fc2425'),
        (x'7267348e11f1513df36d0f8395a970f4'),
        (x'99c6b52611f1513ea64be8b3d39cec1d'),
        (x'8dba82a211f1513f95c6ff869226b33d'),
        (x'889232ce11f15140c545cea79ba46d2f')
),

enemy_teams as (
    select
        enemy_team.id
    from selected_matches sm
    join matches m
        on m.match_guid = sm.match_guid
    join players main_character
        on main_character.match_id = m.id
        and main_character.global_player_id = 4
    join teams enemy_team
        on enemy_team.match_id = m.id
        and enemy_team.id <> main_character.team_id
),

enemy_players as (
    select
        p.*
    from enemy_teams et
    join players p
        on p.team_id = et.id
),

aggregated as (
    select
        avg(ep.score) as average_score,
        avg(ep.goals) as average_goals,
        avg(ep.shots) as average_shots,
        avg(ep.assists) as average_assists,
        avg(ep.saves) as average_saves,
        avg(ep.demos) as average_demos
    from enemy_players ep
)

select * from aggregated
