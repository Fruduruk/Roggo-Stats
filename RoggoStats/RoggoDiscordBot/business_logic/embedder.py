import interactions
from interactions.models import discord

from business_logic.utils import time_range_to_string, playlist_to_string, match_type_to_string
from models.winrate_result import WinrateResult


def create_winrate_embed(winrate_result: WinrateResult):
    embed = interactions.Embed(color=discord.BrandColors.BLURPLE)
    embed.title = "Winrate of " + str(winrate_result.names)
    embed.add_field("Winrate", str(winrate_result.winrate) + "%")
    embed.add_field("Time Range", time_range_to_string(winrate_result.time_range))
    embed.add_field("Playlist", playlist_to_string(winrate_result.playlist))
    embed.add_field("Match Type", match_type_to_string(winrate_result.match_type))
    embed.add_field("Replay Count", winrate_result.replay_count)
    return embed
