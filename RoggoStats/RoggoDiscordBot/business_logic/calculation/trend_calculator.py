import ballchasing_pb2 as bc
import matplotlib.pyplot as plt

from business_logic.calculation.calc_utils import find_advanced_player_in_advanced_replay
from business_logic.grpc.grpc_client import get_advanced_replays
from business_logic.utils import get_basic_result
from models.trend_result import TrendResult


async def calculate_trend(request: bc.FilterRequest) -> TrendResult:
    replays = await get_advanced_replays(request)
    trend_result = TrendResult(get_basic_result(request, len(replays)))
    replays.reverse()

    dates = [replay.created for replay in replays if
             find_advanced_player_in_advanced_replay(replay, request.identities[0]) is not None]
    values = [find_advanced_player_in_advanced_replay(replay, request.identities[0]) for replay in replays if
              find_advanced_player_in_advanced_replay(replay, request.identities[0]) is not None]


    plt.figure(figsize=(10, 6))

    return trend_result
