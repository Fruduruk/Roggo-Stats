from interactions import (
    Embed,
    Extension,
    slash_command,
    SlashContext,
    slash_option,
    OptionType,
)
from numpy import int64

from db.storage import register_user, unregister_user

print("loading register extension")


class Register(Extension):
    @slash_command(
        name="register",
        description="Registriere eine SteamId für deinen Discord Usernamen. ",
    )
    @slash_option(
        name="steam_id",
        description="Deine SteamId",
        required=True,
        opt_type=OptionType.STRING,
    )
    async def register(
        self,
        ctx: SlashContext,
        steam_id: str
    ):
        if not steam_id.isdigit():
            await ctx.send(f"Der angegebene Wert ist keine SteamId: {steam_id}", ephemeral=True)
            return
        
        print(
            f"registering SteamId {steam_id} for user {ctx.user.display_name} with discord id {ctx.user.id}"
        )
        register_user(ctx.user.id, steam_id)
        await ctx.send(f"Deine SteamId ist jetzt für diesen Bot verknüpft.\nhttps://ballchasing.com/player/steam/{steam_id}", ephemeral=True)

    @slash_command(
        name="unregister",
        description="Entferne die Verknüpfung zu deiner SteamId",
    )
    async def unregister(
        self,
        ctx: SlashContext
    ):
        print(
            f"unregistering user {ctx.user.display_name}"
        )
        unregister_user(ctx.user.id)
        await ctx.send("Deine SteamId wurde entfernt", ephemeral=True)
