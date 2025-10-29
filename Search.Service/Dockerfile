FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Search.Service/Search.Service.csproj", "Search.Service/"]
RUN dotnet restore "Search.Service/Search.Service.csproj"
COPY . .
WORKDIR "/src/Search.Service"
RUN dotnet build "Search.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Search.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Search.Service.dll"]