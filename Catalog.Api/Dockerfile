# Etapa 1: Compilar el proyecto usando el SDK completo
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["Catalog.Api/Catalog.Api.csproj", "Catalog.Api/"]
COPY ["Catalog.Application/Catalog.Application.csproj", "Catalog.Application/"]
COPY ["Catalog.Domain/Catalog.Domain.csproj", "Catalog.Domain/"]
COPY ["Catalog.Infrastructure/Catalog.Infrastructure.csproj", "Catalog.Infrastructure/"]
RUN dotnet restore "Catalog.Api/Catalog.Api.csproj"

# Copiar el resto del código y compilar
COPY . .
WORKDIR "/src/Catalog.Api"
RUN dotnet build "Catalog.Api.csproj" -c Release -o /app/build

# Etapa 2: Publicar el proyecto
FROM build AS publish
RUN dotnet publish "Catalog.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 3: Crear la imagen final usando solo el runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.Api.dll"]


