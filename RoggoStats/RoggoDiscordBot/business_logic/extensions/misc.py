from business_logic.grpc.grpc_client import get_simple_replays
from business_logic.grpc.grpc_helper_functions import to_identity
import interactions
from interactions import (
    Extension, slash_command, SlashContext, slash_option, OptionType, SlashCommandChoice
)

print("loading misc extension...")


class Misc(Extension):
    @slash_command(name="get-20-titles", description="Erhalte die letzten 20 Titel der Replays eines Spielers")
    async def get_20_titles(self, ctx: SlashContext): await ctx.send("Titles:")


async def replays(ctx, name: str):
    identity = to_identity(name)

    advanced_replays = await get_simple_replays([identity])
    if advanced_replays:
        titles = [replay.title for replay in advanced_replays]
        await ctx.respond("\n".join(titles))
    else:
        await ctx.respond("no replay found.")
