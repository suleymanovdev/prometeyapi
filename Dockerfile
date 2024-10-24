FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5205

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["prometeyapi.WebApi/prometeyapi.WebApi.csproj", "prometeyapi.WebApi/"]
COPY ["prometeyapi.Infrastructure/prometeyapi.Infrastructure.csproj", "prometeyapi.Infrastructure/"]
COPY ["prometeyapi.Core/prometeyapi.Core.csproj", "prometeyapi.Core/"]

RUN dotnet restore "prometeyapi.WebApi/prometeyapi.WebApi.csproj"
COPY . .
WORKDIR "/src/prometeyapi.WebApi"
RUN dotnet build "prometeyapi.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "prometeyapi.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "prometeyapi.WebApi.dll"]
