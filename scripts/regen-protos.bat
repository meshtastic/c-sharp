cd ./protobufs/meshtastic/
rename deviceonly.proto deviceonly.ignoreproto
cd ../../

%USERPROFILE%/.nuget/packages/google.protobuf.tools/3.29.3/tools/windows_x64/protoc.exe -I=protobufs --csharp_out=./Meshtastic/Generated --csharp_opt=base_namespace=Meshtastic.Protobufs ./protobufs/meshtastic/*.proto

cd ./protobufs/meshtastic/
rename deviceonly.ignoreproto deviceonly.proto
cd ../../
