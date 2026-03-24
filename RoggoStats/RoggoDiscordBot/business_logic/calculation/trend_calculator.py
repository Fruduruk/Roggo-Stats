from datetime import datetime
from typing import List

import ballchasing_pb2 as bc
import matplotlib.pyplot as plt
import numpy as np

from business_logic.calculation.calc_utils import (
    find_advanced_player_in_advanced_replay,
)
from business_logic.calculation.value_fetcher import get_value
from business_logic.grpc.grpc_client import get_advanced_replays
from business_logic.grpc.grpc_helper_functions import (
    find_name_of_identity_in_simple_replays,
    find_name_of_identity_in_advanced_replays,
)
from business_logic.utils import get_basic_result
from models.time_series_player_stats import TimeSeriesPlayerStats
from models.statistic import Statistic
from models.image_result import ImageResult


def generate_image(maps: List[TimeSeriesPlayerStats], statistic: Statistic) -> str:
    fig = plt.figure(figsize=(10, 6))
    fig.patch.set_facecolor("#1e1e1e")
    ax = plt.gca()
    ax.set_facecolor("#252526")
    colors = ["#ADF467", "#3CA5F0", "#FA4B63", "#F07B3C"]

    all_dates = sorted({date for value_map in maps for date in value_map.values.keys()})
    date_labels = [date.strftime("%Y-%m-%d") for date in all_dates]

    tick_interval = len(all_dates) // 10
    tick_interval = max(1, tick_interval)

    tick_indices = range(0, len(all_dates), tick_interval)
    tick_labels = [date_labels[i] for i in tick_indices]

    for value_map, color in zip(maps, colors):
        if len(value_map.values) > 0:
            values = [value_map.values.get(date) for date in all_dates]

            nums, valid_values = zip(
                *[(i, val) for i, val in enumerate(values) if val is not None]
            )

            if valid_values:
                ax.plot(
                    nums,
                    valid_values,
                    marker="o",
                    linestyle=" ",
                    color=color,
                    label=f"{value_map.name}",
                )

    for value_map, color in zip(maps, colors):
        if len(value_map.values) > 0:
            values = [value_map.values.get(date) for date in all_dates]

            nums, valid_values = zip(
                *[(i, val) for i, val in enumerate(values) if val is not None]
            )

            if valid_values:
                m, b = np.polyfit(nums, valid_values, 1)
                ax.plot(nums, m * np.array(nums) + b, linestyle="-", color=color)

    ax.set_xticks(list(tick_indices))
    # ax.set_ylim(bottom=0)
    ax.set_xticklabels(tick_labels, rotation=45)
    ax.set_title(str(statistic).upper(), fontweight="bold", color="#d4d4d4")
    ax.set_xlabel("Time", color="#d4d4d4")
    ax.set_ylabel(str(statistic), color="#d4d4d4")
    ax.tick_params(colors="#d4d4d4")

    for spine in ax.spines.values():
        spine.set_color("#3c3c3c")

    legend = ax.legend()
    if legend is not None:
        legend.get_frame().set_facecolor("#252526")
        legend.get_frame().set_edgecolor("#3c3c3c")
        for text in legend.get_texts():
            text.set_color("#d4d4d4")

    plt.tight_layout()

    path_names = "-".join([stat.name for stat in maps])
    pat_date = datetime.now().strftime("%Y-%m-%d-%H-%M-%S")
    path = f"trend_over_time_{str(statistic)}_{path_names}_{pat_date}.png"
    plt.savefig(path, facecolor=fig.get_facecolor())
    plt.close()
    return path


async def calculate_trend(
    request: bc.FilterRequest, statistic: Statistic
) -> ImageResult:
    replays = await get_advanced_replays(request)
    if not replays:
        replays = []
    if len(replays) == 0:
        return ImageResult(
            get_basic_result(
                request,
                len(replays),
                names=[identity.nameOrId for identity in request.identities],
            )
        )

    names = [
        find_name_of_identity_in_advanced_replays(identity, replays)
        for identity in request.identities
    ]
    trend_result = ImageResult(get_basic_result(request, len(replays), names=names))
    trend_result.stat_name = str(statistic)

    if len(replays) == 0:
        return trend_result

    replays.reverse()

    player_statistic_value_maps = []
    for identity in request.identities:
        value_tuple_list: list[tuple[datetime, float]] = [
            (
                datetime.fromisoformat(replay.date.replace("Z", "+00:00")),
                get_value(
                    find_advanced_player_in_advanced_replay(replay, identity), statistic
                ),
            )
            for replay in replays
            if find_advanced_player_in_advanced_replay(replay, identity) is not None
        ]

        player_stats = TimeSeriesPlayerStats(
            name=find_name_of_identity_in_advanced_replays(identity, replays),
            tuples=value_tuple_list,
        )

        player_statistic_value_maps.append(player_stats)

    path = generate_image(player_statistic_value_maps, statistic)
    trend_result.image_path = path
    return trend_result
