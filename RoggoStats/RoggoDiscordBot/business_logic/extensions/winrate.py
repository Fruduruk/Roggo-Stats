import ballchasing_pb2 as bc
from business_logic.calculation.winrate_calculator import calculate_winrate
from business_logic.embedder import create_winrate_embed, create_error_embed

from interactions import (
    Extension, slash_command, SlashContext, slash_option, OptionType, SlashCommandChoice
)

from business_logic.grpc.grpc_helper_functions import to_identity

print("loading winrate extension...")


class Winrate(Extension):

    @slash_command(name="winrate", description="Erhalte die Winrate für gegebene Spieler")
    @slash_option(
        name="time_range",
        description="Wähle ein Zeitintervall",
        required=True,
        opt_type=OptionType.INTEGER,
        choices=[
            SlashCommandChoice(name="Today", value=1),
            SlashCommandChoice(name="Yesterday", value=2),
            SlashCommandChoice(name="Week", value=3),
            SlashCommandChoice(name="Month", value=4),
            SlashCommandChoice(name="Year", value=5),
        ]
    )
    @slash_option(
        name="names",
        description="Trage Namen getrennt mit Komma ein",
        required=True,
        opt_type=OptionType.STRING,
    )
    @slash_option(
        name="playlist",
        description="Wähle eine Playlist",
        required=False,
        opt_type=OptionType.INTEGER,
        choices=[
            SlashCommandChoice(name="Duels", value=1),
            SlashCommandChoice(name="Doubles", value=2),
            SlashCommandChoice(name="Standard", value=3),
            SlashCommandChoice(name="Chaos", value=4),
            SlashCommandChoice(name="Private", value=5),
            SlashCommandChoice(name="Offline", value=6),
            SlashCommandChoice(name="Snow Day", value=7),
            SlashCommandChoice(name="Rocket Labs", value=8),
            SlashCommandChoice(name="Hoops", value=9),
            SlashCommandChoice(name="Rumble", value=10),
            SlashCommandChoice(name="Tournament", value=11),
            SlashCommandChoice(name="Drop Shot", value=12),
            SlashCommandChoice(name="Drop Shot Rumble", value=13),
            SlashCommandChoice(name="Heat Seeker", value=14),
        ]
    )
    @slash_option(
        name="match_type",
        description="Wähle ob gewertet oder nicht",
        required=False,
        opt_type=OptionType.INTEGER,
        choices=[
            SlashCommandChoice(name="Ranked", value=1),
            SlashCommandChoice(name="Unranked", value=2)
        ]
    )
    @slash_option(
        name="cap",
        description="Wähle die maximale Anzahl",
        required=False,
        opt_type=OptionType.INTEGER,
    )
    async def winrate(self, ctx: SlashContext,
                      time_range: int,
                      names: str,
                      playlist: int = None,
                      match_type: int = None,
                      cap: int = None):
        print(f"calculating winrate for {time_range},{names},{playlist},{match_type},{cap}...")
        message = await ctx.send("Roggo Stats is thinking...")
        # noinspection PyBroadException
        # broad except so it will never reach the user
        try:
            await message.edit(
                content="Roggo Stats computed for you:",
                embed=create_winrate_embed(
                    winrate_result=await calculate_winrate(
                        request=bc.FilterRequest(
                            replayCap=cap,
                            identities=[to_identity(name.strip()) for name in
                                        names.split(",")],
                            groupType=bc.TOGETHER,
                            playlist=playlist if playlist else bc.Playlist.ALL,
                            matchType=match_type if match_type else bc.MatchType.BOTH,
                            timeRange=time_range if time_range else bc.TimeRange.EVERY_TIME,
                        )
                    )
                )
            )
        except:
            await ctx.send(embed=create_error_embed())
