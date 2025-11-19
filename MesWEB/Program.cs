using MesWEB.Components;
using MesWEB.Data;
using MesWEB.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Register code pages encoding provider so ExcelDataReader can read legacy .xls encodings
System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// -----------------------------------------------------------------------------
// Configuration strategy
// -----------------------------------------------------------------------------
// This application intentionally uses a single configuration file: `appsettings.json`.
// Environment-specific files (e.g. appsettings.Development.json) are NOT used.
// We clear default configuration providers and load only appsettings.json so
// behavior is deterministic across environments and when running via Docker/Compose.
// Note: environment variables will still be available via builder.Configuration if
// they are present, but we intentionally do not load additional appsettings.*.json files.
// ----------------------------------------------------------------------------

// appsettings.json のみを使用（環境別ファイルは使用しない）
builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add environment variables last so they can still override settings if needed (optional)
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        // WebSocket接続のタイムアウトを延長
        options.DisconnectedCircuitMaxRetained = 100;
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
        options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
        options.MaxBufferedUnacknowledgedRenderBatches = 10;
    });

// DeviceDetectionServiceをスコープドサービスとして登録（セッションごとに独立）
builder.Services.AddScoped<DeviceDetectionService>();

// 接続文字列を appsettings.jsonから取得
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("接続文字列 'Default' が appsettings.json に設定されていません。");

// Read flag to control whether automatic EF Core migrations should run on startup
// Default: false (disable automatic migrations). Set environment variable ApplyMigrations=true to enable.
var applyMigrations = builder.Configuration.GetValue<bool>("ApplyMigrations", false);

Console.WriteLine($"=== Database Configuration ===");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Configuration sources loaded: {string.Join(',', builder.Configuration.Sources.Select(s => s.GetType().Name))}");
Console.WriteLine($"Using configuration file: appsettings.json");
Console.WriteLine($"Connection String: {connectionString}");
Console.WriteLine($"ApplyMigrations: {applyMigrations}");
Console.WriteLine($"==============================");

// DbContextFactoryを追加（Blazor Serverの同時実行問題を解決）
builder.Services.AddDbContextFactory<AppDbContext>(options =>
  options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure())
);

var app = builder.Build();

// 環境情報をログ出力
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("=== Environment Information ===");
startupLogger.LogInformation("Environment Name: {EnvironmentName}", app.Environment.EnvironmentName);
startupLogger.LogInformation("Is Development: {IsDevelopment}", app.Environment.IsDevelopment());
startupLogger.LogInformation("Is Production: {IsProduction}", app.Environment.IsProduction());
startupLogger.LogInformation("Content Root: {ContentRoot}", app.Environment.ContentRootPath);
startupLogger.LogInformation("Web Root: {WebRoot}", app.Environment.WebRootPath);
startupLogger.LogInformation("Configuration Sources: {Sources}", string.Join(',', builder.Configuration.Sources.Select(s => s.GetType().Name)));
startupLogger.LogInformation("Using configuration: appsettings.json");
startupLogger.LogInformation("ApplyMigrations: {ApplyMigrations}", applyMigrations);
startupLogger.LogInformation("===============================");

// Apply pending migrations at startup (async with timeout) if enabled
if (applyMigrations)
{
    using (var scope = app.Services.CreateScope())
    {
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("データベース接続を確認しています...");

            await using var db = await dbFactory.CreateDbContextAsync();

            // データベース接続テスト（タイムアウト10秒）
            var canConnect = await Task.Run(async () =>
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                return await db.Database.CanConnectAsync(cts.Token);
            });

            if (!canConnect)
            {
                logger.LogWarning("データベースに接続できません。マイグレーションをスキップします。");
            }
            else
            {
                logger.LogInformation("SQL Serverマイグレーションを適用しています...");

                // マイグレーション実行（タイムアウト60秒）
                await Task.Run(async () =>
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                    await db.Database.MigrateAsync(cts.Token);
                });

                logger.LogInformation("SQL Serverマイグレーションの適用が完了しました");
            }
        }
        catch (TaskCanceledException)
        {
            logger.LogWarning("データベース初期化がタイムアウトしました。アプリケーションを起動しますが、データベース機能は利用できない可能性があります。");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "データベースの初期化中にエラーが発生しました。アプリケーションは起動しますが、データベース機能は利用できない可能性があります。");
        }
    }
}
else
{
    startupLogger.LogInformation("ApplyMigrations is disabled; skipping automatic EF Core migrations on startup.");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Productionでも詳細エラー表示（デバッグ用）
    app.UseDeveloperExceptionPage();

    // 本番環境では以下に切り替え
    // app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
