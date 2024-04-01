import BusinessLogic.discord_bot as bot
import ballchasing_pb2
import ballchasing_pb2_grpc
from BusinessLogic.grpc_client import get_advanced_replays


@bot.client.slash_command()
async def replays(ctx, name: str = None):
    if not name:
        await ctx.respond("Bitte gib einen Namen ein.")
        return

    identity = ballchasing_pb2.Identity(identityType=ballchasing_pb2.NAME, nameOrId=name)

    replays = await get_advanced_replays([identity])
    titles = [replay.title for replay in replays]
    await ctx.respond("\n".join(titles))
