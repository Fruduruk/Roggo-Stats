from typing import List
import grpc
import ballchasing_pb2
import ballchasing_pb2_grpc
from config import BALLCHASING_HOST, BALLCHASING_PORT


def create_stub() -> ballchasing_pb2_grpc.BallchasingStub:
    channel = grpc.insecure_channel(BALLCHASING_HOST + ':' + BALLCHASING_PORT)
    return ballchasing_pb2_grpc.BallchasingStub(channel)


async def get_advanced_replays(identities: List[ballchasing_pb2.Identity]) -> None:
    stub = create_stub()
    request = ballchasing_pb2.FilterRequest(
        replayCap=20,
        identities=identities,
        groupType=ballchasing_pb2.TOGETHER,
        playlist=ballchasing_pb2.DOUBLES,
        matchType=ballchasing_pb2.RANKED
    )

    try:
        response = stub.GetAdvancedReplays(request)
        return response.replays
    except grpc.RpcError as e:
        print(f"Ein Fehler ist aufgetreten: {e}")
