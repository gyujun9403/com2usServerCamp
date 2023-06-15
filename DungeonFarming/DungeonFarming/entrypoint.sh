#!/bin/bash
set -e

echo "Delay for DB server readiness"
sleep 30
echo "API Server Start"

dotnet DungeonFarming.dll