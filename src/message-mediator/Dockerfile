FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MessageMediator.csproj", "."]
RUN dotnet restore "./MessageMediator.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "MessageMediator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MessageMediator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MessageMediator.dll"]