from enum import Enum


class Statistic(Enum):
    BOOST_USED_PER_MINUTE = 0
    BOOST_COLLECTED_PER_MINUTE = 1
    BOOST_AMOUNT_STOLEN = 2
    BOOST_AMOUNT_USED_WHILE_SUPERSONIC = 3
    BOOST_COLLECTED_SMALL_TO_BIG_RATIO = 4
    PERCENT_SLOW_SPEED = 5
    PERCENT_SUPERSONIC_SPEED = 6
    AVERAGE_SPEED = 7
    PERCENT_HIGH_AIR = 8
    TIME_POWERSLIDE = 9
    COUNT_POWERSLIDE = 10
    AVERAGE_DISTANCE_TO_BALL = 11
    AVERAGE_DISTANCE_TO_MATES = 12
    PERCENT_CLOSEST_TO_BALL = 13
    PERCENT_FARTHEST_FROM_BALL = 14
    PERCENT_MOST_BACK = 15
    PERCENT_MOST_FORWARD = 16
    GOALS_AGAINST_WHILE_LAST_DEFENDER = 17
    DEMOS_INFLICTED = 18
    DEMOS_TAKEN = 19

    def __str__(self) -> str:
        match self:
            case Statistic.BOOST_USED_PER_MINUTE:
                return "boost used per minute"
            case Statistic.BOOST_COLLECTED_PER_MINUTE:
                return "boost collected per minute"
            case Statistic.BOOST_AMOUNT_STOLEN:
                return "boost amount stolen"
            case Statistic.BOOST_AMOUNT_USED_WHILE_SUPERSONIC:
                return "boost amount used while supersonic"
            case Statistic.BOOST_COLLECTED_SMALL_TO_BIG_RATIO:
                return "boost count collected small to big ratio"
            case Statistic.PERCENT_SLOW_SPEED:
                return "percent slow speed"
            case Statistic.PERCENT_SUPERSONIC_SPEED:
                return "percent supersonic speed"
            case Statistic.AVERAGE_SPEED:
                return "average speed"
            case Statistic.PERCENT_HIGH_AIR:
                return "percent high air"
            case Statistic.TIME_POWERSLIDE:
                return "time powerslide"
            case Statistic.COUNT_POWERSLIDE:
                return "count powerslide"
            case Statistic.AVERAGE_DISTANCE_TO_BALL:
                return "average distance to ball"
            case Statistic.AVERAGE_DISTANCE_TO_MATES:
                return "average distance to mates"
            case Statistic.PERCENT_CLOSEST_TO_BALL:
                return "percent closest to ball"
            case Statistic.PERCENT_FARTHEST_FROM_BALL:
                return "percent farthest from ball"
            case Statistic.PERCENT_MOST_BACK:
                return "percent most back"
            case Statistic.PERCENT_MOST_FORWARD:
                return "percent most forward"
            case Statistic.GOALS_AGAINST_WHILE_LAST_DEFENDER:
                return "goals against while last defender"
            case Statistic.DEMOS_INFLICTED:
                return "demos inflicted"
            case Statistic.DEMOS_TAKEN:
                return "demos taken"
            case None:
                return "unknown"
