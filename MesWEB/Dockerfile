# ビルドステージ
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# ロケール設定を追加（日本語対応）
ENV LANG=ja_JP.UTF-8
ENV LC_ALL=ja_JP.UTF-8

# プロジェクトファイルをコピーして依存関係を復元
COPY ["CrystalGrowthNotebook2.csproj", "./"]
RUN dotnet restore "CrystalGrowthNotebook2.csproj"

# ソースコードをコピーしてビルド
COPY . .
RUN dotnet build "CrystalGrowthNotebook2.csproj" -c Release -o /app/build

# 発行ステージ
FROM build AS publish
RUN dotnet publish "CrystalGrowthNotebook2.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ランタイムステージ
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# 非rootユーザーで実行
RUN useradd -m -u 1000 appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

# 環境変数
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "CrystalGrowthNotebook2.dll"]
