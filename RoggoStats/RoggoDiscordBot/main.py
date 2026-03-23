import asyncio
import os
import signal
import config
from business_logic.discord_bot import bot


def install_shutdown_handlers():
    def handle_signal(signum, frame):
        print(f"Signal empfangen: {signum}")
        os._exit(0)

    signal.signal(signal.SIGINT, handle_signal)
    signal.signal(signal.SIGTERM, handle_signal)

    if hasattr(signal, "SIGBREAK"):
        signal.signal(signal.SIGBREAK, handle_signal)


async def main() -> None:
    if not config.check_environment_variables():
        print("Stopping program...")
        return

    print("starting discord bot...")
    install_shutdown_handlers()
    await bot.astart(config.DISCORD_BOT_TOKEN)


if __name__ == "__main__":
    asyncio.run(main())
