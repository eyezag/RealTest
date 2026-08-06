# Build context is the repository root (see docker-compose.yml: context: .)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/backend/RealTest.sln ./
COPY src/backend/RealTest.Api/RealTest.Api.csproj RealTest.Api/
COPY src/backend/RealTest.Application/RealTest.Application.csproj RealTest.Application/
COPY src/backend/RealTest.Domain/RealTest.Domain.csproj RealTest.Domain/
COPY src/backend/RealTest.Infrastructure/RealTest.Infrastructure.csproj RealTest.Infrastructure/
COPY src/backend/RealTest.Contracts/RealTest.Contracts.csproj RealTest.Contracts/
RUN dotnet restore RealTest.Api/RealTest.Api.csproj

COPY src/backend/ .
RUN dotnet publish RealTest.Api/RealTest.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RealTest.Api.dll"]
