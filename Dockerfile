FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build-env
WORKDIR /src
EXPOSE 80

COPY BalloonSuite.CustomerExport/bin/Debug/netcoreapp3.1/publish BalloonSuite.CustomerExport/

WORKDIR /src/BalloonSuite.CustomerExport

ENTRYPOINT ["dotnet", "BalloonSuite.CustomerExport.dll"]
