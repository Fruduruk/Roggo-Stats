from business_logic.calculation.winrate_calculator import calculate_winrate
from business_logic.embedder import create_winrate_embed
from business_logic.utils import split_strings

from business_logic.enums import TimeRange
from interactions import (
    Extension, slash_command, SlashContext, slash_option, OptionType, SlashCommandChoice
)

print("loading winrate extension...")



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
        embed=create_winrate_embed(winrate_result=calculate_winrate(
            time_range=TimeRange(time_range),
            names=split_strings(names)
        ))
    )