import interactions
from interactions.models import discord

from business_logic.utils import time_range_to_string, playlist_to_string, match_type_to_string
from models.result import Result
from models.winrate_result import WinrateResult


def create_error_embed() -> discord.Embed:
    embed = interactions.Embed(color=discord.BrandColors.RED)
    embed.title = "Da stimmt was nicht."
    return embed


def create_basic_embed(result: Result) -> discord.Embed:
    embed = interactions.Embed(color=discord.BrandColors.BLURPLE)
    embed.add_field("Time Range", time_range_to_string(result.time_range))
    embed.add_field("Playlist", playlist_to_string(result.playlist))
    embed.add_field("Match Type", match_type_to_string(result.match_type))
    embed.add_field("Replay Count", result.replay_count)
    return embed


def create_winrate_embed(winrate_result: WinrateResult) -> discord.Embed:
    embed = create_basic_embed(winrate_result)
    embed.title = "Winrate of " + str(winrate_result.names)
    embed.add_field("Winrate", str(winrate_result.winrate * 100) + "%")
    return embed
