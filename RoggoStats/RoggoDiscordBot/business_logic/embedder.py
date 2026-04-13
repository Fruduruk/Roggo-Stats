import interactions
from interactions.models import discord

from business_logic.utils import (
    time_range_to_string,
    playlist_to_string,
    match_type_to_string,
    group_type_to_string,
)
from models.result import Result
from models.image_result import ImageResult, ImagesResult
from models.winrate_result import WinrateResult


def create_error_embed() -> discord.Embed:
    embed = interactions.Embed(color=discord.BrandColors.RED)
    embed.title = "Da stimmt was nicht."
    return embed


def create_basic_embed(result: Result, inline: bool = False) -> discord.Embed:
    embed = interactions.Embed(color="#569cd6")
    embed.add_field(
        "Time Range", time_range_to_string(result.time_range), inline=inline
    )
    embed.add_field("Playlist", playlist_to_string(result.playlist), inline=inline)
    embed.add_field(
        "Match Type", match_type_to_string(result.match_type), inline=inline
    )
    embed.add_field("Replay Count", result.replay_count, inline=inline)
    embed.add_field("Group Type", group_type_to_string(result.group_type), inline=True)

    return embed


def create_winrate_embed(result: WinrateResult) -> discord.Embed:
    embed = create_basic_embed(result, inline=True)
    embed.title = "Winrate of " + ", ".join(result.names)
    embed.add_field("Winrate", str(result.winrate * 100) + "%")
    return embed


def create_weekday_winrate_embed(result: ImageResult) -> discord.Embed:
    embed = create_basic_embed(result, inline=True)
    embed.title = "Winrate on every weekday of " + ", ".join(
        result.names
    )
    return embed


def create_trend_embed(result: ImageResult) -> discord.Embed:
    embed = create_basic_embed(result, inline=True)
    embed.title = "Trend of " + ", ".join(result.names)
    embed.add_field("Statistic Value", result.stat_name, inline=True)
    return embed


def create_trends_embed(result: ImagesResult) -> discord.Embed:
    embed = create_basic_embed(result, inline=True)
    embed.title = "Trends of " + ", ".join(result.names)
    # embed.add_field("Statistic Values", , inline=True)
    return embed
