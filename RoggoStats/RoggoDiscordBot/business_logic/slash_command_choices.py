import ballchasing_pb2 as bc


from interactions import (
    SlashCommandChoice,
)
from business_logic.utils import group_type_to_string, match_type_to_string, playlist_to_string, time_range_to_string
from models.statistic import Statistic


TIME_CHOICES = [
    SlashCommandChoice(name=time_range_to_string(bc.TimeRange.TODAY), value=1),
    SlashCommandChoice(name=time_range_to_string(bc.TimeRange.YESTERDAY), value=2),
    SlashCommandChoice(name=time_range_to_string(bc.TimeRange.WEEK), value=3),
    SlashCommandChoice(name=time_range_to_string(bc.TimeRange.MONTH), value=4),
    SlashCommandChoice(name=time_range_to_string(bc.TimeRange.YEAR), value=5),
]

PLAYLIST_CHOICES = [
    SlashCommandChoice(name=playlist_to_string(bc.Playlist.DUELS), value=1),
    SlashCommandChoice(name=playlist_to_string(bc.Playlist.DOUBLES), value=2),
    SlashCommandChoice(name=playlist_to_string(bc.Playlist.STANDARD), value=3),
    SlashCommandChoice(name=playlist_to_string(bc.Playlist.CHAOS), value=4),
    SlashCommandChoice(name=playlist_to_string(bc.Playlist.PRIVATE_GAME), value=5),
    SlashCommandChoice(name=playlist_to_string(bc.Playlist.OFFLINE), value=6),
    SlashCommandChoice(name=playlist_to_string(bc.Playlist.SNOW_DAY), value=7),
    SlashCommandChoice(name=playlist_to_string(bc.Playlist.ROCKET_LABS), value=8),
    SlashCommandChoice(name=playlist_to_string(bc.Playlist.HOOPS), value=9),
    SlashCommandChoice(name=playlist_to_string(bc.Playlist.RUMBLE), value=10),
    SlashCommandChoice(name=playlist_to_string(bc.Playlist.TOURNAMENT), value=11),
    SlashCommandChoice(name=playlist_to_string(bc.Playlist.DROP_SHOT), value=12),
    SlashCommandChoice(name=playlist_to_string(bc.Playlist.DROP_SHOT_RUMBLE), value=13),
    SlashCommandChoice(name=playlist_to_string(bc.Playlist.HEAT_SEEKER), value=14),
]

STATISTIC_CHOICES = [
    SlashCommandChoice(name=str(Statistic.BOOST_USED_PER_MINUTE), value=0),
    SlashCommandChoice(name=str(Statistic.BOOST_COLLECTED_PER_MINUTE), value=1),
    SlashCommandChoice(name=str(Statistic.BOOST_AMOUNT_STOLEN), value=2),
    SlashCommandChoice(name=str(Statistic.BOOST_AMOUNT_USED_WHILE_SUPERSONIC), value=3),
    SlashCommandChoice(name=str(Statistic.BOOST_COLLECTED_SMALL_TO_BIG_RATIO), value=4),
    SlashCommandChoice(name=str(Statistic.PERCENT_SLOW_SPEED), value=5),
    SlashCommandChoice(name=str(Statistic.PERCENT_SUPERSONIC_SPEED), value=6),
    SlashCommandChoice(name=str(Statistic.AVERAGE_SPEED), value=7),
    SlashCommandChoice(name=str(Statistic.PERCENT_HIGH_AIR), value=8),
    SlashCommandChoice(name=str(Statistic.TIME_POWERSLIDE), value=9),
    SlashCommandChoice(name=str(Statistic.COUNT_POWERSLIDE), value=10),
    SlashCommandChoice(name=str(Statistic.AVERAGE_DISTANCE_TO_BALL), value=11),
    SlashCommandChoice(name=str(Statistic.AVERAGE_DISTANCE_TO_MATES), value=12),
    SlashCommandChoice(name=str(Statistic.PERCENT_CLOSEST_TO_BALL), value=13),
    SlashCommandChoice(name=str(Statistic.PERCENT_FARTHEST_FROM_BALL), value=14),
    SlashCommandChoice(name=str(Statistic.PERCENT_MOST_BACK), value=15),
    SlashCommandChoice(name=str(Statistic.PERCENT_MOST_FORWARD), value=16),
    SlashCommandChoice(name=str(Statistic.GOALS_AGAINST_WHILE_LAST_DEFENDER), value=17),
    SlashCommandChoice(name=str(Statistic.DEMOS_INFLICTED), value=18),
    SlashCommandChoice(name=str(Statistic.DEMOS_TAKEN), value=19),
]

MATCH_TYPE_CHOICES = [
    SlashCommandChoice(name=match_type_to_string(bc.MatchType.RANKED), value=1),
    SlashCommandChoice(name=match_type_to_string(bc.MatchType.UNRANKED), value=2),
]

GROUP_TYPE_CHOICES = [
    SlashCommandChoice(name=group_type_to_string(bc.GroupType.TOGETHER), value=0),
    SlashCommandChoice(name=group_type_to_string(bc.GroupType.INDIVIDUALLY), value=1),
]
