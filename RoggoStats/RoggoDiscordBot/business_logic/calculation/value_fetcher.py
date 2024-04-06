from typing import Optional

import ballchasing_pb2 as bc
from models.statistic import Statistic


def calculate_boost_collected_small_to_big_ratio(advanced_player: bc.AdvancedPlayer) -> float:
    if advanced_player.stats.boost.generalBoost.countCollectedBig == 0:
        return advanced_player.stats.boost.generalBoost.countCollectedSmall
    return (advanced_player.stats.boost.generalBoost.countCollectedSmall /
            advanced_player.stats.boost.generalBoost.countCollectedBig)


def get_value(advanced_player: bc.AdvancedPlayer, statistic: Statistic) -> float:
    match statistic:
        case Statistic.BOOST_USED_PER_MINUTE:
            return advanced_player.stats.boost.generalBoost.bpm
        case Statistic.BOOST_COLLECTED_PER_MINUTE:
            return advanced_player.stats.boost.generalBoost.bcpm
        case Statistic.BOOST_AMOUNT_STOLEN:
            return advanced_player.stats.boost.generalBoost.amountStolen
        case Statistic.BOOST_AMOUNT_USED_WHILE_SUPERSONIC:
            return advanced_player.stats.boost.generalBoost.amountUsedWhileSupersonic
        case Statistic.BOOST_COLLECTED_SMALL_TO_BIG_RATIO:
            return calculate_boost_collected_small_to_big_ratio(advanced_player)
        case Statistic.PERCENT_SLOW_SPEED:
            return advanced_player.stats.movement.percentSlowSpeed
        case Statistic.PERCENT_SUPERSONIC_SPEED:
            return advanced_player.stats.movement.percentSupersonicSpeed
        case Statistic.AVERAGE_SPEED:
            return advanced_player.stats.movement.avgSpeed
        case Statistic.PERCENT_HIGH_AIR:
            return advanced_player.stats.movement.percentHighAir
        case Statistic.TIME_POWERSLIDE:
            return advanced_player.stats.movement.generalMovement.timePowerslide
        case Statistic.COUNT_POWERSLIDE:
            return advanced_player.stats.movement.generalMovement.countPowerslide
        case Statistic.AVERAGE_DISTANCE_TO_BALL:
            return advanced_player.stats.positioning.avgDistanceToBall
        case Statistic.AVERAGE_DISTANCE_TO_MATES:
            return advanced_player.stats.positioning.avgDistanceToMates
        case Statistic.PERCENT_CLOSEST_TO_BALL:
            return advanced_player.stats.positioning.percentClosestToBall
        case Statistic.PERCENT_FARTHEST_FROM_BALL:
            return advanced_player.stats.positioning.percentFarthestFromBall
        case Statistic.PERCENT_MOST_BACK:
            return advanced_player.stats.positioning.percentMostBack
        case Statistic.PERCENT_MOST_FORWARD:
            return advanced_player.stats.positioning.percentMostForward
        case Statistic.GOALS_AGAINST_WHILE_LAST_DEFENDER:
            return advanced_player.stats.positioning.goalsAgainstWhileLastDefender
        case Statistic.DEMOS_INFLICTED:
            return advanced_player.stats.demo.inflicted
        case Statistic.DEMOS_TAKEN:
            return advanced_player.stats.demo.taken
