import ballchasing_pb2


def to_name_identity(name: str):
    return ballchasing_pb2.Identity(identityType=ballchasing_pb2.NAME, nameOrId=name)
