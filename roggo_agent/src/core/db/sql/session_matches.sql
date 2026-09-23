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

match_data as (
    select
        m.id as match_id,
        p.team_id as own_team_id,

        case
            when own_team.score > enemy_team.score then 1
            when own_team.score < enemy_team.score then 0
            else null
        end as main_character_won,

        own_team.score as own_score,
        enemy_team.score as enemy_score

    from selected_matches sm
    join matches m
        on m.match_guid = sm.match_guid
    join players p
        on p.match_id = m.id
       and p.global_player_id = ?1
    
    join teams own_team
        on own_team.id = p.team_id
    join teams enemy_team
        on enemy_team.match_id = m.id
        and enemy_team.id <> own_team.id
),


best_own_players as (
    select
        p.match_id,
        p.global_player_id,
        p.display_name,
        max(p.score) as score
    from match_data md
    join players p
        on p.match_id = md.match_id
       and p.team_id = md.own_team_id
    group by p.match_id
)

select
    m.id,
    m.match_guid,
    m.arena,
    m.duration,
    m.created_at_ms,
    m.ended_at_ms,
    m.had_overtime,
    m.deleted,
    m.playlist,

    md.main_character_won,
    md.own_score,
    md.enemy_score,
    own_best.global_player_id as own_best_global_player_id


from selected_matches sm
join matches m
    on m.match_guid = sm.match_guid
join match_data md
    on md.match_id = m.id
join best_own_players own_best
    on own_best.match_id = m.id