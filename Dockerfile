# Dockerfile para SoftFocus Backend (.NET 9)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivo de proyecto y restaurar dependencias
COPY ["SoftFocusBackend/SoftFocusBackend.csproj", "SoftFocusBackend/"]
RUN dotnet restore "SoftFocusBackend/SoftFocusBackend.csproj"

# Copiar código fuente
COPY . .
WORKDIR "/src/SoftFocusBackend"

# Build del proyecto
RUN dotnet build "SoftFocusBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SoftFocusBackend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "SoftFocusBackend.dll"]