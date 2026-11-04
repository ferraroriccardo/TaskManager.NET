# Multi-stage build: compile with the SDK image (large) and run with the runtime image (small).

# ---- Stage 1: build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# First only the .csproj + restore: Docker caches this layer,
# so NuGet packages are re-downloaded only when the dependencies change.
COPY TaskManager.Api.csproj .
RUN dotnet restore

# Then the rest of the code
COPY . .
RUN dotnet publish -c Release -o /app --no-restore

# ---- Stage 2: runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskManager.Api.dll"]
