

from interactions import Extension, OptionType, SlashContext, slash_command, slash_option
import ballchasing_pb2 as bc

from business_logic.calculation.trend_calculator import calculate_trend
from business_logic.embedder import create_error_embed, create_trend_embed
from business_logic.parser import parse_names
from business_logic.slash_command_choices import GROUP_TYPE_CHOICES, MATCH_TYPE_CHOICES, PLAYLIST_CHOICES
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
    async def summary(
        self,
        ctx: SlashContext,
        names: str,
        playlist: int = None,
        match_type: int = None,
        group_type: int = 0,
    ):
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
            timeRange= bc.TimeRange.YESTERDAY,
        )
        STATISTICS = [
            Statistic.AVERAGE_SPEED,
            Statistic.AVERAGE_DISTANCE_TO_BALL,
            Statistic.AVERAGE_DISTANCE_TO_MATES,
            Statistic.BOOST_AMOUNT_STOLEN
        ]

        results = []

        for statistic in STATISTICS:
            result = await calculate_trend(
                request=request,
                statistic=statistic
            )
            results.append(result)
        try:
            await message.edit(
                content="",
                embed=create_trend_embed(results[0]),
                files=[result.image_path for result in results]
            )
        except:
            await ctx.send(embed=create_error_embed())
        

