FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build-env
WORKDIR /src
EXPOSE 80

COPY BalloonSuite.CustomerExport/bin/Release/netcoreapp3.1 BalloonSuite.CustomerExport/

WORKDIR /src/BalloonSuite.CustomerExport

ENTRYPOINT ["dotnet", "BalloonSuite.CustomerExport.dll"]
