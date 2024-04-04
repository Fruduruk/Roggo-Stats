from enum import Enum


class Statistic(Enum):
    PERCENT_SUPERSONIC_SPEED = 0

    def __str__(self) -> str:
        match self:
            case Statistic.PERCENT_SUPERSONIC_SPEED:
                return "percent supersonic speed"
            case None:
                return "unknown"
