FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
RUN apt-get update -y \
    && apt-get upgrade -y \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
RUN apt-get update -y \
    && apt-get upgrade -y \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /src
COPY ["WHMapper.csproj", "."]
RUN dotnet restore "WHMapper.csproj"
COPY . .
RUN dotnet build "WHMapper.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WHMapper.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WHMapper.dll"]