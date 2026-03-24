from datetime import datetime
from typing import List

import ballchasing_pb2 as bc
import matplotlib.pyplot as plt
import numpy as np
import calendar
from business_logic.calculation.calc_utils import won
from business_logic.grpc.grpc_client import get_simple_replays
from business_logic.grpc.grpc_helper_functions import (
    find_name_of_identity_in_simple_replays,
)
from business_logic.utils import get_basic_result
from models.image_result import ImageResult
from datetime import datetime

from dataclasses import dataclass


@dataclass
class WeekdayWinrate:
    weekday: str
    winrate: float
    count: int


def generate_image(data: List[WeekdayWinrate]) -> str:
    fig = plt.figure(figsize=(10, 6))
    fig.patch.set_facecolor("#1e1e1e")
    ax = plt.gca()
    ax.set_facecolor("#252526")

    weekdays = [entry.weekday for entry in data]
    winrates = [entry.winrate * 100 for entry in data]
    counts = [entry.count for entry in data]
    ax.axhline(y=50, color="red", linestyle="--", linewidth=1.5, alpha=0.5)

    bars = ax.bar(
        weekdays,
        winrates,
        color="#569cd6",
        edgecolor="#3c3c3c",
        width=0.5,
    )

    ax.set_title("Winrate by Weekday", fontweight="bold", color="#d4d4d4")
    ax.set_xlabel("Weekday", color="#d4d4d4")
    ax.set_ylabel("Winrate (%)", color="#d4d4d4")
    ax.tick_params(colors="#d4d4d4")

    for spine in ax.spines.values():
        spine.set_color("#3c3c3c")

    ax.yaxis.grid(True, color="#3c3c3c", linestyle="-", linewidth=0.8, alpha=0.6)
    ax.set_axisbelow(True)
    
    for bar, value in zip(bars, counts):
        ax.text(
            bar.get_x() + bar.get_width() / 2,
            1,
            value,
            ha="center",
            va="bottom",
            color="#1e1e1e",
            fontsize=10,
        )

    plt.tight_layout()

    pat_date = datetime.now().strftime("%Y-%m-%d-%H-%M-%S")
    path = f"weekday_winrate_{pat_date}.png"
    plt.savefig(path, facecolor=fig.get_facecolor())
    plt.close()
    return path


async def calculate_weekday_winrate(request: bc.FilterRequest) -> ImageResult:
    replays = await get_simple_replays(request)
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
        find_name_of_identity_in_simple_replays(identity, replays)
        for identity in request.identities
    ]
    result = ImageResult(get_basic_result(request, len(replays), names))

    weekday_winrates = []

    for weekday in range(7):
        weekday_replays = [
            replay
            for replay in replays
            if datetime.fromisoformat(replay.date).weekday() == weekday
        ]
        if weekday_replays:
            won_count = sum(
                won(replay, request.identities) for replay in weekday_replays
            )
            weekday_winrate = WeekdayWinrate(
                weekday=calendar.day_name[weekday],
                winrate=won_count / len(weekday_replays),
                count=len(weekday_replays)
            )
            weekday_winrates.append(weekday_winrate)
        else:
            weekday_winrates.append(WeekdayWinrate(
                weekday=calendar.day_name[weekday],
                winrate=0,
                count=0
            ))

    path = generate_image(weekday_winrates)
    result.image_path = path

    return result
