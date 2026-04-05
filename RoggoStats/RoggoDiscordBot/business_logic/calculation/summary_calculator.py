import ballchasing_pb2 as bc

from business_logic.calculation.calculate_value_maps import calculate_value_maps
from business_logic.calculation.image_generator import generate_image
from business_logic.grpc.grpc_client import get_advanced_replays
from business_logic.grpc.grpc_helper_functions import (
    find_name_of_identity_in_advanced_replays,
)
from business_logic.utils import get_basic_result
from models.statistic import Statistic
from models.image_result import ImagesResult


async def calculate_summary(
    request: bc.FilterRequest, statistics: list[Statistic]
) -> ImagesResult:
    replays = await get_advanced_replays(request)
    if not replays:
        replays = []
    if len(replays) == 0:
        return ImagesResult(
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
    result = ImagesResult(get_basic_result(
        request, len(replays), names=names))

    if len(replays) == 0:
        return result

    replays.reverse()

    statistics_and_paths = []

    for statistic in statistics:
        player_statistic_value_maps = await calculate_value_maps(
            replays=replays,
            request=request,
            statistic=statistic
        )

        path = generate_image(player_statistic_value_maps, statistic)
        statistics_and_paths.append((str(statistic), path))

    result.statistics_and_paths = statistics_and_paths
    return result
