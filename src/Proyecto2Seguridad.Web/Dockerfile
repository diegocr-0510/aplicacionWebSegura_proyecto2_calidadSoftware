# Imagen base para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Imagen para compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivo del proyecto y restaurar dependencias
COPY ["Proyecto2Seguridad.Web.csproj", "./"]
RUN dotnet restore "Proyecto2Seguridad.Web.csproj"

# Copiar el resto del código
COPY . .

# Compilar en modo Release
RUN dotnet publish "Proyecto2Seguridad.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Ejecutar la aplicación
ENTRYPOINT ["dotnet", "Proyecto2Seguridad.Web.dll"]