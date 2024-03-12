FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
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