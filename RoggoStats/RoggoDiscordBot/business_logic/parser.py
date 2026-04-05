import re

from business_logic.grpc.grpc_helper_functions import to_identity
from db.storage import try_load_steam_id
import ballchasing_pb2 as bc


def try_parse_discord_id(discord_id: str) -> int | None:
    match = re.fullmatch(r"<@!?(\d+)>", discord_id.strip())
    if match is None:
        return None
    return int(match.group(1))


def parse_names(names: str) -> tuple[list[bc.Identity], list[str]]:
    if not names:
        return [],[]
    strings = [
        name.strip() for name in names.split(",")
    ]
    identities = []
    unknown_identities = []
    for name in strings:
        if "@" not in name:
            identities.append(to_identity(name))
            continue

        discord_id = try_parse_discord_id(name)
        if discord_id is None:
            unknown_identities.append(name)
            continue

        steam_id = try_load_steam_id(discord_id)
        if steam_id is None:
            unknown_identities.append(name)
            continue

        identities.append(bc.Identity(
            identityType=bc.STEAM_ID, nameOrId=str(steam_id)))
        
    return identities, unknown_identities
