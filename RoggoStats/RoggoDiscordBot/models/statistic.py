from enum import Enum


class Statistic(Enum):
    PERCENT_SUPERSONIC_SPEED = 0
    BOOST_COLLECTED_PER_MINUTE = 1

    def __str__(self) -> str:
        match self:
            case Statistic.PERCENT_SUPERSONIC_SPEED:
                return "percent supersonic speed"
            case Statistic.BOOST_COLLECTED_PER_MINUTE:
                return "boost collected per minute"
            case None:
                return "unknown"
