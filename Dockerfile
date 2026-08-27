FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["PqrsSaaS.sln*", "./"]
COPY ["src/Pqrs.Domain/Pqrs.Domain.csproj", "src/Pqrs.Domain/"]
COPY ["src/Pqrs.Application/Pqrs.Application.csproj", "src/Pqrs.Application/"]
COPY ["src/Pqrs.Infrastructure/Pqrs.Infrastructure.csproj", "src/Pqrs.Infrastructure/"]
COPY ["src/Pqrs.API/Pqrs.API.csproj", "src/Pqrs.API/"]

RUN dotnet restore "src/Pqrs.API/Pqrs.API.csproj"

COPY . .
WORKDIR "/src/src/Pqrs.API"
RUN dotnet build "Pqrs.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pqrs.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Pqrs.API.dll"]
