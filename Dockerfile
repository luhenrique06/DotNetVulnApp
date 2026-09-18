# VaultBank - lab OWASP Top 10 2025 (intencionalmente vulnerável)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY brokenaccesscontrol.csproj .
RUN dotnet restore brokenaccesscontrol.csproj

COPY . .
RUN dotnet publish brokenaccesscontrol.csproj -c Release -o /app/publish /p:UseAppHost=false

# Regenera o banco SQLite a partir do seed.
RUN apt-get update && apt-get install -y --no-install-recommends sqlite3 \
    && mkdir -p /app/publish/Database \
    && sqlite3 /app/publish/Database/brokenaccesscontrol.db < Database/seed.sql

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# bash/ghostscript/enscript: necessários para o cenário de command injection (A05)
# e geração de PDF do extrato.
RUN apt-get update \
    && apt-get install -y --no-install-recommends bash ghostscript enscript \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
RUN mkdir -p /app/logs

ENV ASPNETCORE_URLS=http://+:8080
ENV SqliteDatabase=/app/Database/brokenaccesscontrol.db
ENV JWTSecret=fedaf7d8863b48e197b9287d492b708e
ENV ASPNETCORE_ENVIRONMENT=Teste

EXPOSE 8080
ENTRYPOINT ["dotnet", "brokenaccesscontrol.dll"]
