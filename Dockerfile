FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ArgenCash.Api/ArgenCash.Api.csproj", "ArgenCash.Api/"]
COPY ["ArgenCash.Application/ArgenCash.Application.csproj", "ArgenCash.Application/"]
COPY ["ArgenCash.Domain/ArgenCash.Domain.csproj", "ArgenCash.Domain/"]
COPY ["ArgenCash.Infrastructure/ArgenCash.Infrastructure.csproj", "ArgenCash.Infrastructure/"]

RUN dotnet restore "ArgenCash.Api/ArgenCash.Api.csproj"

COPY . .
WORKDIR /src/ArgenCash.Api
RUN dotnet publish "ArgenCash.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ArgenCash.Api.dll"]
