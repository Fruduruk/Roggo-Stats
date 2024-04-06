# Roggo Stats (RocketLeagueStats)

This project uses the ballchasing.com api to calculate interesting statistics.

Docs: https://ballchasing.com/doc/api

### Old Rocket League Stats
The old RocketLeagueStats project was a WPF app, a Windows background service and a Discord bot, all written in C#.

## BallchasingWrapper
The core of the new project (RoggoStats) is also written in C#.
I took some code from the old project and made a gRPC service called BallchasingWrapper. This service also contains the old background service. Its purpose is to use the ballchasing api as efficiently as possible by using Mongo DB as a cache.

Every other service uses this gRPC service as the base.

## RoggoDiscordBot
I have rewritten the old discord bot in python with new slash-commands and way better plotting.

### Deployment
The new services are all Linux-compatible and can be provided via Docker containers.
