from typing import List

import ballchasing_pb2 as bc
from models.result import Result


def get_basic_result(request: bc.FilterRequest, replay_count: int, names: List[str]) -> Result:
    result = Result()
    result.names = names
    result.time_range = request.timeRange
    result.playlist = request.playlist
    result.replay_count = replay_count
    result.match_type = request.matchType
    result.group_type = request.groupType
    return result


def group_type_to_string(group_type: bc.GroupType) -> str:
    match group_type:
        case bc.GroupType.TOGETHER:
            return "Together"
        case bc.GroupType.INDIVIDUALLY:
            return "Individually"


def match_type_to_string(match_type: bc.MatchType) -> str:
    match match_type:
        case bc.MatchType.BOTH:
            return "Ranked and Unranked"
        case bc.MatchType.RANKED:
            return "Ranked"
        case bc.MatchType.UNRANKED:
            return "Unranked"


def time_range_to_string(time_range: bc.TimeRange) -> str:
    match time_range:
        case bc.TimeRange.TODAY:
            return "Today"
        case bc.TimeRange.YESTERDAY:
            return "Yesterday"
        case bc.TimeRange.WEEK:
            return "Last 7 days"
        case bc.TimeRange.MONTH:
            return "Last 30 days"
        case bc.TimeRange.YEAR:
            return "Last 365 days"


def playlist_to_string(playlist: bc.Playlist) -> str:
    match playlist:
        case bc.Playlist.ALL:
            return "all"
        case bc.Playlist.DUELS:
            return "1v1"
        case bc.Playlist.DOUBLES:
            return "2v2"
        case bc.Playlist.STANDARD:
            return "3v3"
        case bc.Playlist.CHAOS:
            return "4v4"
        case bc.Playlist.PRIVATE_GAME:
            return "private"
        case bc.Playlist.OFFLINE:
            return "offline"
        case bc.Playlist.SNOW_DAY:
            return "snow day"
        case bc.Playlist.ROCKET_LABS:
            return "rocket labs"
        case bc.Playlist.HOOPS:
            return "hoops"
        case bc.Playlist.RUMBLE:
            return "rumble"
        case bc.Playlist.TOURNAMENT:
            return "tournament"
        case bc.Playlist.DROP_SHOT:
            return "drop shot"
        case bc.Playlist.DROP_SHOT_RUMBLE:
            return "drop shot rumble"
        case bc.Playlist.HEAT_SEEKER:
            return "heat seeker"
