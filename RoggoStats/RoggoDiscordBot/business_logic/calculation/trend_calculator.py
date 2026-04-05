import ballchasing_pb2 as bc

from business_logic.calculation.calculate_value_maps import calculate_value_maps
from business_logic.calculation.image_generator import generate_image
from business_logic.grpc.grpc_client import get_advanced_replays
from business_logic.grpc.grpc_helper_functions import (
    find_name_of_identity_in_advanced_replays,
)
from business_logic.utils import get_basic_result
from models.statistic import Statistic
from models.image_result import ImageResult


async def calculate_trend(
    request: bc.FilterRequest,
    statistic: Statistic,
    total_y_axis: bool
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
    trend_result = ImageResult(get_basic_result(
        request, len(replays), names=names))
    trend_result.stat_name = str(statistic)

    if len(replays) == 0:
        return trend_result

    replays.reverse()

    player_statistic_value_maps = await calculate_value_maps(
        replays=replays,
        request=request,
        statistic=statistic
    )

    path = generate_image(player_statistic_value_maps, statistic, total_y_axis)
    trend_result.image_path = path
    return trend_result
