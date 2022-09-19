#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
ENV TZ=America/Sao_Paulo
RUN cat /usr/share/zoneinfo/$TZ > /etc/localtime \
		&& cat /usr/share/zoneinfo/$TZ > /etc/timezone \
		&& update-ca-certificates \
        rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ./src/Cadastro.GRPC/Cadastro.GRPC.csproj ./Cadastro.GRPC/
COPY ./src/Cadastro.Domain/Cadastro.Domain.csproj ./Cadastro.Domain/
COPY ./src/Cadastro.Data/Cadastro.Data.csproj ./Cadastro.Data/
COPY ./src/Cadastro.Configuracoes/Cadastro.Configuracoes.csproj ./Cadastro.Configuracoes/

RUN dotnet restore ./Cadastro.GRPC/Cadastro.GRPC.csproj

COPY ./src/Cadastro.GRPC/ ./Cadastro.GRPC/
COPY ./src/Cadastro.Configuracoes/ ./Cadastro.Configuracoes/
COPY ./src/Cadastro.Domain/ ./Cadastro.Domain/
COPY ./src/Cadastro.Data/ ./Cadastro.Data/
RUN dotnet build ./Cadastro.GRPC/Cadastro.GRPC.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish ./Cadastro.GRPC/Cadastro.GRPC.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ./src/nginx/certificate/ /usr/local/share/ca-certificates/
RUN update-ca-certificates
ENTRYPOINT ["dotnet", "Cadastro.GRPC.dll"]
