#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Cadastro.GRPC/Cadastro.GRPC.csproj", "src/Cadastro.GRPC/"]
RUN dotnet restore "src/Cadastro.GRPC/Cadastro.GRPC.csproj"
COPY . .
WORKDIR "/src/src/Cadastro.GRPC"
RUN dotnet build "Cadastro.GRPC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cadastro.GRPC.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cadastro.GRPC.dll"]