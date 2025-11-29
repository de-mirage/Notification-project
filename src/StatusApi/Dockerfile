FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/StatusApi/StatusApi.csproj", "src/StatusApi/"]
COPY ["src/NotificationService.Models/NotificationService.Models.csproj", "src/NotificationService.Models/"]
RUN dotnet restore "src/StatusApi/StatusApi.csproj"

COPY . .
WORKDIR "/src/src/StatusApi"
RUN dotnet build "StatusApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StatusApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StatusApi.dll"]
