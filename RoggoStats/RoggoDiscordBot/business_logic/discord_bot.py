import interactions
from business_logic.extensions_loader import load_extensions

print("loading discord bot...")
bot = interactions.Client()
print("loading extensions...")
load_extensions(bot)
