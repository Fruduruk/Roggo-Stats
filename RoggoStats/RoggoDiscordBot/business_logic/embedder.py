import interactions
from interactions.models import discord

from business_logic.enums import TimeRange


def create_winrate_embed(winrate_result):
    embed = interactions.Embed(color=discord.BrandColors.BLURPLE)
    embed.title = "Winrate of " + str(winrate_result.names)
    embed.add_field("Winrate", winrate_result.winrate)
    embed.description = "For time range: " + str(winrate_result.time_range)
    return embed
