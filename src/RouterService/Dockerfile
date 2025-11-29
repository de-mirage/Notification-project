FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/RouterService/RouterService.csproj", "src/RouterService/"]
COPY ["src/NotificationService.Models/NotificationService.Models.csproj", "src/NotificationService.Models/"]
RUN dotnet restore "src/RouterService/RouterService.csproj"

COPY . .
WORKDIR "/src/src/RouterService"
RUN dotnet build "RouterService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RouterService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RouterService.dll"]
