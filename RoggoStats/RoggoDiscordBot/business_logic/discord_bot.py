import interactions

from business_logic.extensions_loader import load_extensions
from interactions.models import discord

print("loading discord bot...")
bot = interactions.Client()
print("loading extensions...")
load_extensions(bot)


@bot.listen()
async def on_startup():
    activity = discord.Activity.create(
        name="Rocket League Replays",
        type=discord.activity.ActivityType.WATCHING
    )
    await bot.change_presence(status=discord.Status.ONLINE, activity=activity)
