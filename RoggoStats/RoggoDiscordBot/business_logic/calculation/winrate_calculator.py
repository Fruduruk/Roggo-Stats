from typing import List

import ballchasing_pb2 as bc
from business_logic.grpc.grpc_client import get_simple_replays
from business_logic.utils import get_basic_result
from models.winrate_result import WinrateResult


def to_platform(identity_type: bc.IdentityType):
    match identity_type:
        case bc.IdentityType.NAME:
            return None
        case bc.IdentityType.STEAM_ID:
            return "steam"
        case bc.IdentityType.EPIC_ID:
            return "epic"
        case bc.IdentityType.PS4_GAMER_TAG:
            return "ps4"


def contains_player(identities: List[bc.Identity], player: bc.Player) -> bool:
    return any(
        identity.nameOrId == player.name if identity.identityType == bc.IdentityType.NAME
        else identity.nameOrId == player.id.id and to_platform(identity.identityType) == player.id.platform
        for identity in identities
    )


def contains_all_identities(players: List[bc.Player], identities: List[bc.Identity]):
    contains_count = 0
    for player in players:
        if contains_player(identities, player):
            contains_count += 1
    return contains_count == len(identities)


def won(replay: bc.Replay, identities: List[bc.Identity]) -> bool:
    blue_won = replay.blue.goals > replay.orange.goals
    if contains_all_identities(replay.blue.players, identities):
        return blue_won
    return not blue_won


async def calculate_winrate(request: bc.FilterRequest) -> WinrateResult:
    replays = await get_simple_replays(request)
    wr_result = WinrateResult(get_basic_result(request, len(replays)))

    if replays:
        won_count = sum(won(replay, request.identities) for replay in replays)
        wr_result.winrate = won_count / len(replays)

    return wr_result
