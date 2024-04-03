from business_logic.enums import TimeRange
from business_logic.grpc.grpc_client import get_simple_replays
from business_logic.grpc.grpc_helper_functions import to_name_identity
from models.winrate_result import WinrateResult


async def calculate_winrate(time_range: TimeRange, names: list) -> WinrateResult:
    replays = await get_simple_replays([to_name_identity(name) for name in names])

    result = WinrateResult()
    result.winrate = len(replays)
    result.names = names
    result.time_range = time_range
    return result
