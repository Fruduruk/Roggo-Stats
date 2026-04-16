## Avaliable Data
Rocket League statistics for uploaded replays to https://ballchasing.com
- core (score, goals, assists, demolitions, ...)
- boost (boost used, boost collected, stolen boost pads, ...)
- positioning (% most back, % closest to ball, ...)
- movement (distance traveled, average speed, % high air, powerslide count, ...)
- rank information (players rank, max/min rank of a team)
- *camera settings*

## Definitions
- A team-session is a sequence of replays played by the same team, where no gap between two consecutive replays exceeds one hour.
- A solo-session is a sequence of replays played by one player, where no gap between two censecutive replays exceeds one hour.

# Requirements
## Installation
- The bot shall inform the user that uploading replays to ballchasing.com is necessary.
- The bot shall inform the user that linking their Rocket League identity to their discord identity is necessary.
- The bot shall give the user an option to link their Rocket League identity based on uploaded replays.
## Team Session Summary
- The bot shall infer the latest team-session from the users identity and uploaded replays.
- *The bot shall summarize statistics for the user after the session via slash command.*
## Solo Session Summary
- The bot shall infer the latest solo-session from the users identity and uploaded replays.
- The bot shall combine all team mate stats into "the average teammate".
- *The bot shall summarize statistics for the user after the session via slash command.*
## Trend
- *The bot shall give the user a trend over time for specific statistics*