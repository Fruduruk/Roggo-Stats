-- explain query plan
-- with selected_matches(match_guid) as (
--     values
--         (x'81dabd4e11f151359589109f47dddbaa'),
--         (x'64d6ede811f151367c9419a03b5c4973'),
--         (x'05a1348611f1513730f887b4613b6737'),
--         (x'11ea0bf411f15138d2dc8c9ba58fab1b'),
--         (x'1810750811f151390fd7cdae1b760370'),
--         (x'08dcdf6c11f1513add4551b80ba4e52f'),
--         (x'1561ab1811f1513b17758ab36d034fcf'),
--         (x'1ecfc4ea11f1513c4d7aa685f8fc2425'),
--         (x'7267348e11f1513df36d0f8395a970f4'),
--         (x'99c6b52611f1513ea64be8b3d39cec1d'),
--         (x'8dba82a211f1513f95c6ff869226b33d'),
--         (x'889232ce11f15140c545cea79ba46d2f')
-- ),

own_teams as (
    select
        own_team.id
    from selected_matches sm
    join matches m
        on m.match_guid = sm.match_guid
    join players main_character
        on main_character.match_id = m.id
        and main_character.global_player_id = 4
    join teams own_team
        on own_team.match_id = m.id
        and own_team.id = main_character.team_id
),

team_player_stats as (
    select
        m.duration,
        ps.*
    from own_teams ot
    join players p
        on p.team_id = ot.id
        and p.global_player_id <> 4
    join matches m
        on m.id = p.match_id
    left join player_stats ps
        on ps.player_id = p.id
),

aggregated as (
    select
        avg(1.0 * tps.time_boosting / tps.duration) as average_percent_boosting,
        avg(1.0 * tps.time_demolished / tps.duration) as average_percent_demolished,
        avg(1.0 * tps.time_on_ground / tps.duration) as average_percent_on_ground,
        avg(1.0 * tps.time_on_wall / tps.duration) as average_percent_on_wall,
        avg(1.0 * tps.time_powersliding / tps.duration) as average_percent_powersliding,
        avg(1.0 * tps.time_supersonic / tps.duration) as average_percent_supersonic
    from team_player_stats tps
)

select * from aggregated
