# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY rating-service.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "rating-service.dll"]