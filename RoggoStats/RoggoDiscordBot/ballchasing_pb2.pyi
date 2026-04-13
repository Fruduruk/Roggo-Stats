from google.protobuf import empty_pb2 as _empty_pb2
from google.protobuf.internal import containers as _containers
from google.protobuf.internal import enum_type_wrapper as _enum_type_wrapper
from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from collections.abc import Iterable as _Iterable, Mapping as _Mapping
from typing import ClassVar as _ClassVar, Optional as _Optional, Union as _Union

DESCRIPTOR: _descriptor.FileDescriptor

class Visibility(int, metaclass=_enum_type_wrapper.EnumTypeWrapper):
    __slots__ = ()
    Unknown: _ClassVar[Visibility]
    Public: _ClassVar[Visibility]
    Private: _ClassVar[Visibility]
    Unlisted: _ClassVar[Visibility]

class TimeRange(int, metaclass=_enum_type_wrapper.EnumTypeWrapper):
    __slots__ = ()
    EVERY_TIME: _ClassVar[TimeRange]
    TODAY: _ClassVar[TimeRange]
    YESTERDAY: _ClassVar[TimeRange]
    WEEK: _ClassVar[TimeRange]
    MONTH: _ClassVar[TimeRange]
    YEAR: _ClassVar[TimeRange]

class IdentityType(int, metaclass=_enum_type_wrapper.EnumTypeWrapper):
    __slots__ = ()
    NAME: _ClassVar[IdentityType]
    STEAM_ID: _ClassVar[IdentityType]
    EPIC_ID: _ClassVar[IdentityType]
    PS4_GAMER_TAG: _ClassVar[IdentityType]

class GroupType(int, metaclass=_enum_type_wrapper.EnumTypeWrapper):
    __slots__ = ()
    TOGETHER: _ClassVar[GroupType]
    INDIVIDUALLY: _ClassVar[GroupType]

class MatchType(int, metaclass=_enum_type_wrapper.EnumTypeWrapper):
    __slots__ = ()
    BOTH: _ClassVar[MatchType]
    RANKED: _ClassVar[MatchType]
    UNRANKED: _ClassVar[MatchType]

class Playlist(int, metaclass=_enum_type_wrapper.EnumTypeWrapper):
    __slots__ = ()
    ALL: _ClassVar[Playlist]
    DUELS: _ClassVar[Playlist]
    DOUBLES: _ClassVar[Playlist]
    STANDARD: _ClassVar[Playlist]
    CHAOS: _ClassVar[Playlist]
    PRIVATE_GAME: _ClassVar[Playlist]
    OFFLINE: _ClassVar[Playlist]
    SNOW_DAY: _ClassVar[Playlist]
    ROCKET_LABS: _ClassVar[Playlist]
    HOOPS: _ClassVar[Playlist]
    RUMBLE: _ClassVar[Playlist]
    TOURNAMENT: _ClassVar[Playlist]
    DROP_SHOT: _ClassVar[Playlist]
    DROP_SHOT_RUMBLE: _ClassVar[Playlist]
    HEAT_SEEKER: _ClassVar[Playlist]

class RankType(int, metaclass=_enum_type_wrapper.EnumTypeWrapper):
    __slots__ = ()
    UNKNOWN: _ClassVar[RankType]
    NO_RANK: _ClassVar[RankType]
    BRONZE_1: _ClassVar[RankType]
    BRONZE_2: _ClassVar[RankType]
    BRONZE_3: _ClassVar[RankType]
    SILVER_1: _ClassVar[RankType]
    SILVER_2: _ClassVar[RankType]
    SILVER_3: _ClassVar[RankType]
    GOLD_1: _ClassVar[RankType]
    GOLD_2: _ClassVar[RankType]
    GOLD_3: _ClassVar[RankType]
    PLATINUM_1: _ClassVar[RankType]
    PLATINUM_2: _ClassVar[RankType]
    PLATINUM_3: _ClassVar[RankType]
    DIAMOND_1: _ClassVar[RankType]
    DIAMOND_2: _ClassVar[RankType]
    DIAMOND_3: _ClassVar[RankType]
    CHAMPION_1: _ClassVar[RankType]
    CHAMPION_2: _ClassVar[RankType]
    CHAMPION_3: _ClassVar[RankType]
    GRAND_CHAMPION_1: _ClassVar[RankType]
    GRAND_CHAMPION_2: _ClassVar[RankType]
    GRAND_CHAMPION_3: _ClassVar[RankType]
    SUPERSONIC_LEGEND: _ClassVar[RankType]
    OLD_GRAND_CHAMPION: _ClassVar[RankType]
Unknown: Visibility
Public: Visibility
Private: Visibility
Unlisted: Visibility
EVERY_TIME: TimeRange
TODAY: TimeRange
YESTERDAY: TimeRange
WEEK: TimeRange
MONTH: TimeRange
YEAR: TimeRange
NAME: IdentityType
STEAM_ID: IdentityType
EPIC_ID: IdentityType
PS4_GAMER_TAG: IdentityType
TOGETHER: GroupType
INDIVIDUALLY: GroupType
BOTH: MatchType
RANKED: MatchType
UNRANKED: MatchType
ALL: Playlist
DUELS: Playlist
DOUBLES: Playlist
STANDARD: Playlist
CHAOS: Playlist
PRIVATE_GAME: Playlist
OFFLINE: Playlist
SNOW_DAY: Playlist
ROCKET_LABS: Playlist
HOOPS: Playlist
RUMBLE: Playlist
TOURNAMENT: Playlist
DROP_SHOT: Playlist
DROP_SHOT_RUMBLE: Playlist
HEAT_SEEKER: Playlist
UNKNOWN: RankType
NO_RANK: RankType
BRONZE_1: RankType
BRONZE_2: RankType
BRONZE_3: RankType
SILVER_1: RankType
SILVER_2: RankType
SILVER_3: RankType
GOLD_1: RankType
GOLD_2: RankType
GOLD_3: RankType
PLATINUM_1: RankType
PLATINUM_2: RankType
PLATINUM_3: RankType
DIAMOND_1: RankType
DIAMOND_2: RankType
DIAMOND_3: RankType
CHAMPION_1: RankType
CHAMPION_2: RankType
CHAMPION_3: RankType
GRAND_CHAMPION_1: RankType
GRAND_CHAMPION_2: RankType
GRAND_CHAMPION_3: RankType
SUPERSONIC_LEGEND: RankType
OLD_GRAND_CHAMPION: RankType

class BackgroundDownloadResponse(_message.Message):
    __slots__ = ("operations",)
    OPERATIONS_FIELD_NUMBER: _ClassVar[int]
    operations: _containers.RepeatedCompositeFieldContainer[BackgroundDownloadOperation]
    def __init__(self, operations: _Optional[_Iterable[_Union[BackgroundDownloadOperation, _Mapping]]] = ...) -> None: ...

class BackgroundDownloadRequest(_message.Message):
    __slots__ = ("operation",)
    OPERATION_FIELD_NUMBER: _ClassVar[int]
    operation: BackgroundDownloadOperation
    def __init__(self, operation: _Optional[_Union[BackgroundDownloadOperation, _Mapping]] = ...) -> None: ...

class BackgroundDownloadOperation(_message.Message):
    __slots__ = ("identity", "cycleIntervalInHours")
    IDENTITY_FIELD_NUMBER: _ClassVar[int]
    CYCLEINTERVALINHOURS_FIELD_NUMBER: _ClassVar[int]
    identity: Identity
    cycleIntervalInHours: float
    def __init__(self, identity: _Optional[_Union[Identity, _Mapping]] = ..., cycleIntervalInHours: _Optional[float] = ...) -> None: ...

class BasicResponse(_message.Message):
    __slots__ = ("success", "error")
    SUCCESS_FIELD_NUMBER: _ClassVar[int]
    ERROR_FIELD_NUMBER: _ClassVar[int]
    success: bool
    error: str
    def __init__(self, success: bool = ..., error: _Optional[str] = ...) -> None: ...

class AdvancedReplaysResponse(_message.Message):
    __slots__ = ("count", "replays")
    COUNT_FIELD_NUMBER: _ClassVar[int]
    REPLAYS_FIELD_NUMBER: _ClassVar[int]
    count: int
    replays: _containers.RepeatedCompositeFieldContainer[AdvancedReplay]
    def __init__(self, count: _Optional[int] = ..., replays: _Optional[_Iterable[_Union[AdvancedReplay, _Mapping]]] = ...) -> None: ...

class AdvancedReplay(_message.Message):
    __slots__ = ("id", "link", "created", "status", "rocketLeagueId", "matchGuid", "title", "mapCode", "serverType", "teamSize", "matchType", "playlist", "duration", "overtime", "overtimeSeconds", "season", "date", "visibility", "minRank", "maxRank", "blue", "orange", "mapName", "server")
    ID_FIELD_NUMBER: _ClassVar[int]
    LINK_FIELD_NUMBER: _ClassVar[int]
    CREATED_FIELD_NUMBER: _ClassVar[int]
    STATUS_FIELD_NUMBER: _ClassVar[int]
    ROCKETLEAGUEID_FIELD_NUMBER: _ClassVar[int]
    MATCHGUID_FIELD_NUMBER: _ClassVar[int]
    TITLE_FIELD_NUMBER: _ClassVar[int]
    MAPCODE_FIELD_NUMBER: _ClassVar[int]
    SERVERTYPE_FIELD_NUMBER: _ClassVar[int]
    TEAMSIZE_FIELD_NUMBER: _ClassVar[int]
    MATCHTYPE_FIELD_NUMBER: _ClassVar[int]
    PLAYLIST_FIELD_NUMBER: _ClassVar[int]
    DURATION_FIELD_NUMBER: _ClassVar[int]
    OVERTIME_FIELD_NUMBER: _ClassVar[int]
    OVERTIMESECONDS_FIELD_NUMBER: _ClassVar[int]
    SEASON_FIELD_NUMBER: _ClassVar[int]
    DATE_FIELD_NUMBER: _ClassVar[int]
    VISIBILITY_FIELD_NUMBER: _ClassVar[int]
    MINRANK_FIELD_NUMBER: _ClassVar[int]
    MAXRANK_FIELD_NUMBER: _ClassVar[int]
    BLUE_FIELD_NUMBER: _ClassVar[int]
    ORANGE_FIELD_NUMBER: _ClassVar[int]
    MAPNAME_FIELD_NUMBER: _ClassVar[int]
    SERVER_FIELD_NUMBER: _ClassVar[int]
    id: str
    link: str
    created: str
    status: str
    rocketLeagueId: str
    matchGuid: str
    title: str
    mapCode: str
    serverType: str
    teamSize: int
    matchType: MatchType
    playlist: Playlist
    duration: int
    overtime: bool
    overtimeSeconds: int
    season: Season
    date: str
    visibility: Visibility
    minRank: Rank
    maxRank: Rank
    blue: AdvancedTeam
    orange: AdvancedTeam
    mapName: str
    server: Server
    def __init__(self, id: _Optional[str] = ..., link: _Optional[str] = ..., created: _Optional[str] = ..., status: _Optional[str] = ..., rocketLeagueId: _Optional[str] = ..., matchGuid: _Optional[str] = ..., title: _Optional[str] = ..., mapCode: _Optional[str] = ..., serverType: _Optional[str] = ..., teamSize: _Optional[int] = ..., matchType: _Optional[_Union[MatchType, str]] = ..., playlist: _Optional[_Union[Playlist, str]] = ..., duration: _Optional[int] = ..., overtime: bool = ..., overtimeSeconds: _Optional[int] = ..., season: _Optional[_Union[Season, _Mapping]] = ..., date: _Optional[str] = ..., visibility: _Optional[_Union[Visibility, str]] = ..., minRank: _Optional[_Union[Rank, _Mapping]] = ..., maxRank: _Optional[_Union[Rank, _Mapping]] = ..., blue: _Optional[_Union[AdvancedTeam, _Mapping]] = ..., orange: _Optional[_Union[AdvancedTeam, _Mapping]] = ..., mapName: _Optional[str] = ..., server: _Optional[_Union[Server, _Mapping]] = ...) -> None: ...

class AdvancedTeam(_message.Message):
    __slots__ = ("color", "stats", "players")
    COLOR_FIELD_NUMBER: _ClassVar[int]
    STATS_FIELD_NUMBER: _ClassVar[int]
    PLAYERS_FIELD_NUMBER: _ClassVar[int]
    color: str
    stats: TeamStats
    players: _containers.RepeatedCompositeFieldContainer[AdvancedPlayer]
    def __init__(self, color: _Optional[str] = ..., stats: _Optional[_Union[TeamStats, _Mapping]] = ..., players: _Optional[_Iterable[_Union[AdvancedPlayer, _Mapping]]] = ...) -> None: ...

class AdvancedPlayer(_message.Message):
    __slots__ = ("startTime", "endTime", "name", "id", "rank", "carId", "carName", "camera", "steeringSensitivity", "stats")
    STARTTIME_FIELD_NUMBER: _ClassVar[int]
    ENDTIME_FIELD_NUMBER: _ClassVar[int]
    NAME_FIELD_NUMBER: _ClassVar[int]
    ID_FIELD_NUMBER: _ClassVar[int]
    RANK_FIELD_NUMBER: _ClassVar[int]
    CARID_FIELD_NUMBER: _ClassVar[int]
    CARNAME_FIELD_NUMBER: _ClassVar[int]
    CAMERA_FIELD_NUMBER: _ClassVar[int]
    STEERINGSENSITIVITY_FIELD_NUMBER: _ClassVar[int]
    STATS_FIELD_NUMBER: _ClassVar[int]
    startTime: float
    endTime: float
    name: str
    id: PlayerId
    rank: Rank
    carId: int
    carName: str
    camera: Camera
    steeringSensitivity: float
    stats: PlayerStats
    def __init__(self, startTime: _Optional[float] = ..., endTime: _Optional[float] = ..., name: _Optional[str] = ..., id: _Optional[_Union[PlayerId, _Mapping]] = ..., rank: _Optional[_Union[Rank, _Mapping]] = ..., carId: _Optional[int] = ..., carName: _Optional[str] = ..., camera: _Optional[_Union[Camera, _Mapping]] = ..., steeringSensitivity: _Optional[float] = ..., stats: _Optional[_Union[PlayerStats, _Mapping]] = ...) -> None: ...

class PlayerStats(_message.Message):
    __slots__ = ("core", "boost", "movement", "positioning", "demo")
    CORE_FIELD_NUMBER: _ClassVar[int]
    BOOST_FIELD_NUMBER: _ClassVar[int]
    MOVEMENT_FIELD_NUMBER: _ClassVar[int]
    POSITIONING_FIELD_NUMBER: _ClassVar[int]
    DEMO_FIELD_NUMBER: _ClassVar[int]
    core: PlayerCore
    boost: PlayerBoost
    movement: PlayerMovement
    positioning: PlayerPositioning
    demo: Demo
    def __init__(self, core: _Optional[_Union[PlayerCore, _Mapping]] = ..., boost: _Optional[_Union[PlayerBoost, _Mapping]] = ..., movement: _Optional[_Union[PlayerMovement, _Mapping]] = ..., positioning: _Optional[_Union[PlayerPositioning, _Mapping]] = ..., demo: _Optional[_Union[Demo, _Mapping]] = ...) -> None: ...

class PlayerCore(_message.Message):
    __slots__ = ("generalCore", "mvp")
    GENERALCORE_FIELD_NUMBER: _ClassVar[int]
    MVP_FIELD_NUMBER: _ClassVar[int]
    generalCore: GeneralCore
    mvp: bool
    def __init__(self, generalCore: _Optional[_Union[GeneralCore, _Mapping]] = ..., mvp: bool = ...) -> None: ...

class PlayerBoost(_message.Message):
    __slots__ = ("generalBoost", "percentZeroBoost", "percentFullBoost", "percentBoost0To25", "percentBoost25To50", "percentBoost50To75", "percentBoost75To100")
    GENERALBOOST_FIELD_NUMBER: _ClassVar[int]
    PERCENTZEROBOOST_FIELD_NUMBER: _ClassVar[int]
    PERCENTFULLBOOST_FIELD_NUMBER: _ClassVar[int]
    PERCENTBOOST0TO25_FIELD_NUMBER: _ClassVar[int]
    PERCENTBOOST25TO50_FIELD_NUMBER: _ClassVar[int]
    PERCENTBOOST50TO75_FIELD_NUMBER: _ClassVar[int]
    PERCENTBOOST75TO100_FIELD_NUMBER: _ClassVar[int]
    generalBoost: GeneralBoost
    percentZeroBoost: float
    percentFullBoost: float
    percentBoost0To25: float
    percentBoost25To50: float
    percentBoost50To75: float
    percentBoost75To100: float
    def __init__(self, generalBoost: _Optional[_Union[GeneralBoost, _Mapping]] = ..., percentZeroBoost: _Optional[float] = ..., percentFullBoost: _Optional[float] = ..., percentBoost0To25: _Optional[float] = ..., percentBoost25To50: _Optional[float] = ..., percentBoost50To75: _Optional[float] = ..., percentBoost75To100: _Optional[float] = ...) -> None: ...

class PlayerMovement(_message.Message):
    __slots__ = ("generalMovement", "avgSpeed", "avgPowerslideDuration", "avgSpeedPercentage", "percentSlowSpeed", "percentBoostSpeed", "percentSupersonicSpeed", "percentGround", "percentLowAir", "percentHighAir")
    GENERALMOVEMENT_FIELD_NUMBER: _ClassVar[int]
    AVGSPEED_FIELD_NUMBER: _ClassVar[int]
    AVGPOWERSLIDEDURATION_FIELD_NUMBER: _ClassVar[int]
    AVGSPEEDPERCENTAGE_FIELD_NUMBER: _ClassVar[int]
    PERCENTSLOWSPEED_FIELD_NUMBER: _ClassVar[int]
    PERCENTBOOSTSPEED_FIELD_NUMBER: _ClassVar[int]
    PERCENTSUPERSONICSPEED_FIELD_NUMBER: _ClassVar[int]
    PERCENTGROUND_FIELD_NUMBER: _ClassVar[int]
    PERCENTLOWAIR_FIELD_NUMBER: _ClassVar[int]
    PERCENTHIGHAIR_FIELD_NUMBER: _ClassVar[int]
    generalMovement: GeneralMovement
    avgSpeed: int
    avgPowerslideDuration: float
    avgSpeedPercentage: float
    percentSlowSpeed: float
    percentBoostSpeed: float
    percentSupersonicSpeed: float
    percentGround: float
    percentLowAir: float
    percentHighAir: float
    def __init__(self, generalMovement: _Optional[_Union[GeneralMovement, _Mapping]] = ..., avgSpeed: _Optional[int] = ..., avgPowerslideDuration: _Optional[float] = ..., avgSpeedPercentage: _Optional[float] = ..., percentSlowSpeed: _Optional[float] = ..., percentBoostSpeed: _Optional[float] = ..., percentSupersonicSpeed: _Optional[float] = ..., percentGround: _Optional[float] = ..., percentLowAir: _Optional[float] = ..., percentHighAir: _Optional[float] = ...) -> None: ...

class PlayerPositioning(_message.Message):
    __slots__ = ("generalPositioning", "avgDistanceToBall", "avgDistanceToBallPossession", "avgDistanceToBallNoPossession", "avgDistanceToMates", "timeMostBack", "timeMostForward", "timeClosestToBall", "timeFarthestFromBall", "percentDefensiveThird", "percentOffensiveThird", "percentNeutralThird", "percentDefensiveHalf", "percentOffensiveHalf", "percentBehindBall", "percentInfrontBall", "percentMostBack", "percentMostForward", "percentClosestToBall", "percentFarthestFromBall", "goalsAgainstWhileLastDefender")
    GENERALPOSITIONING_FIELD_NUMBER: _ClassVar[int]
    AVGDISTANCETOBALL_FIELD_NUMBER: _ClassVar[int]
    AVGDISTANCETOBALLPOSSESSION_FIELD_NUMBER: _ClassVar[int]
    AVGDISTANCETOBALLNOPOSSESSION_FIELD_NUMBER: _ClassVar[int]
    AVGDISTANCETOMATES_FIELD_NUMBER: _ClassVar[int]
    TIMEMOSTBACK_FIELD_NUMBER: _ClassVar[int]
    TIMEMOSTFORWARD_FIELD_NUMBER: _ClassVar[int]
    TIMECLOSESTTOBALL_FIELD_NUMBER: _ClassVar[int]
    TIMEFARTHESTFROMBALL_FIELD_NUMBER: _ClassVar[int]
    PERCENTDEFENSIVETHIRD_FIELD_NUMBER: _ClassVar[int]
    PERCENTOFFENSIVETHIRD_FIELD_NUMBER: _ClassVar[int]
    PERCENTNEUTRALTHIRD_FIELD_NUMBER: _ClassVar[int]
    PERCENTDEFENSIVEHALF_FIELD_NUMBER: _ClassVar[int]
    PERCENTOFFENSIVEHALF_FIELD_NUMBER: _ClassVar[int]
    PERCENTBEHINDBALL_FIELD_NUMBER: _ClassVar[int]
    PERCENTINFRONTBALL_FIELD_NUMBER: _ClassVar[int]
    PERCENTMOSTBACK_FIELD_NUMBER: _ClassVar[int]
    PERCENTMOSTFORWARD_FIELD_NUMBER: _ClassVar[int]
    PERCENTCLOSESTTOBALL_FIELD_NUMBER: _ClassVar[int]
    PERCENTFARTHESTFROMBALL_FIELD_NUMBER: _ClassVar[int]
    GOALSAGAINSTWHILELASTDEFENDER_FIELD_NUMBER: _ClassVar[int]
    generalPositioning: GeneralPositioning
    avgDistanceToBall: int
    avgDistanceToBallPossession: int
    avgDistanceToBallNoPossession: int
    avgDistanceToMates: int
    timeMostBack: float
    timeMostForward: float
    timeClosestToBall: float
    timeFarthestFromBall: float
    percentDefensiveThird: float
    percentOffensiveThird: float
    percentNeutralThird: float
    percentDefensiveHalf: float
    percentOffensiveHalf: float
    percentBehindBall: float
    percentInfrontBall: float
    percentMostBack: float
    percentMostForward: float
    percentClosestToBall: float
    percentFarthestFromBall: float
    goalsAgainstWhileLastDefender: int
    def __init__(self, generalPositioning: _Optional[_Union[GeneralPositioning, _Mapping]] = ..., avgDistanceToBall: _Optional[int] = ..., avgDistanceToBallPossession: _Optional[int] = ..., avgDistanceToBallNoPossession: _Optional[int] = ..., avgDistanceToMates: _Optional[int] = ..., timeMostBack: _Optional[float] = ..., timeMostForward: _Optional[float] = ..., timeClosestToBall: _Optional[float] = ..., timeFarthestFromBall: _Optional[float] = ..., percentDefensiveThird: _Optional[float] = ..., percentOffensiveThird: _Optional[float] = ..., percentNeutralThird: _Optional[float] = ..., percentDefensiveHalf: _Optional[float] = ..., percentOffensiveHalf: _Optional[float] = ..., percentBehindBall: _Optional[float] = ..., percentInfrontBall: _Optional[float] = ..., percentMostBack: _Optional[float] = ..., percentMostForward: _Optional[float] = ..., percentClosestToBall: _Optional[float] = ..., percentFarthestFromBall: _Optional[float] = ..., goalsAgainstWhileLastDefender: _Optional[int] = ...) -> None: ...

class Camera(_message.Message):
    __slots__ = ("fov", "height", "pitch", "distance", "stiffness", "swivelSpeed", "transitionSpeed")
    FOV_FIELD_NUMBER: _ClassVar[int]
    HEIGHT_FIELD_NUMBER: _ClassVar[int]
    PITCH_FIELD_NUMBER: _ClassVar[int]
    DISTANCE_FIELD_NUMBER: _ClassVar[int]
    STIFFNESS_FIELD_NUMBER: _ClassVar[int]
    SWIVELSPEED_FIELD_NUMBER: _ClassVar[int]
    TRANSITIONSPEED_FIELD_NUMBER: _ClassVar[int]
    fov: int
    height: int
    pitch: int
    distance: int
    stiffness: float
    swivelSpeed: float
    transitionSpeed: float
    def __init__(self, fov: _Optional[int] = ..., height: _Optional[int] = ..., pitch: _Optional[int] = ..., distance: _Optional[int] = ..., stiffness: _Optional[float] = ..., swivelSpeed: _Optional[float] = ..., transitionSpeed: _Optional[float] = ...) -> None: ...

class TeamStats(_message.Message):
    __slots__ = ("ball", "core", "boost", "movement", "positioning", "demo")
    BALL_FIELD_NUMBER: _ClassVar[int]
    CORE_FIELD_NUMBER: _ClassVar[int]
    BOOST_FIELD_NUMBER: _ClassVar[int]
    MOVEMENT_FIELD_NUMBER: _ClassVar[int]
    POSITIONING_FIELD_NUMBER: _ClassVar[int]
    DEMO_FIELD_NUMBER: _ClassVar[int]
    ball: Ball
    core: GeneralCore
    boost: GeneralBoost
    movement: GeneralMovement
    positioning: GeneralPositioning
    demo: Demo
    def __init__(self, ball: _Optional[_Union[Ball, _Mapping]] = ..., core: _Optional[_Union[GeneralCore, _Mapping]] = ..., boost: _Optional[_Union[GeneralBoost, _Mapping]] = ..., movement: _Optional[_Union[GeneralMovement, _Mapping]] = ..., positioning: _Optional[_Union[GeneralPositioning, _Mapping]] = ..., demo: _Optional[_Union[Demo, _Mapping]] = ...) -> None: ...

class Demo(_message.Message):
    __slots__ = ("inflicted", "taken")
    INFLICTED_FIELD_NUMBER: _ClassVar[int]
    TAKEN_FIELD_NUMBER: _ClassVar[int]
    inflicted: int
    taken: int
    def __init__(self, inflicted: _Optional[int] = ..., taken: _Optional[int] = ...) -> None: ...

class GeneralPositioning(_message.Message):
    __slots__ = ("timeDefensiveThird", "timeNeutralThird", "timeOffensiveThird", "timeDefensiveHalf", "timeOffensiveHalf", "timeBehindBall", "timeInfrontBall")
    TIMEDEFENSIVETHIRD_FIELD_NUMBER: _ClassVar[int]
    TIMENEUTRALTHIRD_FIELD_NUMBER: _ClassVar[int]
    TIMEOFFENSIVETHIRD_FIELD_NUMBER: _ClassVar[int]
    TIMEDEFENSIVEHALF_FIELD_NUMBER: _ClassVar[int]
    TIMEOFFENSIVEHALF_FIELD_NUMBER: _ClassVar[int]
    TIMEBEHINDBALL_FIELD_NUMBER: _ClassVar[int]
    TIMEINFRONTBALL_FIELD_NUMBER: _ClassVar[int]
    timeDefensiveThird: float
    timeNeutralThird: float
    timeOffensiveThird: float
    timeDefensiveHalf: float
    timeOffensiveHalf: float
    timeBehindBall: float
    timeInfrontBall: float
    def __init__(self, timeDefensiveThird: _Optional[float] = ..., timeNeutralThird: _Optional[float] = ..., timeOffensiveThird: _Optional[float] = ..., timeDefensiveHalf: _Optional[float] = ..., timeOffensiveHalf: _Optional[float] = ..., timeBehindBall: _Optional[float] = ..., timeInfrontBall: _Optional[float] = ...) -> None: ...

class GeneralMovement(_message.Message):
    __slots__ = ("totalDistance", "timeSupersonicSpeed", "timeBoostSpeed", "timeSlowSpeed", "timeGround", "timeLowAir", "timeHighAir", "timePowerslide", "countPowerslide")
    TOTALDISTANCE_FIELD_NUMBER: _ClassVar[int]
    TIMESUPERSONICSPEED_FIELD_NUMBER: _ClassVar[int]
    TIMEBOOSTSPEED_FIELD_NUMBER: _ClassVar[int]
    TIMESLOWSPEED_FIELD_NUMBER: _ClassVar[int]
    TIMEGROUND_FIELD_NUMBER: _ClassVar[int]
    TIMELOWAIR_FIELD_NUMBER: _ClassVar[int]
    TIMEHIGHAIR_FIELD_NUMBER: _ClassVar[int]
    TIMEPOWERSLIDE_FIELD_NUMBER: _ClassVar[int]
    COUNTPOWERSLIDE_FIELD_NUMBER: _ClassVar[int]
    totalDistance: int
    timeSupersonicSpeed: float
    timeBoostSpeed: float
    timeSlowSpeed: float
    timeGround: float
    timeLowAir: float
    timeHighAir: float
    timePowerslide: float
    countPowerslide: int
    def __init__(self, totalDistance: _Optional[int] = ..., timeSupersonicSpeed: _Optional[float] = ..., timeBoostSpeed: _Optional[float] = ..., timeSlowSpeed: _Optional[float] = ..., timeGround: _Optional[float] = ..., timeLowAir: _Optional[float] = ..., timeHighAir: _Optional[float] = ..., timePowerslide: _Optional[float] = ..., countPowerslide: _Optional[int] = ...) -> None: ...

class GeneralBoost(_message.Message):
    __slots__ = ("bpm", "bcpm", "avgAmount", "amountCollected", "amountStolen", "amountCollectedBig", "amountStolenBig", "amountCollectedSmall", "amountStolenSmall", "countCollectedBig", "countStolenBig", "countCollectedSmall", "countStolenSmall", "amountOverfill", "amountOverfillStolen", "amountUsedWhileSupersonic", "timeZeroBoost", "timeFullBoost", "timeBoost0To25", "timeBoost25To50", "timeBoost50To75", "timeBoost75To100")
    BPM_FIELD_NUMBER: _ClassVar[int]
    BCPM_FIELD_NUMBER: _ClassVar[int]
    AVGAMOUNT_FIELD_NUMBER: _ClassVar[int]
    AMOUNTCOLLECTED_FIELD_NUMBER: _ClassVar[int]
    AMOUNTSTOLEN_FIELD_NUMBER: _ClassVar[int]
    AMOUNTCOLLECTEDBIG_FIELD_NUMBER: _ClassVar[int]
    AMOUNTSTOLENBIG_FIELD_NUMBER: _ClassVar[int]
    AMOUNTCOLLECTEDSMALL_FIELD_NUMBER: _ClassVar[int]
    AMOUNTSTOLENSMALL_FIELD_NUMBER: _ClassVar[int]
    COUNTCOLLECTEDBIG_FIELD_NUMBER: _ClassVar[int]
    COUNTSTOLENBIG_FIELD_NUMBER: _ClassVar[int]
    COUNTCOLLECTEDSMALL_FIELD_NUMBER: _ClassVar[int]
    COUNTSTOLENSMALL_FIELD_NUMBER: _ClassVar[int]
    AMOUNTOVERFILL_FIELD_NUMBER: _ClassVar[int]
    AMOUNTOVERFILLSTOLEN_FIELD_NUMBER: _ClassVar[int]
    AMOUNTUSEDWHILESUPERSONIC_FIELD_NUMBER: _ClassVar[int]
    TIMEZEROBOOST_FIELD_NUMBER: _ClassVar[int]
    TIMEFULLBOOST_FIELD_NUMBER: _ClassVar[int]
    TIMEBOOST0TO25_FIELD_NUMBER: _ClassVar[int]
    TIMEBOOST25TO50_FIELD_NUMBER: _ClassVar[int]
    TIMEBOOST50TO75_FIELD_NUMBER: _ClassVar[int]
    TIMEBOOST75TO100_FIELD_NUMBER: _ClassVar[int]
    bpm: int
    bcpm: float
    avgAmount: float
    amountCollected: int
    amountStolen: int
    amountCollectedBig: int
    amountStolenBig: int
    amountCollectedSmall: int
    amountStolenSmall: int
    countCollectedBig: int
    countStolenBig: int
    countCollectedSmall: int
    countStolenSmall: int
    amountOverfill: int
    amountOverfillStolen: int
    amountUsedWhileSupersonic: int
    timeZeroBoost: float
    timeFullBoost: float
    timeBoost0To25: float
    timeBoost25To50: float
    timeBoost50To75: float
    timeBoost75To100: float
    def __init__(self, bpm: _Optional[int] = ..., bcpm: _Optional[float] = ..., avgAmount: _Optional[float] = ..., amountCollected: _Optional[int] = ..., amountStolen: _Optional[int] = ..., amountCollectedBig: _Optional[int] = ..., amountStolenBig: _Optional[int] = ..., amountCollectedSmall: _Optional[int] = ..., amountStolenSmall: _Optional[int] = ..., countCollectedBig: _Optional[int] = ..., countStolenBig: _Optional[int] = ..., countCollectedSmall: _Optional[int] = ..., countStolenSmall: _Optional[int] = ..., amountOverfill: _Optional[int] = ..., amountOverfillStolen: _Optional[int] = ..., amountUsedWhileSupersonic: _Optional[int] = ..., timeZeroBoost: _Optional[float] = ..., timeFullBoost: _Optional[float] = ..., timeBoost0To25: _Optional[float] = ..., timeBoost25To50: _Optional[float] = ..., timeBoost50To75: _Optional[float] = ..., timeBoost75To100: _Optional[float] = ...) -> None: ...

class GeneralCore(_message.Message):
    __slots__ = ("shots", "shotsAgainst", "goals", "goalsAgainst", "saves", "assists", "score", "shootingPercentage")
    SHOTS_FIELD_NUMBER: _ClassVar[int]
    SHOTSAGAINST_FIELD_NUMBER: _ClassVar[int]
    GOALS_FIELD_NUMBER: _ClassVar[int]
    GOALSAGAINST_FIELD_NUMBER: _ClassVar[int]
    SAVES_FIELD_NUMBER: _ClassVar[int]
    ASSISTS_FIELD_NUMBER: _ClassVar[int]
    SCORE_FIELD_NUMBER: _ClassVar[int]
    SHOOTINGPERCENTAGE_FIELD_NUMBER: _ClassVar[int]
    shots: int
    shotsAgainst: int
    goals: int
    goalsAgainst: int
    saves: int
    assists: int
    score: int
    shootingPercentage: float
    def __init__(self, shots: _Optional[int] = ..., shotsAgainst: _Optional[int] = ..., goals: _Optional[int] = ..., goalsAgainst: _Optional[int] = ..., saves: _Optional[int] = ..., assists: _Optional[int] = ..., score: _Optional[int] = ..., shootingPercentage: _Optional[float] = ...) -> None: ...

class Ball(_message.Message):
    __slots__ = ("possessionTime", "timeInSide")
    POSSESSIONTIME_FIELD_NUMBER: _ClassVar[int]
    TIMEINSIDE_FIELD_NUMBER: _ClassVar[int]
    possessionTime: float
    timeInSide: float
    def __init__(self, possessionTime: _Optional[float] = ..., timeInSide: _Optional[float] = ...) -> None: ...

class Server(_message.Message):
    __slots__ = ("name", "region")
    NAME_FIELD_NUMBER: _ClassVar[int]
    REGION_FIELD_NUMBER: _ClassVar[int]
    name: str
    region: str
    def __init__(self, name: _Optional[str] = ..., region: _Optional[str] = ...) -> None: ...

class IdRequest(_message.Message):
    __slots__ = ("id",)
    ID_FIELD_NUMBER: _ClassVar[int]
    id: str
    def __init__(self, id: _Optional[str] = ...) -> None: ...

class FilterRequest(_message.Message):
    __slots__ = ("replayCap", "identities", "groupType", "playlist", "matchType", "title", "season", "minDate", "maxDate", "minRank", "maxRank", "timeRange")
    REPLAYCAP_FIELD_NUMBER: _ClassVar[int]
    IDENTITIES_FIELD_NUMBER: _ClassVar[int]
    GROUPTYPE_FIELD_NUMBER: _ClassVar[int]
    PLAYLIST_FIELD_NUMBER: _ClassVar[int]
    MATCHTYPE_FIELD_NUMBER: _ClassVar[int]
    TITLE_FIELD_NUMBER: _ClassVar[int]
    SEASON_FIELD_NUMBER: _ClassVar[int]
    MINDATE_FIELD_NUMBER: _ClassVar[int]
    MAXDATE_FIELD_NUMBER: _ClassVar[int]
    MINRANK_FIELD_NUMBER: _ClassVar[int]
    MAXRANK_FIELD_NUMBER: _ClassVar[int]
    TIMERANGE_FIELD_NUMBER: _ClassVar[int]
    replayCap: int
    identities: _containers.RepeatedCompositeFieldContainer[Identity]
    groupType: GroupType
    playlist: Playlist
    matchType: MatchType
    title: str
    season: Season
    minDate: str
    maxDate: str
    minRank: RankType
    maxRank: RankType
    timeRange: TimeRange
    def __init__(self, replayCap: _Optional[int] = ..., identities: _Optional[_Iterable[_Union[Identity, _Mapping]]] = ..., groupType: _Optional[_Union[GroupType, str]] = ..., playlist: _Optional[_Union[Playlist, str]] = ..., matchType: _Optional[_Union[MatchType, str]] = ..., title: _Optional[str] = ..., season: _Optional[_Union[Season, _Mapping]] = ..., minDate: _Optional[str] = ..., maxDate: _Optional[str] = ..., minRank: _Optional[_Union[RankType, str]] = ..., maxRank: _Optional[_Union[RankType, str]] = ..., timeRange: _Optional[_Union[TimeRange, str]] = ...) -> None: ...

class Identity(_message.Message):
    __slots__ = ("identityType", "nameOrId")
    IDENTITYTYPE_FIELD_NUMBER: _ClassVar[int]
    NAMEORID_FIELD_NUMBER: _ClassVar[int]
    identityType: IdentityType
    nameOrId: str
    def __init__(self, identityType: _Optional[_Union[IdentityType, str]] = ..., nameOrId: _Optional[str] = ...) -> None: ...

class Season(_message.Message):
    __slots__ = ("number", "freeToPlay")
    NUMBER_FIELD_NUMBER: _ClassVar[int]
    FREETOPLAY_FIELD_NUMBER: _ClassVar[int]
    number: int
    freeToPlay: bool
    def __init__(self, number: _Optional[int] = ..., freeToPlay: bool = ...) -> None: ...

class SimpleReplaysResponse(_message.Message):
    __slots__ = ("count", "replays")
    COUNT_FIELD_NUMBER: _ClassVar[int]
    REPLAYS_FIELD_NUMBER: _ClassVar[int]
    count: int
    replays: _containers.RepeatedCompositeFieldContainer[Replay]
    def __init__(self, count: _Optional[int] = ..., replays: _Optional[_Iterable[_Union[Replay, _Mapping]]] = ...) -> None: ...

class Replay(_message.Message):
    __slots__ = ("id", "title", "mapName", "playlist", "matchType", "duration", "overtime", "date", "blue", "orange", "minRank", "maxRank")
    ID_FIELD_NUMBER: _ClassVar[int]
    TITLE_FIELD_NUMBER: _ClassVar[int]
    MAPNAME_FIELD_NUMBER: _ClassVar[int]
    PLAYLIST_FIELD_NUMBER: _ClassVar[int]
    MATCHTYPE_FIELD_NUMBER: _ClassVar[int]
    DURATION_FIELD_NUMBER: _ClassVar[int]
    OVERTIME_FIELD_NUMBER: _ClassVar[int]
    DATE_FIELD_NUMBER: _ClassVar[int]
    BLUE_FIELD_NUMBER: _ClassVar[int]
    ORANGE_FIELD_NUMBER: _ClassVar[int]
    MINRANK_FIELD_NUMBER: _ClassVar[int]
    MAXRANK_FIELD_NUMBER: _ClassVar[int]
    id: str
    title: str
    mapName: str
    playlist: Playlist
    matchType: MatchType
    duration: int
    overtime: bool
    date: str
    blue: Team
    orange: Team
    minRank: Rank
    maxRank: Rank
    def __init__(self, id: _Optional[str] = ..., title: _Optional[str] = ..., mapName: _Optional[str] = ..., playlist: _Optional[_Union[Playlist, str]] = ..., matchType: _Optional[_Union[MatchType, str]] = ..., duration: _Optional[int] = ..., overtime: bool = ..., date: _Optional[str] = ..., blue: _Optional[_Union[Team, _Mapping]] = ..., orange: _Optional[_Union[Team, _Mapping]] = ..., minRank: _Optional[_Union[Rank, _Mapping]] = ..., maxRank: _Optional[_Union[Rank, _Mapping]] = ...) -> None: ...

class Rank(_message.Message):
    __slots__ = ("id", "tier", "division", "name")
    ID_FIELD_NUMBER: _ClassVar[int]
    TIER_FIELD_NUMBER: _ClassVar[int]
    DIVISION_FIELD_NUMBER: _ClassVar[int]
    NAME_FIELD_NUMBER: _ClassVar[int]
    id: RankType
    tier: int
    division: int
    name: str
    def __init__(self, id: _Optional[_Union[RankType, str]] = ..., tier: _Optional[int] = ..., division: _Optional[int] = ..., name: _Optional[str] = ...) -> None: ...

class Team(_message.Message):
    __slots__ = ("goals", "players")
    GOALS_FIELD_NUMBER: _ClassVar[int]
    PLAYERS_FIELD_NUMBER: _ClassVar[int]
    goals: int
    players: _containers.RepeatedCompositeFieldContainer[Player]
    def __init__(self, goals: _Optional[int] = ..., players: _Optional[_Iterable[_Union[Player, _Mapping]]] = ...) -> None: ...

class Player(_message.Message):
    __slots__ = ("startTime", "endTime", "name", "id", "mvp", "rank", "score")
    STARTTIME_FIELD_NUMBER: _ClassVar[int]
    ENDTIME_FIELD_NUMBER: _ClassVar[int]
    NAME_FIELD_NUMBER: _ClassVar[int]
    ID_FIELD_NUMBER: _ClassVar[int]
    MVP_FIELD_NUMBER: _ClassVar[int]
    RANK_FIELD_NUMBER: _ClassVar[int]
    SCORE_FIELD_NUMBER: _ClassVar[int]
    startTime: float
    endTime: float
    name: str
    id: PlayerId
    mvp: bool
    rank: Rank
    score: int
    def __init__(self, startTime: _Optional[float] = ..., endTime: _Optional[float] = ..., name: _Optional[str] = ..., id: _Optional[_Union[PlayerId, _Mapping]] = ..., mvp: bool = ..., rank: _Optional[_Union[Rank, _Mapping]] = ..., score: _Optional[int] = ...) -> None: ...

class PlayerId(_message.Message):
    __slots__ = ("platform", "id")
    PLATFORM_FIELD_NUMBER: _ClassVar[int]
    ID_FIELD_NUMBER: _ClassVar[int]
    platform: str
    id: str
    def __init__(self, platform: _Optional[str] = ..., id: _Optional[str] = ...) -> None: ...
