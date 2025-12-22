using Microsoft.EntityFrameworkCore;
using WHMapper.Models.Db;

namespace WHMapper.Data
{
    public class WHMapperContext : DbContext
	{
        // Core tables
        public DbSet<WHMap> DbWHMaps { get; set; } = null!;
		public DbSet<WHSystem> DbWHSystems { get; set; } = null!;
        public DbSet<WHSystemLink> DbWHSystemLinks { get; set; } = null!;
        public DbSet<WHSignature> DbWHSignatures { get; set; } = null!;
        public DbSet<WHNote> DbWHNotes { get; set; } = null!;
        public DbSet<WHRoute> DbWHRoutes { get; set; } = null!;
        public DbSet<WHJumpLog> DbWHJumpLogs { get; set; } = null!;

        // Multi-tenant instance tables
        public DbSet<WHInstance> DbWHInstances { get; set; } = null!;
        public DbSet<WHInstanceAdmin> DbWHInstanceAdmins { get; set; } = null!;
        public DbSet<WHInstanceAccess> DbWHInstanceAccesses { get; set; } = null!;
        public DbSet<WHMapAccess> DbWHMapAccesses { get; set; } = null!;

        public WHMapperContext(DbContextOptions<WHMapperContext> options) : base(options)
		{

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Instance configuration
            modelBuilder.Entity<WHInstance>().ToTable("Instances");
            modelBuilder.Entity<WHInstance>().HasIndex(x => new { x.OwnerEveEntityId, x.OwnerType }).IsUnique(true);
            modelBuilder.Entity<WHInstance>().HasMany(x => x.WHMaps).WithOne().HasForeignKey(x => x.WHInstanceId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WHInstance>().HasMany(x => x.Administrators).WithOne().HasForeignKey(x => x.WHInstanceId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WHInstance>().HasMany(x => x.InstanceAccesses).WithOne().HasForeignKey(x => x.WHInstanceId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            // Instance Admin configuration
            modelBuilder.Entity<WHInstanceAdmin>().ToTable("InstanceAdmins");
            modelBuilder.Entity<WHInstanceAdmin>().HasIndex(x => new { x.WHInstanceId, x.EveCharacterId }).IsUnique(true);

            // Instance Access configuration
            modelBuilder.Entity<WHInstanceAccess>().ToTable("InstanceAccesses");
            modelBuilder.Entity<WHInstanceAccess>().HasIndex(x => new { x.WHInstanceId, x.EveEntityId, x.EveEntity }).IsUnique(true);

            modelBuilder.Entity<WHMap>().ToTable("Maps");
            modelBuilder.Entity<WHMap>().HasIndex(x => new { x.Name }).IsUnique(true);
            modelBuilder.Entity<WHMap>().HasMany(x => x.WHSystems).WithOne().HasForeignKey(x => x.WHMapId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WHMap>().HasMany(x => x.WHSystemLinks).WithOne().HasForeignKey(x => x.WHMapId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WHMap>().HasMany(x => x.WHMapAccesses).WithOne(x => x.WHMap).HasForeignKey(x => x.WHMapId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            // Map Access configuration
            modelBuilder.Entity<WHMapAccess>().ToTable("MapAccesses");
            modelBuilder.Entity<WHMapAccess>().HasIndex(x => new { x.WHMapId, x.EveEntityId, x.EveEntity }).IsUnique(true);

            modelBuilder.Entity<WHSystem>().ToTable("Systems");
            modelBuilder.Entity<WHSystem>().HasIndex(x => new { x.WHMapId,x.SoloarSystemId }).IsUnique(true);
            modelBuilder.Entity<WHSystem>().HasIndex(x => new { x.WHMapId,x.Name }).IsUnique(true);
            modelBuilder.Entity<WHSystem>().HasOne<WHMap>().WithMany(x => x.WHSystems).HasForeignKey(x =>x.WHMapId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WHSystem>().HasMany<WHSystemLink>().WithOne().HasForeignKey(x=>x.IdWHSystemFrom).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WHSystem>().HasMany<WHSystemLink>().WithOne().HasForeignKey(x => x.IdWHSystemTo).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WHSystemLink>().ToTable("SystemLinks");
            modelBuilder.Entity<WHSystemLink>().HasIndex(x => new { x.WHMapId,x.IdWHSystemFrom, x.IdWHSystemTo }).IsUnique(true);
            modelBuilder.Entity<WHSystemLink>().HasOne<WHMap>().WithMany(x => x.WHSystemLinks).HasForeignKey(x =>x.WHMapId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WHSystemLink>().HasMany<WHJumpLog>(x=>x.JumpHistory).WithOne().HasForeignKey(x => x.WHSystemLinkId).IsRequired().OnDelete(DeleteBehavior.Cascade);
      
            modelBuilder.Entity<WHSignature>().ToTable("Signatures");
            modelBuilder.Entity<WHSignature>().HasIndex(x => new { x.WHId,x.Name }).IsUnique(true);
            modelBuilder.Entity<WHSignature>().HasOne<WHSystem>().WithMany(x => x.WHSignatures).HasForeignKey(x=>x.WHId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WHNote>().ToTable("Notes");
            modelBuilder.Entity<WHNote>().HasOne<WHMap>().WithMany().HasForeignKey(x => x.MapId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WHNote>().HasIndex(x => new { x.MapId, x.SoloarSystemId }).IsUnique(true);

            modelBuilder.Entity<WHRoute>().ToTable("Routes");
            modelBuilder.Entity<WHRoute>().HasOne<WHMap>().WithMany().HasForeignKey(x => x.MapId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WHRoute>().HasIndex(x => new { x.MapId,x.SolarSystemId,x.EveEntityId }).IsUnique(true);

            modelBuilder.Entity<WHJumpLog>().ToTable("JumpLogs");
            modelBuilder.Entity<WHJumpLog>().HasIndex(x => new { x.CharacterId, x.JumpDate }).IsUnique(true);
        }
    }
}
