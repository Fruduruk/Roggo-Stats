import discord
import business_logic.modules.misc_module as misc_module
import business_logic.modules.winrate_module as winrate_module

print("loading discord bot...")
bot = discord.Bot()


@bot.slash_command()
async def replays(ctx, name: str = None):
    if str is None:
        await ctx.respond("Please provide a name.")
    else:
        await misc_module.replays(ctx, name)


@bot.slash_command()
async def winrate(ctx, name: str = None):
    if str is None:
        await ctx.respond("Please provide a name.")
    else:
        await winrate_module.get_winrate_today(ctx, name)
