# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore
COPY backend/backend.csproj backend/
RUN dotnet restore "backend/backend.csproj"

# Publish
COPY backend/ backend/
RUN dotnet publish "backend/backend.csproj" -c Release -o /app/publish

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5000

ENTRYPOINT ["dotnet", "backend.dll"]