FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/ComputingService/ComputingService.csproj", "src/ComputingService/"]
COPY ["src/NotificationService.Models/NotificationService.Models.csproj", "src/NotificationService.Models/"]
RUN dotnet restore "src/ComputingService/ComputingService.csproj"

COPY src/ComputingService/. ./src/ComputingService/
COPY src/NotificationService.Models/. ./src/NotificationService.Models/
COPY src/ComputingService/Program.cs ./src/ComputingService/
COPY src/ComputingService/appsettings.json ./src/ComputingService/

WORKDIR "/src/src/ComputingService"
RUN dotnet build "ComputingService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ComputingService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ComputingService.dll"]
