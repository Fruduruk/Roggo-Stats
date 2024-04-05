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
            return "together"
        case bc.GroupType.INDIVIDUALLY:
            return "individually"


def match_type_to_string(match_type: bc.MatchType) -> str:
    match match_type:
        case bc.MatchType.BOTH:
            return "both"
        case bc.MatchType.RANKED:
            return "ranked"
        case bc.MatchType.UNRANKED:
            return "unranked"


def time_range_to_string(time_range: bc.TimeRange) -> str:
    match time_range:
        case bc.TimeRange.TODAY:
            return "today"
        case bc.TimeRange.YESTERDAY:
            return "yesterday"
        case bc.TimeRange.WEEK:
            return "last 7 days"
        case bc.TimeRange.MONTH:
            return "last 30 days"
        case bc.TimeRange.YEAR:
            return "last 365 days"


def playlist_to_string(playlist: bc.Playlist) -> str:
    match playlist:
        case bc.Playlist.ALL:
            return "all"
        case bc.Playlist.DUELS:
            return "duels"
        case bc.Playlist.DOUBLES:
            return "doubles"
        case bc.Playlist.STANDARD:
            return "standard"
        case bc.Playlist.CHAOS:
            return "chaos"
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
