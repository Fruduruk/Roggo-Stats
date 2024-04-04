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


def identity_equals_player(identity: bc.Identity, player: bc.Player) -> bool:
    if identity.identityType == bc.IdentityType.NAME and identity.nameOrId == player.name:
        return True
    else:
        return to_platform(identity.identityType) == player.id.platform and identity.nameOrId == player.id.id


def find_advanced_player_in_advanced_team(team: bc.AdvancedTeam, identity: bc.Identity) -> bc.AdvancedPlayer:
    for player in team.players:
        if identity_equals_player(identity, player):
            return player
    return None


def find_advanced_player_in_advanced_replay(replay: bc.AdvancedReplay, identity: bc.Identity) -> bc.AdvancedPlayer:
    player = find_advanced_player_in_advanced_team(replay.blue, identity)
    if player:
        return player
    player = find_advanced_player_in_advanced_team(replay.orange, identity)
    if player:
        return player
    return None


def contains_player(identities: List[bc.Identity], player: bc.Player) -> bool:
    for identity in identities:
        if identity_equals_player(identity, player):
            return True

    return False


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
