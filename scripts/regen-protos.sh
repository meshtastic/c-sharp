cd ./protobufs/meshtastic/
mv deviceonly.proto deviceonly.ignoreproto
cd ../../

~/.nuget/packages/google.protobuf.tools/3.29.3/tools/macosx_x64/protoc -I=protobufs --csharp_out=./Meshtastic/Generated --csharp_opt=base_namespace=Meshtastic.Protobufs ./protobufs/meshtastic/*.proto

cd ./protobufs/meshtastic/
mv deviceonly.ignoreproto deviceonly.proto
cd ../../
