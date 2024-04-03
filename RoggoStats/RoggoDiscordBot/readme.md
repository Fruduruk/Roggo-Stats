# Protobuf tutorial

First run ``pip install grpc-tools`` to install the required protobuf generation tools.

Then run ``python -m grpc_tools.protoc -I./Protos --python_out=. --grpc_python_out=. ./Protos/ballchasing.proto``
to generate the files ["ballchasing_pb2.py", "ballchasing_pb2_grpc.py"].

