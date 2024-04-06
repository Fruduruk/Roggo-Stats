import config
from business_logic.discord_bot import bot


def main() -> None:
    if not config.check_environment_variables():
        print('Stopping program...')
    else:
        print('starting discord bot...')
        bot.start(config.DISCORD_BOT_TOKEN)
        print('Stopping program...')


if __name__ == '__main__':
    main()
