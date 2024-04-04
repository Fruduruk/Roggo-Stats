import ballchasing_pb2 as bc
from business_logic.calculation.trend_calculator import calculate_trend
from business_logic.embedder import create_error_embed, create_trend_embed

from interactions import (
    Extension, slash_command, SlashContext, slash_option, OptionType, SlashCommandChoice
)

from business_logic.grpc.grpc_helper_functions import to_name_identity

print("loading winrate extension...")


class Trend(Extension):
    @slash_command(name="trend", description="Erhalte einen statistik trend für gegebene Spieler")
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
        name="group_type",
        description="Wähle, ob die Spieler in einem Team gespielt haben müssen, oder ob auch solo games erlaubt sind",
        required=False,
        opt_type=OptionType.INTEGER,
        choices=[
            SlashCommandChoice(name="Together", value=0),
            SlashCommandChoice(name="Individually", value=1),
        ]
    )
    async def trend(self, ctx: SlashContext,
                    time_range: int,
                    names: str,
                    playlist: int = None,
                    match_type: int = None,
                    group_type: int = 0):
        message = await ctx.send(
            "Roggo Stats is thinking...\n" +
            "Trend calculations are hard work, all the number crunching, please wait, this may take a while...")
        # noinspection PyBroadException
        # broad except so it will never reach the user
        # try:
        trend_result = await calculate_trend(
            request=bc.FilterRequest(
                identities=[to_name_identity(name) for name in
                            names.split(",")],
                groupType=group_type,
                playlist=playlist if playlist else bc.Playlist.ALL,
                matchType=match_type if match_type else bc.MatchType.BOTH,
                timeRange=time_range if time_range else bc.TimeRange.EVERY_TIME,
            )
        )
        await message.edit(
            content="Roggo Stats computed for you:",
            embed=create_trend_embed(trend_result),
            file=trend_result.image_path,
        )
# except:
# await ctx.send(embed=create_error_embed())
