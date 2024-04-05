import interactions
from interactions.models import discord

from business_logic.utils import time_range_to_string, playlist_to_string, match_type_to_string, group_type_to_string
from models.result import Result
from models.trend_result import TrendResult
from models.winrate_result import WinrateResult


def create_error_embed() -> discord.Embed:
    embed = interactions.Embed(color=discord.BrandColors.RED)
    embed.title = "Da stimmt was nicht."
    return embed


def create_basic_embed(result: Result, inline: bool = False) -> discord.Embed:
    embed = interactions.Embed(color=discord.BrandColors.BLURPLE)
    embed.add_field("Time Range", time_range_to_string(result.time_range), inline=inline)
    embed.add_field("Playlist", playlist_to_string(result.playlist), inline=inline)
    embed.add_field("Match Type", match_type_to_string(result.match_type), inline=inline)
    embed.add_field("Replay Count", result.replay_count, inline=inline)
    embed.add_field("Group Type", group_type_to_string(result.group_type), inline=True)

    return embed


def create_winrate_embed(winrate_result: WinrateResult) -> discord.Embed:
    embed = create_basic_embed(winrate_result)
    embed.title = "Winrate of " + ", ".join(winrate_result.names)
    embed.add_field("Winrate", str(winrate_result.winrate * 100) + "%")
    return embed


def create_trend_embed(trend_result: TrendResult) -> discord.Embed:
    embed = create_basic_embed(trend_result, inline=True)
    embed.title = "Trend of " + ", ".join(trend_result.names)
    embed.add_field("Statistic Value", trend_result.stat_name, inline=True)
    return embed
