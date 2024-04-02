from business_logic.grpc.grpc_client import get_advanced_replays
from business_logic.grpc.grpc_helper_functions import to_name_identity

print("loading misc module...")


async def command_is_being_worked_on(ctx):
    return await ctx.respond("🤔")


async def replays(ctx, name: str):
    message = await command_is_being_worked_on(ctx)

    identity = to_name_identity(name)

    replays = await get_advanced_replays([identity])
    if replays:
        titles = [replay.title for replay in replays]
        await ctx.respond("\n".join(titles))
    else:
        await ctx.respond("no replay found.")
