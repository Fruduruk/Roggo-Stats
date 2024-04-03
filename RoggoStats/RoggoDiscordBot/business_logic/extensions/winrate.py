from business_logic.grpc.grpc_client import get_advanced_replays
from business_logic.grpc.grpc_helper_functions import to_name_identity
from business_logic.utils import split_strings
from interactions.models import discord
from business_logic.enums import TimeRange
import interactions
from interactions import (
    Extension, slash_command, SlashContext, slash_option, OptionType, SlashCommandChoice
)

print("loading winrate extension...")


def create_winrate_embed(time_range: TimeRange, names: list):
    embed = interactions.Embed(color=discord.BrandColors.RED)
    embed.title = "Selected time range: " + str(time_range)
    embed.description = "Selected names: " + str(names)
    return embed


class Winrate(Extension):

    @slash_command(name="winrate", description="Erhalte die Winrate für gegebene Spieler")
    @slash_option(
        name="time_range",
        description="Zeitangabe",
        required=True,
        opt_type=OptionType.INTEGER,
        choices=[
            SlashCommandChoice(name="Today", value=1),
            SlashCommandChoice(name="Yesterday", value=2),
            SlashCommandChoice(name="Week", value=3),
            SlashCommandChoice(name="Month", value=4),
            SlashCommandChoice(name="Year", value=5),
        ]
    )
    @slash_option(
        name="names",
        description="Namen getrennt mit Komma",
        required=True,
        opt_type=OptionType.STRING,
    )
    async def winrate(self, ctx: SlashContext,
                      time_range: int,
                      names: str): await ctx.send(
        embed=create_winrate_embed(
            time_range=TimeRange(time_range),
            names=split_strings(names)
        )
    )

    # @winrate.subcommand(sub_cmd_name="today",
    #                     sub_cmd_description="Erhalte die heutige Winrate für gegebene Spieler")
    # async def winrate_today(self, ctx: SlashContext): await ctx.send(embed =create_winrate_embed(TimeRange.TODAY,names))
    #
    # @winrate.subcommand(sub_cmd_name="week",
    #                     sub_cmd_description="Erhalte die Winrate der letzten Woche für gegebene Spieler")
    # async def winrate_week(self, ctx: SlashContext): await ctx.send("winrate week:")
    #
    # @winrate.subcommand(sub_cmd_name="month",
    #                     sub_cmd_description="Erhalte die Winrate des letzten Monats für gegebene Spieler")
    # async def winrate_month(self, ctx: SlashContext): await ctx.send("winrate month:")
