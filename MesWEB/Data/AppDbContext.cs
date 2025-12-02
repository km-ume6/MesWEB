using Microsoft.EntityFrameworkCore;

namespace MesWEB.Data
{
    // Type aliases for backward compatibility
    using GrowthNoteItem = MesWEB.Shared.Data.GrowthNoteItem;
    using PageAccessCounter = MesWEB.Shared.Data.PageAccessCounter;
    using CellMappingLabel = MesWEB.Shared.Data.CellMappingLabel;
    using CellMappingTemplate = MesWEB.Shared.Data.CellMappingTemplate;
    using CellMappingItem = MesWEB.Shared.Data.CellMappingItem;

    // MesWEB.Data.AppDbContext is now an alias for MesWEB.Shared.Data.AppDbContext
    // This ensures backward compatibility while using the shared DbContext
    public class AppDbContext : MesWEB.Shared.Data.AppDbContext
    {
        public AppDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<MesWEB.Shared.Data.AppDbContext> options) : base(options) { }
    }
}