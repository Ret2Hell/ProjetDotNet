# Use a smaller base image if possible
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only the project file and restore dependencies
COPY ["ProjetDotNet.csproj", "./"]
RUN dotnet restore "ProjetDotNet.csproj"

# Copy the rest of the files and build
COPY . .
RUN dotnet build "ProjetDotNet.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ProjetDotNet.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProjetDotNet.dll"]