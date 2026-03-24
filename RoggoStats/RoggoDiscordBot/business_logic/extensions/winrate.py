import ballchasing_pb2 as bc
from business_logic.calculation.winrate_calculator import calculate_winrate
from business_logic.calculation.weekday_winrate_calculator import (
    calculate_weekday_winrate,
)
import business_logic.embedder as embedder

from interactions import (
    Extension,
    slash_command,
    SlashContext,
    slash_option,
    OptionType,
)

from business_logic.slash_command_choices import MATCH_TYPE_CHOICES, PLAYLIST_CHOICES, TIME_CHOICES
from business_logic.grpc.grpc_helper_functions import to_identity

print("loading winrate extension...")


class Winrate(Extension):
    @slash_command(
        name="winrate", description="Erhalte die Winrate für gegebene Spieler"
    )
    @slash_option(
        name="time_range",
        description="Wähle ein Zeitintervall",
        required=True,
        opt_type=OptionType.INTEGER,
        choices=TIME_CHOICES,
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
        name="cap",
        description="Wähle die maximale Anzahl",
        required=False,
        opt_type=OptionType.INTEGER,
    )
    async def winrate(
        self,
        ctx: SlashContext,
        time_range: int,
        names: str,
        playlist: int = None,
        match_type: int = None,
        cap: int = None,
    ):
        print(
            f"calculating winrate for {time_range},{names},{playlist},{match_type},{cap}..."
        )
        message = await ctx.send("Roggo Stats is thinking...")
        try:
            await message.edit(
                content="",
                embed=embedder.create_winrate_embed(
                    winrate_result=await calculate_winrate(
                        request=bc.FilterRequest(
                            replayCap=cap,
                            identities=[
                                to_identity(name.strip()) for name in names.split(",")
                            ],
                            groupType=bc.TOGETHER,
                            playlist=playlist if playlist else bc.Playlist.ALL,
                            matchType=match_type if match_type else bc.MatchType.BOTH,
                            timeRange=(
                                time_range if time_range else bc.TimeRange.EVERY_TIME
                            ),
                        )
                    )
                ),
            )
        except:
            await ctx.send(embed=embedder.create_error_embed())

    @slash_command(
        name="weekday_winrate",
        description="Erhalte die Winrate für gegebene Spieler pro Wochentag",
    )
    @slash_option(
        name="time_range",
        description="Wähle ein Zeitintervall",
        required=True,
        opt_type=OptionType.INTEGER,
        choices=TIME_CHOICES,
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
        name="cap",
        description="Wähle die maximale Anzahl",
        required=False,
        opt_type=OptionType.INTEGER,
    )
    async def weekday_winrate(
        self,
        ctx: SlashContext,
        time_range: int,
        names: str,
        playlist: int = None,
        match_type: int = None,
        cap: int = None,
    ):
        print(
            f"calculating weekday winrate for {time_range},{names},{playlist},{match_type},{cap}..."
        )
        message = await ctx.send("Roggo Stats is thinking...")
        try:
            result = await calculate_weekday_winrate(
                request=bc.FilterRequest(
                    replayCap=cap,
                    identities=[to_identity(name.strip()) for name in names.split(",")],
                    groupType=bc.TOGETHER,
                    playlist=playlist if playlist else bc.Playlist.ALL,
                    matchType=match_type if match_type else bc.MatchType.BOTH,
                    timeRange=(time_range if time_range else bc.TimeRange.EVERY_TIME),
                )
            )

            await message.edit(
                content="",
                embed=embedder.create_weekday_winrate_embed(result),
                file=result.image_path,
            )
        except:
            await ctx.send(embed=embedder.create_error_embed())
