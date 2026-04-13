@echo off
cd /d "%~dp0"

start "" code ".\RoggoStats\RoggoDiscordBot"
start "" wt -d ".\RoggoStats"
start "" wt -d ".\RoggoStats\BallchasingWrapper"

exit