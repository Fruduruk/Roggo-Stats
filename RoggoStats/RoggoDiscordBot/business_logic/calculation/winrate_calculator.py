import ballchasing_pb2 as bc
from business_logic.calculation.calc_utils import won
from business_logic.grpc.grpc_client import get_simple_replays
from business_logic.grpc.grpc_helper_functions import find_name_of_identity_in_simple_replays
from business_logic.utils import get_basic_result
from models.winrate_result import WinrateResult


async def calculate_winrate(request: bc.FilterRequest) -> WinrateResult:
    replays = await get_simple_replays(request)
    if not replays:
        replays = []
    if len(replays) == 0:
        return WinrateResult(
            get_basic_result(request, len(replays), names=[identity.nameOrId for identity in request.identities]))

    names = [find_name_of_identity_in_simple_replays(identity, replays) for identity in request.identities]
    wr_result = WinrateResult(get_basic_result(request, len(replays), names))

    if replays:
        won_count = sum(won(replay, request.identities) for replay in replays)
        wr_result.winrate = won_count / len(replays)

    return wr_result
