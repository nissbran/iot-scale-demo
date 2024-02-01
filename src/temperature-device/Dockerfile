FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /build
RUN wget https://dl.smallstep.com/cli/docs-cli-install/latest/step-cli_amd64.deb
COPY ["TemperatureDevice.csproj", "."]
RUN dotnet restore "TemperatureDevice.csproj"
COPY . .
RUN dotnet build "TemperatureDevice.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TemperatureDevice.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /build/step-cli_amd64.deb .
RUN dpkg -i step-cli_amd64.deb && \
    rm step-cli_amd64.deb
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TemperatureDevice.dll"]