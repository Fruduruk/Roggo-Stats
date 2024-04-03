from enum import Enum


class TimeRange(Enum):
    TODAY = 1
    YESTERDAY = 2
    WEEK = 3
    MONTH = 4
    YEAR = 5

    def __str__(self):
        match self:
            case TimeRange.TODAY:
                return "today"
            case TimeRange.YESTERDAY:
                return "yesterday"
            case TimeRange.WEEK:
                return "week"
            case TimeRange.MONTH:
                return "month"
            case TimeRange.YEAR:
                return "year"
