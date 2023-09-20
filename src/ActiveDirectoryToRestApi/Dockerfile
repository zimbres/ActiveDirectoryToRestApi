FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
RUN apt update && apt install -y --no-install-recommends libldap-2.4-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/ActiveDirectoryToRestApi/ActiveDirectoryToRestApi.csproj", "src/ActiveDirectoryToRestApi/"]
RUN dotnet restore "src/ActiveDirectoryToRestApi/ActiveDirectoryToRestApi.csproj"
COPY . .
WORKDIR "/src/src/ActiveDirectoryToRestApi"
RUN dotnet build "ActiveDirectoryToRestApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ActiveDirectoryToRestApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ActiveDirectoryToRestApi.dll"]