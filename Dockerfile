FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/AtlasIvrChat.Api/AtlasIvrChat.Api.csproj", "src/AtlasIvrChat.Api/"]
COPY ["src/AtlasIvrChat.Infrastructure/AtlasIvrChat.Infrastructure.csproj", "src/AtlasIvrChat.Infrastructure/"]
COPY ["src/AtlasIvrChat.Domain/AtlasIvrChat.Domain.csproj", "src/AtlasIvrChat.Domain/"]
RUN dotnet restore "src/AtlasIvrChat.Api/AtlasIvrChat.Api.csproj"

COPY . .
WORKDIR "/src/src/AtlasIvrChat.Api"

FROM build AS publish
RUN dotnet publish "AtlasIvrChat.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AtlasIvrChat.Api.dll"]