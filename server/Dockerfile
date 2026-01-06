# Runtime image only - uses pre-published files from host
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY publish/ .

EXPOSE 8080
ENTRYPOINT ["dotnet", "server.dll"]
