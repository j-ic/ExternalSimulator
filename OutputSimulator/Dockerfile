FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

# copy Execute File
COPY ./bin/Release/net8.0/linux-x64/publish/ /app/

# Set the entry point to your .exe file
ENTRYPOINT ["./OutputSimulator"]