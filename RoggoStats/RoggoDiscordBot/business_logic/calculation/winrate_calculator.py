from business_logic.enums import TimeRange
from models.winrate_result import WinrateResult


def calculate_winrate(time_range: TimeRange, names: list) -> WinrateResult:
    

    result = WinrateResult()
    result.winrate = 50
    result.names = names
    result.time_range = time_range
    return result
