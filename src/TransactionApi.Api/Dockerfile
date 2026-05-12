FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/TransactionApi.Api/TransactionApi.Api.csproj", "src/TransactionApi.Api/"]
COPY ["src/TransactionApi.Application/TransactionApi.Application.csproj", "src/TransactionApi.Application/"]
COPY ["src/TransactionApi.Domain/TransactionApi.Domain.csproj", "src/TransactionApi.Domain/"]
COPY ["src/TransactionApi.Infrastructure/TransactionApi.Infrastructure.csproj", "src/TransactionApi.Infrastructure/"]
RUN dotnet restore "src/TransactionApi.Api/TransactionApi.Api.csproj"
COPY . .
WORKDIR "/src/src/TransactionApi.Api"
RUN dotnet build "TransactionApi.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TransactionApi.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TransactionApi.Api.dll"]
