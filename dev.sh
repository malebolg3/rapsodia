#!/bin/bash
cd /home/ubuntu/th1er/malebolge/Rapsodia

ENV_FILE="../.env"
if [ -f ".env.ssh" ]; then
    ENV_FILE=".env.ssh"
    echo "🔧 Modo SSH (dotnet local)"
else
    echo "🚀 Modo Docker"
fi

set -a
source "$ENV_FILE"
set +a

dotnet watch run --project Rapsodia.Blue/Rapsodia.Blue.csproj --urls http://localhost:5073 &
PID1=$!
sleep 20

dotnet watch run --project Rapsodia.Red/Rapsodia.Red.csproj --urls http://localhost:5074 &
PID2=$!
sleep 20

dotnet watch run --project Rapsodia.csproj --urls http://localhost:5075 &
PID3=$!
sleep 20

dotnet watch run --project Rapsodia.Silver/Rapsodia.Silver.csproj --urls http://localhost:5076 &
PID4=$!

echo "Blue:   http://localhost:5073/swagger"
echo "Red:    http://localhost:5074/swagger"
echo "Violet: http://localhost:5075/swagger"
echo "Silver: http://localhost:5076/swagger"
echo "Ctrl+C para parar."

trap "kill $PID1 $PID2 $PID3 $PID4 2>/dev/null; exit" SIGINT SIGTERM
wait
