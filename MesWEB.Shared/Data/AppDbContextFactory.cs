using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MesWEB.Shared.Data
{
    // This helper intentionally does NOT implement IDesignTimeDbContextFactory to avoid
    // conflicting design-time factories being discovered by the EF tools when multiple
    // projects reference the same AppDbContext type.
    internal static class SharedAppDbContextFactoryHelper
    {
        public static DbContextOptions<AppDbContext> CreateOptions()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            var conn = Environment.GetEnvironmentVariable("SQLSERVER_CONNECTIONSTRING")
                ?? config.GetConnectionString("Default")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default");

            if (string.IsNullOrWhiteSpace(conn))
            {
                throw new InvalidOperationException("環境変数 'SQLSERVER_CONNECTIONSTRING' または appsettings.json の ConnectionStrings:Default を設定してください。");
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(conn, sqlOptions => sqlOptions.EnableRetryOnFailure());

            return optionsBuilder.Options;
        }
    }
}
