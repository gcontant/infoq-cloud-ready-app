FROM mcr.microsoft.com/dotnet/sdk:7.0 AS base
WORKDIR /app
COPY . .

RUN dotnet restore Notifications.csproj

RUN dotnet publish \
    --configuration Release \
    --self-contained false \
    --runtime linux-x64 \
    --output /app/publish \
    Notifications.csproj

FROM mcr.microsoft.com/dotnet/aspnet:7.0 as final
WORKDIR /app
COPY --from=base /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Notifications.dll"]