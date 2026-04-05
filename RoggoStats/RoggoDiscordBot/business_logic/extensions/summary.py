

import os

from interactions import Extension, OptionType, SlashCommandChoice, SlashContext, slash_command, slash_option
import ballchasing_pb2 as bc

from business_logic.calculation.summary_calculator import calculate_summary
from business_logic.embedder import create_error_embed, create_trends_embed
from business_logic.parser import parse_names
from business_logic.slash_command_choices import GROUP_TYPE_CHOICES, MATCH_TYPE_CHOICES, PLAYLIST_CHOICES
from business_logic.utils import time_range_to_string
from models.image_result import ImagesResult
from models.statistic import Statistic


print("loading summary extension...")


class Summary(Extension):
    @slash_command(
        name="summary", description="Erhalte eine Zusammenfassung für den heutigen Tag"
    )
    @slash_option(
        name="names",
        description="Trage Namen oder @user getrennt mit Komma ein",
        required=False,
        opt_type=OptionType.STRING
    )
    @slash_option(
        name="time_range",
        description="Wähle ein Zeitintervall",
        required=False,
        opt_type=OptionType.INTEGER,
        choices=[
            SlashCommandChoice(name=time_range_to_string(bc.TimeRange.TODAY), value=1),
            SlashCommandChoice(name=time_range_to_string(bc.TimeRange.YESTERDAY), value=2),
        ],
    )
    @slash_option(
        name="playlist",
        description="Wähle eine Playlist",
        required=False,
        opt_type=OptionType.INTEGER,
        choices=PLAYLIST_CHOICES,
    )
    @slash_option(
        name="match_type",
        description="Wähle ob gewertet oder nicht",
        required=False,
        opt_type=OptionType.INTEGER,
        choices=MATCH_TYPE_CHOICES,
    )
    @slash_option(
        name="group_type",
        description="Wähle, ob die Spieler in einem Team gespielt haben müssen, oder ob auch solo games erlaubt sind",
        required=False,
        opt_type=OptionType.INTEGER,
        choices=GROUP_TYPE_CHOICES,
    )
    @slash_option(
        name="total_y_axis",
        description="Wähle, ob die Y-Achse bei 0 anfangen soll",
        required=False,
        opt_type=OptionType.BOOLEAN
    )
    async def summary(
        self,
        ctx: SlashContext,
        names: str = None,
        time_range: int = 1,
        playlist: int = None,
        match_type: int = None,
        group_type: int = 0,
        total_y_axis: bool = False
    ):
        if not names:
            names = "<@"+str(ctx.user.id)+">"
        identities, unknown_identities = parse_names(names)

        if unknown_identities:
            await ctx.send(f"Users not registered: {unknown_identities}", ephemeral=True)
            return
        print(
            f"calculating trend for {names},{playlist},{match_type},{group_type}..."
        )
        message = await ctx.send("Roggo Stats is thinking...")

        request = bc.FilterRequest(
            identities=identities,
            groupType=group_type,
            playlist=playlist if playlist else bc.Playlist.ALL,
            matchType=match_type if match_type else bc.MatchType.BOTH,
            timeRange=time_range,
        )

        result: ImagesResult = await calculate_summary(
            request=request,
            statistics=[
                Statistic.BOOST_USED_PER_MINUTE,
                Statistic.BOOST_COLLECTED_PER_MINUTE,
                # Statistic.BOOST_AMOUNT_STOLEN,
                # Statistic.BOOST_AMOUNT_USED_WHILE_SUPERSONIC,
                Statistic.BOOST_COLLECTED_SMALL_TO_BIG_RATIO,
                # Statistic.PERCENT_SLOW_SPEED,
                Statistic.PERCENT_SUPERSONIC_SPEED,
                # Statistic.AVERAGE_SPEED,
                Statistic.PERCENT_HIGH_AIR,
                # Statistic.TIME_POWERSLIDE,
                Statistic.COUNT_POWERSLIDE,
                Statistic.AVERAGE_DISTANCE_TO_BALL,
                Statistic.AVERAGE_DISTANCE_TO_MATES,
                Statistic.PERCENT_CLOSEST_TO_BALL,
                # Statistic.PERCENT_FARTHEST_FROM_BALL,
                # Statistic.PERCENT_MOST_BACK,
                # Statistic.PERCENT_MOST_FORWARD,
                Statistic.GOALS_AGAINST_WHILE_LAST_DEFENDER,
                # Statistic.DEMOS_INFLICTED,
                # Statistic.DEMOS_TAKEN,
            ],
            total_y_axis=total_y_axis
        )
        paths = [path for statistic, path in result.statistics_and_paths]
        try:
            await message.edit(
                content="",
                embed=create_trends_embed(result),
                files=paths
            )
        except:
            await ctx.send(embed=create_error_embed())

        for path in paths:
            os.remove(path)
