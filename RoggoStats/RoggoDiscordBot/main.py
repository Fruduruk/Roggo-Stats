import grpc
import os
import discord
import ballchasing_pb2
from typing import List
import ballchasing_pb2_grpc

DISCORD_BOT_TOKEN = os.getenv('DISCORD_BOT_TOKEN')
BALLCHASING_HOST = os.getenv('BALLCHASING_HOST')
BALLCHASING_PORT = os.getenv('BALLCHASING_PORT')
bot = discord.Bot()


def create_stub() -> ballchasing_pb2_grpc.BallchasingStub:
    channel = grpc.insecure_channel(BALLCHASING_HOST + ':' + BALLCHASING_PORT)
    return ballchasing_pb2_grpc.BallchasingStub(channel)


async def get_advanced_replays(stub: ballchasing_pb2_grpc.BallchasingStub,
                               identities: List[ballchasing_pb2.Identity]) -> None:
    request = ballchasing_pb2.FilterRequest(
        replayCap=20,
        identities=identities,
        groupType=ballchasing_pb2.TOGETHER,
        playlist=ballchasing_pb2.DOUBLES,
        matchType=ballchasing_pb2.RANKED
    )

    try:
        response = stub.GetAdvancedReplays(request)
        return response.replays
    except grpc.RpcError as e:
        print(f"Ein Fehler ist aufgetreten: {e}")


def main() -> None:
    print('Roggo Discord Bot started.')
    bot.run(DISCORD_BOT_TOKEN)


@bot.slash_command()
async def replays(ctx, name: str = None):
    if not name:
        await ctx.respond("Bitte gib einen Namen ein.")
        return

    identity = ballchasing_pb2.Identity(identityType=ballchasing_pb2.NAME, nameOrId=name)
    stub = create_stub()

    replays = await get_advanced_replays(stub, [identity])
    titles = [replay.title for replay in replays]
    await ctx.respond("\n".join(titles))


if __name__ == '__main__':
    main()
