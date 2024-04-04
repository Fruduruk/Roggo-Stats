from typing import List

import grpc
import ballchasing_pb2 as bc
import ballchasing_pb2_grpc as bc_grpc
from config import BALLCHASING_HOST, BALLCHASING_PORT

print("loading grpc client...")


def create_stub() -> bc_grpc.BallchasingStub:
    channel = grpc.insecure_channel(BALLCHASING_HOST + ':' + BALLCHASING_PORT)
    return bc_grpc.BallchasingStub(channel)


async def get_simple_replays(request: bc.FilterRequest) -> List[bc.Replay]:
    stub = create_stub()

    try:
        response = stub.GetSimpleReplays(request)
        return response.replays
    except grpc.RpcError as e:
        print(f"Ein Fehler ist aufgetreten: {e}")


async def get_advanced_replays(request: bc.FilterRequest) -> List[bc.AdvancedPlayer]:
    stub = create_stub()

    try:
        response = stub.GetAdvancedReplays(request)
        return response.replays
    except grpc.RpcError as e:
        print(f"Ein Fehler ist aufgetreten: {e}")
