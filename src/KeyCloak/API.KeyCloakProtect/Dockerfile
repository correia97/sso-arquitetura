#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/KeyCloak/API.KeyCloakProtect/API.KeyCloakProtect.csproj", "src/KeyCloak/API.KeyCloakProtect/"]
RUN dotnet restore "src/KeyCloak/API.KeyCloakProtect/API.KeyCloakProtect.csproj"
COPY . .
WORKDIR "/src/src/KeyCloak/API.KeyCloakProtect"
RUN dotnet build "API.KeyCloakProtect.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "API.KeyCloakProtect.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.KeyCloakProtect.dll"]
