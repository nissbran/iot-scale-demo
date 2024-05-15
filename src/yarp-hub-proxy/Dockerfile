FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 9090

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["YarpHubProxy.csproj", "."]
RUN dotnet restore "./YarpHubProxy.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "YarpHubProxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YarpHubProxy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YarpHubProxy.dll"]