#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["03.Endpoints/TapsiDOC.Order.EndPoints.Api.V1/TapsiDOC.Order.EndPoints.Api.V1.csproj", "03.Endpoints/TapsiDOC.Order.EndPoints.Api.V1/"]
COPY ["01.Core/TapsiDOC.Order.Core.ApplicationService/TapsiDOC.Order.Core.ApplicationService.csproj", "01.Core/TapsiDOC.Order.Core.ApplicationService/"]
COPY ["01.Core/TapsiDOC.Order.Core.Domain/TapsiDOC.Order.Core.Domain.csproj", "01.Core/TapsiDOC.Order.Core.Domain/"]
COPY ["02.Infra/TapsiDOC.Order.Infra.Data.Sql.Command/TapsiDOC.Order.Infra.Data.Sql.Commands.csproj", "02.Infra/TapsiDOC.Order.Infra.Data.Sql.Command/"]
COPY ["02.Infra/TapsiDOC.Order.Infra.Data.Sql.Queries/TapsiDOC.Order.Infra.Data.Sql.Queries.csproj", "02.Infra/TapsiDOC.Order.Infra.Data.Sql.Queries/"]
COPY ["03.Endpoints/TapsiDOC.Order.EndPoints.Api.V1/nuget.config", "/src/"]
RUN dotnet restore "./03.Endpoints/TapsiDOC.Order.EndPoints.Api.V1/TapsiDOC.Order.EndPoints.Api.V1.csproj" -v normal --configfile /src/nuget.config
COPY . .
WORKDIR "/src/03.Endpoints/TapsiDOC.Order.EndPoints.Api.V1"
RUN dotnet build "./TapsiDOC.Order.EndPoints.Api.V1.csproj" -c $BUILD_CONFIGURATION -o /app/build -v normal --configfile /src/nuget.config

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TapsiDOC.Order.EndPoints.Api.V1.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TapsiDOC.Order.EndPoints.Api.V1.dll"]