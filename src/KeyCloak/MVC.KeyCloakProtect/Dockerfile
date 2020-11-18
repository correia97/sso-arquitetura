
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY MVC.KeyCloakProtect.csproj .
RUN dotnet restore MVC.KeyCloakProtect.csproj
COPY . .
RUN dotnet build "MVC.KeyCloakProtect.csproj" -c Release -o /app/build
RUN dotnet publish "MVC.KeyCloakProtect.csproj" -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "MVC.KeyCloakProtect.dll"]