FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /app

# copy Execute File
COPY OutputSimulator/bin/Debug/net8.0 /app

# Set the entry point to your .exe file
ENTRYPOINT ["OutputSimulator.exe"]