FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 9000
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY . .
RUN dotnet restore "BallchasingWrapper.csproj"
RUN dotnet build "BallchasingWrapper.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BallchasingWrapper.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

ARG APP_VERSION=dev
ENV APP_VERSION=$APP_VERSION
RUN echo "$APP_VERSION"

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BallchasingWrapper.dll"]
