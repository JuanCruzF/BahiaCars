FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Media.Service/Media.Service.csproj", "Media.Service/"]
RUN dotnet restore "Media.Service/Media.Service.csproj"
COPY . .
WORKDIR "/src/Media.Service"
RUN dotnet build "Media.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Media.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Instalar ffmpeg
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

RUN mkdir -p /app/uploads && chmod 777 /app/uploads

ENTRYPOINT ["dotnet", "Media.Service.dll"]