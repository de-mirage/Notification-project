FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/SmsService/SmsService.csproj", "src/SmsService/"]
COPY ["src/NotificationService.Models/NotificationService.Models.csproj", "src/NotificationService.Models/"]
RUN dotnet restore "src/SmsService/SmsService.csproj"

COPY src/SmsService/. ./src/SmsService/
COPY src/NotificationService.Models/. ./src/NotificationService.Models/
COPY src/SmsService/appsettings.json ./src/SmsService/

WORKDIR "/src/src/SmsService"
RUN dotnet build "SmsService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmsService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmsService.dll"]
