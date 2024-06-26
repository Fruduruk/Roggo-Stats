from models.result import Result


class TrendResult(Result):
    image_path: str = None
    stat_name: str = None

    def __init__(self, result: Result):
        self.names = result.names
        self.time_range = result.time_range
        self.replay_count = result.replay_count
        self.match_type = result.match_type
        self.playlist = result.playlist
        self.group_type = result.group_type
