import ballchasing_pb2 as bc
import matplotlib.pyplot as plt
import numpy as np

from business_logic.calculation.calc_utils import find_advanced_player_in_advanced_replay
from business_logic.grpc.grpc_client import get_advanced_replays
from business_logic.utils import get_basic_result
from models.trend_result import TrendResult


async def calculate_trend(request: bc.FilterRequest) -> TrendResult:
    replays = await get_advanced_replays(request)
    trend_result = TrendResult(get_basic_result(request, len(replays)))
    replays.reverse()

    advanced_player = [find_advanced_player_in_advanced_replay(replay, request.identities[0]) for replay in replays if
                       find_advanced_player_in_advanced_replay(replay, request.identities[0]) is not None]
    values = [advanced_player.stats.movement.avgSpeed for advanced_player in advanced_player]
    nums = np.arange(len(values))

    m, b = np.polyfit(nums, values, 1)

    plt.figure(figsize=(10, 6))

    plt.plot(values, marker="o", linestyle="-", color="b")
    plt.plot(m * nums + b, color="r", linestyle="--", label="Trend line")

    plt.title("Trend over time")
    plt.xlabel("Time")
    plt.ylabel("Average Speed")
    plt.grid(False)

    path = "trend_over_time.png"
    plt.savefig(path)
    plt.close()

    trend_result.image_path = path
    return trend_result
