import ballchasing_pb2 as bc


class Result:
    names: list
    time_range: bc.TimeRange
    replay_count: int
    match_type: bc.MatchType
    playlist: bc.Playlist
