from models.result import Result


class WinrateResult(Result):
    winrate: float = 0

    def __init__(self, result: Result):
        self.names = result.names
        self.time_range = result.time_range
        self.replay_count = result.replay_count
        self.match_type = result.match_type
        self.playlist = result.playlist
        self.group_type = result.group_type
