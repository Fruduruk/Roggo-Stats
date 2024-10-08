import ballchasing_pb2 as bc
from business_logic.calculation.double_trend_calculator import calculate_double_trend
from business_logic.embedder import create_error_embed, create_trend_embed

from interactions import (
    Extension, slash_command, SlashContext, slash_option, OptionType, SlashCommandChoice
)

from business_logic.grpc.grpc_helper_functions import to_identity
from models.statistic import Statistic

print("loading double trend extension...")


class Trend(Extension):
    @slash_command(name="double-trend", description="Erhalte einen statistik trend für gegebene Spieler")
    @slash_option(
        name="statistic",
        description="Wähle einen Wert",
        required=True,
        opt_type=OptionType.INTEGER,
        choices=[
            SlashCommandChoice(name=str(Statistic.BOOST_USED_PER_MINUTE), value=0),
            SlashCommandChoice(name=str(Statistic.BOOST_COLLECTED_PER_MINUTE), value=1),
            SlashCommandChoice(name=str(Statistic.BOOST_AMOUNT_STOLEN), value=2),
            SlashCommandChoice(name=str(Statistic.BOOST_AMOUNT_USED_WHILE_SUPERSONIC), value=3),
            SlashCommandChoice(name=str(Statistic.BOOST_COLLECTED_SMALL_TO_BIG_RATIO), value=4),
            SlashCommandChoice(name=str(Statistic.PERCENT_SLOW_SPEED), value=5),
            SlashCommandChoice(name=str(Statistic.PERCENT_SUPERSONIC_SPEED), value=6),
            SlashCommandChoice(name=str(Statistic.AVERAGE_SPEED), value=7),
            SlashCommandChoice(name=str(Statistic.PERCENT_HIGH_AIR), value=8),
            SlashCommandChoice(name=str(Statistic.TIME_POWERSLIDE), value=9),
            SlashCommandChoice(name=str(Statistic.COUNT_POWERSLIDE), value=10),
            SlashCommandChoice(name=str(Statistic.AVERAGE_DISTANCE_TO_BALL), value=11),
            SlashCommandChoice(name=str(Statistic.AVERAGE_DISTANCE_TO_MATES), value=12),
            SlashCommandChoice(name=str(Statistic.PERCENT_CLOSEST_TO_BALL), value=13),
            SlashCommandChoice(name=str(Statistic.PERCENT_FARTHEST_FROM_BALL), value=14),
            SlashCommandChoice(name=str(Statistic.PERCENT_MOST_BACK), value=15),
            SlashCommandChoice(name=str(Statistic.PERCENT_MOST_FORWARD), value=16),
            SlashCommandChoice(name=str(Statistic.GOALS_AGAINST_WHILE_LAST_DEFENDER), value=17),
            SlashCommandChoice(name=str(Statistic.DEMOS_INFLICTED), value=18),
            SlashCommandChoice(name=str(Statistic.DEMOS_TAKEN), value=19),
        ]
    )
    @slash_option(
        name="divided_by_statistic",
        description="Wähle einen Wert",
        required=True,
        opt_type=OptionType.INTEGER,
        choices=[
            SlashCommandChoice(name=str(Statistic.BOOST_USED_PER_MINUTE), value=0),
            SlashCommandChoice(name=str(Statistic.BOOST_COLLECTED_PER_MINUTE), value=1),
            SlashCommandChoice(name=str(Statistic.BOOST_AMOUNT_STOLEN), value=2),
            SlashCommandChoice(name=str(Statistic.BOOST_AMOUNT_USED_WHILE_SUPERSONIC), value=3),
            SlashCommandChoice(name=str(Statistic.BOOST_COLLECTED_SMALL_TO_BIG_RATIO), value=4),
            SlashCommandChoice(name=str(Statistic.PERCENT_SLOW_SPEED), value=5),
            SlashCommandChoice(name=str(Statistic.PERCENT_SUPERSONIC_SPEED), value=6),
            SlashCommandChoice(name=str(Statistic.AVERAGE_SPEED), value=7),
            SlashCommandChoice(name=str(Statistic.PERCENT_HIGH_AIR), value=8),
            SlashCommandChoice(name=str(Statistic.TIME_POWERSLIDE), value=9),
            SlashCommandChoice(name=str(Statistic.COUNT_POWERSLIDE), value=10),
            SlashCommandChoice(name=str(Statistic.AVERAGE_DISTANCE_TO_BALL), value=11),
            SlashCommandChoice(name=str(Statistic.AVERAGE_DISTANCE_TO_MATES), value=12),
            SlashCommandChoice(name=str(Statistic.PERCENT_CLOSEST_TO_BALL), value=13),
            SlashCommandChoice(name=str(Statistic.PERCENT_FARTHEST_FROM_BALL), value=14),
            SlashCommandChoice(name=str(Statistic.PERCENT_MOST_BACK), value=15),
            SlashCommandChoice(name=str(Statistic.PERCENT_MOST_FORWARD), value=16),
            SlashCommandChoice(name=str(Statistic.GOALS_AGAINST_WHILE_LAST_DEFENDER), value=17),
            SlashCommandChoice(name=str(Statistic.DEMOS_INFLICTED), value=18),
            SlashCommandChoice(name=str(Statistic.DEMOS_TAKEN), value=19),
        ]
    )
    @slash_option(
        name="names",
        description="Trage Namen getrennt mit Komma ein",
        required=True,
        opt_type=OptionType.STRING,
    )
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
                    statistic: int,
                    divided_by_statistic: int,
                    names: str,
                    time_range: int = None,
                    playlist: int = None,
                    match_type: int = None,
                    group_type: int = 0):
        print(f"calculating trend for {time_range},{names},{playlist},{match_type},{statistic}/{divided_by_statistic}...")
        message = await ctx.send("Roggo Stats is thinking...")
        # noinspection PyBroadException
        # broad except so it will never reach the user
        try:
            trend_result = await calculate_double_trend(
                request=bc.FilterRequest(
                    identities=[to_identity(name.strip()) for name in
                                names.split(",")],
                    groupType=group_type,
                    playlist=playlist if playlist else bc.Playlist.ALL,
                    matchType=match_type if match_type else bc.MatchType.BOTH,
                    timeRange=time_range if time_range else bc.TimeRange.EVERY_TIME,
                ),
                statistic=Statistic(statistic),
                divided_by_statistic=Statistic(divided_by_statistic)
            )
            await message.edit(
                content="",
                embed=create_trend_embed(trend_result),
                file=trend_result.image_path,
            )
        except:
            await ctx.send(embed=create_error_embed())
