FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/NotificationGateway/NotificationGateway.csproj", "src/NotificationGateway/"]
COPY ["src/NotificationService.Models/NotificationService.Models.csproj", "src/NotificationService.Models/"]
RUN dotnet restore "src/NotificationGateway/NotificationGateway.csproj"

COPY src/NotificationGateway/. ./src/NotificationGateway/
COPY src/NotificationService.Models/. ./src/NotificationService.Models/
COPY src/NotificationGateway/Program.cs ./
COPY src/NotificationGateway/appsettings.json ./src/NotificationGateway/

WORKDIR "/src/src/NotificationGateway"
RUN dotnet build "NotificationGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationGateway.dll"]
