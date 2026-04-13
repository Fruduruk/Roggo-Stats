from datetime import datetime

import ballchasing_pb2 as bc
from business_logic.calculation.calc_utils import find_advanced_player_in_advanced_replay
from business_logic.calculation.value_fetcher import get_statistic
from business_logic.grpc.grpc_helper_functions import find_name_of_identity_in_advanced_replays
from models.statistic import Statistic
from models.time_series_player_stats import TimeSeriesPlayerStats


async def calculate_value_maps(
        replays: list[bc.AdvancedReplay],
        request: bc.FilterRequest,
        statistic: Statistic) -> list:
    player_statistic_value_maps = []
    for identity in request.identities:
        value_tuple_list: list[tuple[datetime, float]] = [
            (
                datetime.fromisoformat(replay.date.replace("Z", "+00:00")),
                get_statistic(
                    find_advanced_player_in_advanced_replay(
                        replay, identity), statistic
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

    return player_statistic_value_maps
