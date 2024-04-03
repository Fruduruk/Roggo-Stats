from business_logic.grpc.grpc_client import get_advanced_replays
from business_logic.grpc.grpc_helper_functions import to_name_identity
import interactions
from interactions import (
    Extension, slash_command, SlashContext, slash_option, OptionType, SlashCommandChoice
)


print("loading winrate extension...")


class Winrate(Extension):
    @slash_command(name="winrate-today", description="Erhalte die heutige Winrate für gegebene Spieler")
    async def winrate_today(self, ctx: SlashContext): await ctx.send("winrate:")

