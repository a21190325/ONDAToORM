FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ONDAToORMWebAPI/ONDAToORMWebAPI.csproj", "ONDAToORMWebAPI/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Services/Services.csproj", "Services/"]
RUN dotnet restore "./ONDAToORMWebAPI/ONDAToORMWebAPI.csproj"
COPY . .
RUN mkdir -p /app/build/Data
WORKDIR "/src/ONDAToORMWebAPI"
RUN dotnet build "./ONDAToORMWebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ONDAToORMWebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /app/Data
COPY --from=build /src/ONDAToORMWebAPI/Data/TempSQLiteDb.db /app/Data/

USER root
RUN chown app:app /app/Data/TempSQLiteDb.db

ENTRYPOINT ["dotnet", "ONDAToORMWebAPI.dll"]