FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Auth.Service/Auth.Service.csproj", "Auth.Service/"]
RUN dotnet restore "Auth.Service/Auth.Service.csproj"
COPY . .
WORKDIR "/src/Auth.Service"
RUN dotnet build "Auth.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auth.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.Service.dll"]