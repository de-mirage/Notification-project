FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/PushService/PushService.csproj", "src/PushService/"]
COPY ["src/NotificationService.Models/NotificationService.Models.csproj", "src/NotificationService.Models/"]
RUN dotnet restore "src/PushService/PushService.csproj"

COPY src/PushService/. ./src/PushService/
COPY src/NotificationService.Models/. ./src/NotificationService.Models/
COPY src/PushService/appsettings.json ./src/PushService/

WORKDIR "/src/src/PushService"
RUN dotnet build "PushService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PushService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PushService.dll"]
