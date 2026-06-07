-- with selected_matches(match_guid) as (
--     values
--         
        
-- ),

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
    where p.global_player_id = ?1
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

        avg(tp.score) as average_score,
        avg(tp.goals) as average_goals,
        avg(tp.shots) as average_shots,
        avg(tp.assists) as average_assists,
        avg(tp.saves) as average_saves,
        avg(tp.demos) as average_demos,

        avg(1.0 * ps.time_boosting / tp.duration) as average_percent_boosting,
        avg(1.0 * ps.time_demolished / tp.duration) as average_percent_demolished,
        avg(1.0 * ps.time_on_ground / tp.duration) as average_percent_on_ground,
        avg(1.0 * ps.time_on_wall / tp.duration) as average_percent_on_wall,
        avg(1.0 * ps.time_powersliding / tp.duration) as average_percent_powersliding,
        avg(1.0 * ps.time_supersonic / tp.duration) as average_percent_supersonic

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

    a.average_score,
    a.average_goals,
    a.average_shots,
    a.average_assists,
    a.average_saves,
    a.average_demos,

    a.average_percent_boosting,
    a.average_percent_demolished,
    a.average_percent_on_ground,
    a.average_percent_on_wall,
    a.average_percent_powersliding,
    a.average_percent_supersonic

from aggregated a
join global_players gp
    on gp.id = a.global_player_id
