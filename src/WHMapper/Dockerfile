# Use the official ASP.NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443



# Use the official .NET SDK as a parent image for building the application
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src
COPY ["WHMapper.csproj", "."]
RUN dotnet restore "WHMapper.csproj"
COPY . .
RUN dotnet build "WHMapper.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
ARG VERSION=0.0.0
RUN dotnet publish "WHMapper.csproj" -c Release -o /app/publish /p:Version=$VERSION

# Final stage: use the runtime image to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN rm -rf /src /app/build /app/publish && \
    addgroup -S appgroup && adduser -S appuser -G appgroup && \
    chown -R appuser:appgroup /app && \
    chmod -R u+rw /app
USER appuser

ENTRYPOINT ["dotnet", "WHMapper.dll"]