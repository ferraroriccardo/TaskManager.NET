# Multi-stage build: si compila con l'immagine SDK (grande) e si esegue con quella runtime (piccola).

# ---- Stage 1: build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Prima solo il .csproj + restore: Docker mette in cache questo layer,
# quindi i pacchetti NuGet si riscaricano solo se cambiano le dipendenze.
COPY TaskManager.Api.csproj .
RUN dotnet restore

# Poi il resto del codice
COPY . .
RUN dotnet publish -c Release -o /app --no-restore

# ---- Stage 2: runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskManager.Api.dll"]
