FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 9090

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MessageRouter.csproj", "."]
RUN dotnet restore "./MessageRouter.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "MessageRouter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MessageRouter.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MessageRouter.dll"]