from sqlalchemy import select
from sqlalchemy.exc import IntegrityError

from db.db_models import User, SessionLocal


# def get_all_names() -> list[User]:
#     with SessionLocal() as session:
#         statement = select(User)
#         return list(session.scalars(statement).all())


def register_user(discord_id: int, steam_id: int) -> bool:
    with SessionLocal() as session:
        entry = session.get(User, discord_id)

        if entry is None:
            entry = User(
                discord_id=discord_id,
                steam_id=steam_id
            )
            session.add(entry)
        else:
            entry.steam_id = steam_id
        try:
            session.commit()
            return True
        except IntegrityError:
            print("gestorben")
            session.rollback()
            return False


def unregister_user(discord_id: int) -> bool:
    with SessionLocal() as session:
        entry = session.get(User, discord_id)

        if entry is not None:
            session.delete(entry)
        try:
            session.commit()
            return True
        except IntegrityError:
            print("gestorben")
            session.rollback()
            return False

    # def ensure_guild_config(self, guild_id: str) -> None:
    #     with SessionLocal() as session:
    #         config = session.get(GuildConfig, guild_id)
    #         if config is None:
    #             session.add(GuildConfig(guild_id=guild_id))
    #             session.commit()

    # def set_prefix(self, guild_id: str, prefix: str) -> None:
    #     with SessionLocal() as session:
    #         config = session.get(GuildConfig, guild_id)
    #         if config is None:
    #             config = GuildConfig(guild_id=guild_id, prefix=prefix)
    #             session.add(config)
    #         else:
    #             config.prefix = prefix
    #         session.commit()

    # def add_tracked_player(self, guild_id: str, player_name: str, platform: str) -> None:
    #     with SessionLocal() as session:
    #         config = session.get(GuildConfig, guild_id)
    #         if config is None:
    #             config = GuildConfig(guild_id=guild_id)
    #             session.add(config)
    #             session.flush()

    #         session.add(
    #             TrackedPlayer(
    #                 guild_id=guild_id,
    #                 player_name=player_name,
    #                 platform=platform,
    #             )
    #         )
    #         session.commit()

    # def get_tracked_players(self, guild_id: str) -> list[TrackedPlayer]:
    #     with SessionLocal() as session:
    #         config = session.get(GuildConfig, guild_id)
    #         if config is None:
    #             return []
    #         return list(config.tracked_players)
