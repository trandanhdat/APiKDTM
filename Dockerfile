# ============================
# Base image (runtime)
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Render sẽ dùng biến môi trường PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
EXPOSE 10000

# ============================
# Build image
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["APi/APi.csproj", "APi/"]
RUN dotnet restore "APi/APi.csproj"

COPY . .
WORKDIR "/src/APi"
RUN dotnet build "APi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ============================
# Publish
# ============================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "APi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ============================
# Final image
# ============================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Start ASP.NET Web API
ENTRYPOINT ["dotnet", "APi.dll"]
