from datetime import datetime
from typing import List

import ballchasing_pb2 as bc
import matplotlib.pyplot as plt
import calendar
from business_logic.calculation.calc_utils import won
from business_logic.grpc.grpc_client import get_advanced_replays, get_simple_replays
from business_logic.grpc.grpc_helper_functions import (
    find_name_of_identity_in_advanced_replays,
    find_name_of_identity_in_simple_replays,
)
from business_logic.utils import get_basic_result
from models.image_result import ImageResult
from datetime import datetime

from dataclasses import dataclass



async def calculate_core_stats(request: bc.FilterRequest) -> ImageResult:
    replays: list[bc.AdvancedReplay] = await get_advanced_replays(request)
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
    
    replays.reverse()
    for identity in request.identities:
        stats_list = []
        for replay in replays:
            
            




    names = [
        find_name_of_identity_in_advanced_replays(identity, replays)
        for identity in request.identities
    ]
    result = ImageResult(get_basic_result(
        request, len(replays), names=names))
    
    return result
    