import os

DISCORD_BOT_TOKEN = os.getenv('DISCORD_BOT_TOKEN')
BALLCHASING_HOST = os.getenv('BALLCHASING_HOST')
BALLCHASING_PORT = os.getenv('BALLCHASING_PORT')

def check_environment_variables() -> bool:
    missing_vars = []
    if DISCORD_BOT_TOKEN is None:
        missing_vars.append("DISCORD_BOT_TOKEN")
    if BALLCHASING_HOST is None:
        missing_vars.append("BALLCHASING_HOST")
    if BALLCHASING_PORT is None:
        missing_vars.append("BALLCHASING_PORT")

    if missing_vars:
        for var in missing_vars:
            print(f"Please set the {var} environment variable.")
        return False
    return True