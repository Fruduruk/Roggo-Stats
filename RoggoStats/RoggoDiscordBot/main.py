import config
from business_logic.discord_bot import bot

def main() -> None:
    if not config.check_environment_variables():
        print('Stopping program...')
    else:
        print('starting discord bot...')
        bot.start(config.DISCORD_BOT_TOKEN)
        print('Roggo Discord Bot started.')


if __name__ == '__main__':
    main()
