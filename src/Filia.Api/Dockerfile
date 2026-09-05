# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Packages.props", "."]
COPY ["src/Filia.Api/Filia.Api.csproj", "src/Filia.Api/"]
COPY ["src/Filia.Application/Filia.Application.csproj", "src/Filia.Application/"]
COPY ["src/Filia.Infrastructure/Filia.Infrastructure.csproj", "src/Filia.Infrastructure/"]
COPY ["src/Filia.Domain/Filia.Domain.csproj", "src/Filia.Domain/"]
COPY ["src/Filia.ServiceDefaults/Filia.ServiceDefaults.csproj", "src/Filia.ServiceDefaults/"]
RUN dotnet restore "src/Filia.Api/Filia.Api.csproj"

COPY . .
WORKDIR "/src/src/Filia.Api"
RUN dotnet build "Filia.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Filia.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Filia.Api.dll"]
