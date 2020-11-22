
FROM mcr.microsoft.com/dotnet/sdk:3.1-buster AS build
WORKDIR /src
COPY ./API.KeyCloakProtect.csproj .
RUN dotnet restore API.KeyCloakProtect.csproj
COPY . .
RUN dotnet build API.KeyCloakProtect.csproj -c Release -o /app/build
RUN dotnet publish API.KeyCloakProtect.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:3.1-buster-slim AS base
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "API.KeyCloakProtect.dll"]