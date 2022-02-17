#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM  mcr.microsoft.com/dotnet/aspnet:6.0 AS base
ENV TZ=America/Sao_Paulo
RUN cat /usr/share/zoneinfo/$TZ > /etc/localtime \
		&& cat /usr/share/zoneinfo/$TZ > /etc/timezone \
		&& update-ca-certificates
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ./Cadastro.MVC.csproj MVC/
RUN dotnet restore ./MVC/Cadastro.MVC.csproj
COPY . MVC/
RUN dotnet build ./MVC/Cadastro.MVC.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish ./MVC/Cadastro.MVC.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cadastro.MVC.dll"]