# Runtime image only - we'll publish on host
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy pre-published files from host
COPY publish-temp/ .

ENTRYPOINT ["dotnet", "server.dll"]
