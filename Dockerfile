FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /CosmicWorks
COPY ["modeling-demos/modeling-demos.csproj", "modeling-demos/"]
COPY ["models/models.csproj", "models/"]
COPY ["cosmos-management/cosmos-management.csproj", "cosmos-management/"]
COPY ["CosmicWorks.sln", "."]
RUN dotnet restore "CosmicWorks.sln"
COPY . .
RUN dotnet build "CosmicWorks.sln" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "modeling-demos/modeling-demos.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "cosmos-management.dll"]