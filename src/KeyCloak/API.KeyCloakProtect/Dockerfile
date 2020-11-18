
FROM mcr.microsoft.com/dotnet/core/sdk:3.1.302-buster  AS build
WORKDIR /src
COPY ./API.KeyCloakProtect.csproj .
RUN dotnet restore API.KeyCloakProtect.csproj
COPY . .
RUN dotnet build API.KeyCloakProtect.csproj -c Release -o /app/build
RUN dotnet publish API.KeyCloakProtect.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "API.KeyCloakProtect.dll"]