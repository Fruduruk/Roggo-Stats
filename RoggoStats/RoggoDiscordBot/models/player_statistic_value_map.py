from datetime import datetime
from typing import Mapping, List, Tuple


class PlayerStatisticValueMap:
    name: str
    values: Mapping[datetime, float] = {}

    def __init__(self, name: str, tuples: List[Tuple[datetime, float]]):
        self.name: str = name
        self.values: Mapping[datetime, float] = {
            date: value for date, value in tuples
        }
