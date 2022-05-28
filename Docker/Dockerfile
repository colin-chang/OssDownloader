FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine

COPY publish/. /app
WORKDIR /app

ENTRYPOINT ["dotnet", "ColinChang.OssDownloader.dll"]