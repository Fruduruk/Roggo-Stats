docker build --build-arg APP_VERSION=dev -t ballchasing-wrapper:dev ./BallchasingWrapper/.
docker build --build-arg APP_VERSION=dev -t roggo-discord-bot:dev ./RoggoDiscordBot/.
docker image prune -f