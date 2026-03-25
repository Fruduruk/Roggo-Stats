import re


def try_parse_discord_id(discord_id: str) -> int | None:
    match = re.fullmatch(r"<@!?(\d+)>", discord_id.strip())
    if match is None:
        return None
    return int(match.group(1))
