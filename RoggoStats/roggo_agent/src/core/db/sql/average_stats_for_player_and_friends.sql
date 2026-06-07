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

own_teams as (
    select
        m.id as match_id,
        p.team_id as own_team_id,
        m.duration
    from selected_matches sm
    join matches m
        on m.match_guid = sm.match_guid
    join players p
        on p.match_id = m.id
    where p.global_player_id = 4
),

team_players as (
    select
        p.*,
        ot.duration
    from own_teams ot
    join players p
        on p.match_id = ot.match_id
        and p.team_id = ot.own_team_id
),

relevant_players as (
    select
        tp.global_player_id
    from team_players tp
    group by tp.global_player_id
    having count(*) = (select count(*) from own_teams)
),


aggregated as (
    select
        tp.global_player_id,

        count(*) as matches_played,

        avg(tp.score) as average_score,
        avg(tp.goals) as average_goals,
        avg(tp.shots) as average_shots,
        avg(tp.assists) as average_assists,
        avg(tp.saves) as average_saves,
        avg(tp.touches) as average_touches,
        avg(tp.car_touches) as average_car_touches,
        avg(tp.demos) as average_demos,

        avg(1.0 * ps.time_boosting / tp.duration) as average_time_boosting,
        avg(1.0 * ps.time_demolished/ tp.duration) as average_time_demolished,
        avg(1.0 * ps.time_on_ground/ tp.duration) as average_time_on_ground,
        avg(1.0 * ps.time_on_wall/ tp.duration) as average_time_on_wall,
        avg(1.0 * ps.time_powersliding/ tp.duration) as average_time_powersliding,
        avg(1.0 * ps.time_supersonic/ tp.duration) as average_time_supersonic

    from team_players tp
    join relevant_players rp
        on rp.global_player_id = tp.global_player_id
    left join player_stats ps
        on ps.player_id = tp.id
    group by tp.global_player_id
)

select
    a.global_player_id,
    gp.last_username,
    a.matches_played,

    a.average_score,
    a.average_goals,
    a.average_shots,
    a.average_assists,
    a.average_saves,
    a.average_touches,
    a.average_car_touches,
    a.average_demos,

    a.average_time_boosting,
    a.average_time_demolished,
    a.average_time_on_ground,
    a.average_time_on_wall,
    a.average_time_powersliding,
    a.average_time_supersonic

from aggregated a
join global_players gp
    on gp.id = a.global_player_id
