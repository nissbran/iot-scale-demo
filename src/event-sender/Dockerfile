FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /build
COPY ["TemperatureDevice.csproj", "."]
RUN dotnet restore "TemperatureDevice.csproj"
COPY . .
RUN dotnet build "TemperatureDevice.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TemperatureDevice.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TemperatureDevice.dll"]