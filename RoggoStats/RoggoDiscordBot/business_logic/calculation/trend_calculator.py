from datetime import datetime
from typing import List, Mapping

import ballchasing_pb2 as bc
import matplotlib.pyplot as plt
import numpy as np

from business_logic.calculation.calc_utils import find_advanced_player_in_advanced_replay
from business_logic.grpc.grpc_client import get_advanced_replays
from business_logic.utils import get_basic_result
from models.time_series_player_stats import TimeSeriesPlayerStats
from models.statistic import Statistic
from models.trend_result import TrendResult


def generate_image(maps: List[TimeSeriesPlayerStats], statistic: Statistic) -> str:
    plt.figure(figsize=(10, 6))
    colors = ["g", "y", "r", "b"]

    for value_map in maps:
        values = [value for value in value_map.values.values()]
        dates = [str(key) for key in value_map.values.keys()]

        nums = np.arange(len(values))

        m, b = np.polyfit(nums, values, 1)
        color = colors.pop()
        plt.plot(values,  linestyle="-", color=color)
        plt.plot(m * nums + b, linestyle="--", label="Trend line", color=color)

    plt.title("Trend over time")
    plt.xlabel("Time")
    plt.ylabel(str(statistic))
    plt.grid(False)

    path = "trend_over_time.png"
    plt.savefig(path)
    plt.close()
    return path


def get_value(advanced_player: bc.AdvancedPlayer, statistic: Statistic) -> float:
    match statistic:
        case Statistic.PERCENT_SUPERSONIC_SPEED:
            return advanced_player.stats.movement.percentSupersonicSpeed


async def calculate_trend(request: bc.FilterRequest, statistic: Statistic) -> TrendResult:
    replays = get_advanced_replays(request)
    trend_result = TrendResult(get_basic_result(request, len(replays)))
    trend_result.stat_name = str(statistic)

    if len(replays) == 0:
        return trend_result

    replays.reverse()

    player_statistic_value_maps = []
    for identity in request.identities:
        value_tuple_list: list[tuple[datetime, float]] = [
            (
                datetime.fromisoformat(replay.date.replace("Z", "+00:00")),
                get_value(find_advanced_player_in_advanced_replay(replay, identity), statistic))
            for replay in replays if
            find_advanced_player_in_advanced_replay(replay, identity) is not None
        ]

        player_stats = TimeSeriesPlayerStats(name=identity.nameOrId, tuples=value_tuple_list)

        player_statistic_value_maps.append(player_stats)

    path = generate_image(player_statistic_value_maps, statistic)
    trend_result.image_path = path
    return trend_result
