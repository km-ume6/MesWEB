using Microsoft.EntityFrameworkCore;

namespace MesWEB.Shared.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<GrowthNoteItem> GrowthNotes { get; set; } = null!;
        public DbSet<PageAccessCounter> PageAccessCounters { get; set; } = null!;
        public DbSet<CellMappingTemplate> CellMappingTemplates { get; set; } = null!;
        public DbSet<CellMappingItem> CellMappingItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CellMappingTemplateとCellMappingItemのリレーションシップ
            modelBuilder.Entity<CellMappingTemplate>()
                .HasMany(t => t.MappingItems)
                .WithOne(i => i.Template)
                .HasForeignKey(i => i.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
