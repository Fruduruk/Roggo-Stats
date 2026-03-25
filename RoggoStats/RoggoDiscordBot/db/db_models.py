# db_models.py
import os

from sqlalchemy import ForeignKey, Integer, create_engine
from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column, relationship, sessionmaker


class Base(DeclarativeBase):
    pass


class User(Base):
    __tablename__ = "user"

    discord_id: Mapped[int] = mapped_column(Integer, primary_key=True)
    steam_id: Mapped[int] = mapped_column(Integer)
    # tracked_players: Mapped[list["TrackedPlayer"]] = relationship(
    #     back_populates="guild",
    #     cascade="all, delete-orphan",
    # )


# class TrackedPlayer(Base):
#     __tablename__ = "tracked_player"

#     id: Mapped[int] = mapped_column(primary_key=True, autoincrement=True)
#     guild_id: Mapped[str] = mapped_column(ForeignKey("guild_config.guild_id"))
#     player_name: Mapped[str] = mapped_column(String(100))
#     platform: Mapped[str] = mapped_column(String(30))

#     guild: Mapped["GuildConfig"] = relationship(
#         back_populates="tracked_players")

# os.remove("./data/state.db")
engine = create_engine("sqlite:///data/state.db")
SessionLocal = sessionmaker(bind=engine)


def init_db() -> None:
    Base.metadata.create_all(engine)
