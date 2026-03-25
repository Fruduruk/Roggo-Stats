import os

import ballchasing_pb2 as bc
from business_logic.calculation.trend_calculator import calculate_trend
from business_logic.embedder import create_error_embed, create_trend_embed

from interactions import (
    Extension,
    slash_command,
    SlashContext,
    slash_option,
    OptionType,
)


from business_logic.parser import parse_names
from business_logic.slash_command_choices import GROUP_TYPE_CHOICES, MATCH_TYPE_CHOICES, PLAYLIST_CHOICES, STATISTIC_CHOICES, TIME_CHOICES
from business_logic.grpc.grpc_helper_functions import to_identity
from models.statistic import Statistic

print("loading trend extension...")

class Trend(Extension):
    @slash_command(
        name="trend", description="Erhalte einen statistik trend für gegebene Spieler"
    )
    @slash_option(
        name="statistic",
        description="Wähle einen Wert",
        required=True,
        opt_type=OptionType.INTEGER,
        choices=STATISTIC_CHOICES,
    )
    @slash_option(
        name="names",
        description="Trage Namen oder @user getrennt mit Komma ein",
        required=True,
        opt_type=OptionType.STRING,
    )
    @slash_option(
        name="time_range",
        description="Wähle ein Zeitintervall",
        required=True,
        opt_type=OptionType.INTEGER,
        choices=TIME_CHOICES,
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
    async def trend(
        self,
        ctx: SlashContext,
        statistic: int,
        names: str,
        time_range: int = None,
        playlist: int = None,
        match_type: int = None,
        group_type: int = 0,
    ):
        identities, unknown_identities = parse_names(names)

        if unknown_identities:
            await ctx.send(f"Users not registered: {unknown_identities}", ephemeral=True)
            return
        
        print(
            f"calculating trend for {time_range},{names},{playlist},{match_type},{statistic}..."
        )
        message = await ctx.send("Roggo Stats is thinking...")
        try:
            result = await calculate_trend(
                request=bc.FilterRequest(
                    identities=[to_identity(name.strip()) for name in names.split(",")],
                    groupType=group_type,
                    playlist=playlist if playlist else bc.Playlist.ALL,
                    matchType=match_type if match_type else bc.MatchType.BOTH,
                    timeRange=time_range if time_range else bc.TimeRange.EVERY_TIME,
                ),
                statistic=Statistic(statistic),
            )
            await message.edit(
                content="",
                embed=create_trend_embed(result),
                file=result.image_path,
            )
            os.remove(result.image_path)
        except:
            await ctx.send(embed=create_error_embed())
