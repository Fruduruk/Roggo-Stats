from typing import List

import ballchasing_pb2 as bc


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


def identity_equals_simple_player(identity: bc.Identity, player: bc.Player) -> bool:
    if identity.identityType == bc.IdentityType.NAME and identity.nameOrId == player.name:
        return True
    else:
        return to_platform(identity.identityType) == player.id.platform and identity.nameOrId == player.id.id


def identity_equals_advanced_player(identity: bc.Identity, advanced_player: bc.AdvancedPlayer) -> bool:
    if identity.identityType == bc.IdentityType.NAME and identity.nameOrId == advanced_player.name:
        return True
    else:
        return to_platform(identity.identityType) == advanced_player.id.platform and identity.nameOrId == advanced_player.id.id


def to_identity(name: str):
    if name == "danschl":
        return bc.Identity(identityType=bc.STEAM_ID, nameOrId="76561198095673686")
    if name == "lön":
        return bc.Identity(identityType=bc.STEAM_ID, nameOrId="76561198129759987")
    if name == "jan":
        return bc.Identity(identityType=bc.STEAM_ID, nameOrId="76561198269365717")
    if name == "keno":
        return bc.Identity(identityType=bc.STEAM_ID, nameOrId="76561198125752938")

    return bc.Identity(identityType=bc.NAME, nameOrId=name)


def find_name_in_advanced_team(identity: bc.Identity, advanced_team: bc.AdvancedTeam) -> str:
    for player in advanced_team.players:
        if identity_equals_simple_player(identity, player):
            return player.name
    return None


def find_name_of_identity_in_advanced_replays(identity: bc.Identity, advanced_replays: List[bc.AdvancedReplay]) -> str:
    if identity.identityType == bc.IdentityType.NAME:
        return identity.nameOrId

    for replay in advanced_replays:
        name = find_name_in_advanced_team(identity, replay.blue)
        if name:
            return name
        name = find_name_in_advanced_team(identity, replay.orange)
        if name:
            return name
    return identity.nameOrId


def find_name_in_simple_team(identity: bc.Identity, team: bc.Team) -> str:
    for player in team.players:
        if identity_equals_simple_player(identity, player):
            return player.name
    return None


def find_name_of_identity_in_simple_replays(identity: bc.Identity, replays: List[bc.Replay]) -> str:
    if identity.identityType == bc.IdentityType.NAME:
        return identity.nameOrId

    for replay in replays:
        name = find_name_in_simple_team(identity, replay.blue)
        if name:
            return name
        name = find_name_in_simple_team(identity, replay.orange)
        if name:
            return name
    return identity.nameOrId
