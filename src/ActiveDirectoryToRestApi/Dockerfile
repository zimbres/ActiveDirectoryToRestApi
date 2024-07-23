FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/ActiveDirectoryToRestApi/ActiveDirectoryToRestApi.csproj", "src/ActiveDirectoryToRestApi/"]
RUN dotnet restore "./src/ActiveDirectoryToRestApi/./ActiveDirectoryToRestApi.csproj"
COPY . .

ARG TARGETPLATFORM

RUN if [ "$TARGETPLATFORM" = "linux/amd64" ]; then \
    RID=linux-x64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm64" ]; then \
    RID=linux-arm64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm/v7" ]; then \
    RID=linux-arm ; \
    fi \
    && dotnet publish "src/ActiveDirectoryToRestApi/ActiveDirectoryToRestApi.csproj" -c Release -o /app/publish -r $RID --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
RUN apt update
RUN apt install -y libldap-2.5-0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["./ActiveDirectoryToRestApi"]