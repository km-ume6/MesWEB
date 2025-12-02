using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MesWEB.Data
{
    // Keep a single design-time factory in the startup project to avoid duplicate discovery.
    public class AppDbContextFactory : IDesignTimeDbContextFactory<MesWEB.Shared.Data.AppDbContext>
    {
        public MesWEB.Shared.Data.AppDbContext CreateDbContext(string[] args)
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

            var optionsBuilder = new DbContextOptionsBuilder<MesWEB.Shared.Data.AppDbContext>();
            optionsBuilder.UseSqlServer(conn, sqlOptions => sqlOptions.EnableRetryOnFailure());

            return new MesWEB.Shared.Data.AppDbContext(optionsBuilder.Options);
        }
    }
}
