import interactions
from interactions.api.events import Startup
from config import APP_VERSION
from business_logic.extensions_loader import load_extensions
from interactions.models import discord

print("loading discord bot...")
bot = interactions.Client(debug_scope=806067744704823328)
# bot = interactions.Client()
print("loading extensions...")
load_extensions(bot)


@bot.listen(Startup)
async def on_startup():
    activity = discord.Activity.create(
        name="Version: " + APP_VERSION, type=discord.activity.ActivityType.PLAYING
    )
    await bot.change_presence(status=discord.Status.ONLINE, activity=activity)
