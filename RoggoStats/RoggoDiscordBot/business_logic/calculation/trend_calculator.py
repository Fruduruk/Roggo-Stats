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
    plt.figure(figsize=(12, 6))
    colors = ["b", "r", "g", "y"]

    all_dates = sorted({date for value_map in maps for date in value_map.values.keys()})
    date_labels = [date.strftime("%Y-%m-%d") for date in all_dates]

    tick_interval = len(all_dates) // 10
    tick_interval = max(1, tick_interval)

    tick_indices = range(0, len(all_dates), tick_interval)
    tick_labels = [date_labels[i] for i in tick_indices]

    for value_map, color in zip(maps, colors):
        if len(value_map.values) > 0:
            values = [value_map.values.get(date) for date in all_dates]

            nums, valid_values = zip(*[(i, val) for i, val in enumerate(values) if val is not None])

            if valid_values:
                m, b = np.polyfit(nums, valid_values, 1)
                plt.plot(nums, valid_values, marker="o", linestyle=" ", color=color, label=f"{value_map.name}")
                plt.plot(nums, m * np.array(nums) + b, linestyle="--", color=color)

    plt.xticks(ticks=tick_indices, labels=tick_labels, rotation=45)
    plt.title("Trend over time")
    plt.xlabel("Time")
    plt.ylabel(str(statistic))
    plt.legend()
    plt.tight_layout()

    path = "trend_over_time.png"
    plt.savefig(path)
    plt.close()
    return path


def get_value(advanced_player: bc.AdvancedPlayer, statistic: Statistic) -> float:
    match statistic:
        case Statistic.PERCENT_SUPERSONIC_SPEED:
            return advanced_player.stats.movement.percentSupersonicSpeed
        case Statistic.BOOST_COLLECTED_PER_MINUTE:
            return advanced_player.stats.boost.generalBoost.bcpm


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
