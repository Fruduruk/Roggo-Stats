import config
import BusinessLogic.Modules.misc_module as mm

def main() -> None:
    print('Roggo Discord Bot started.')
    if not config.check_environment_variables():
        print('Stopping program...')
    else:
        mm.bot.run(config.DISCORD_BOT_TOKEN)


if __name__ == '__main__':
    main()
