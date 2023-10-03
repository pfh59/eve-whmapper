using System;
using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore;
using WHMapper.Models.Db;

namespace WHMapper.Data
{
	public class WHMapperContext : DbContext
	{
        public DbSet<WHAccess> DbWHAccesses { get; set; } = null!;
        public DbSet<WHAdmin> DbWHAdmins { get; set; } = null!;
        public DbSet<WHMap> DbWHMaps { get; set; } = null!;
		public DbSet<WHSystem> DbWHSystems { get; set; } = null!;
        public DbSet<WHSystemLink> DbWHSystemLinks { get; set; } = null!;
        public DbSet<WHSignature> DbWHSignatures { get; set; } = null!;
        public DbSet<WHNote> DbWHNotes { get; set; } = null!;


        public WHMapperContext(DbContextOptions<WHMapperContext> options) : base(options)
		{

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WHAccess>().ToTable("Accesses");
            modelBuilder.Entity<WHAccess>().HasIndex(x => new { x.EveEntityId }).IsUnique(true);
            modelBuilder.Entity<WHAccess>().HasIndex(x => new { x.EveEntityName }).IsUnique(true);


            modelBuilder.Entity<WHAdmin>().ToTable("Admins");
            modelBuilder.Entity<WHAdmin>().HasIndex(x => new { x.EveCharacterId }).IsUnique(true);
            modelBuilder.Entity<WHAdmin>().HasIndex(x => new { x.EveCharacterName }).IsUnique(true);

            modelBuilder.Entity<WHMap>().ToTable("Maps");
            modelBuilder.Entity<WHMap>().HasIndex(x => new { x.Name }).IsUnique(true);
            modelBuilder.Entity<WHMap>().HasMany(x => x.WHSystems).WithOne().HasForeignKey(x => x.WHMapId).IsRequired();
            modelBuilder.Entity<WHMap>().HasMany(x => x.WHSystemLinks).WithOne().HasForeignKey(x => x.WHMapId).IsRequired();

            modelBuilder.Entity<WHSystem>().ToTable("Systems");
            modelBuilder.Entity<WHSystem>().HasIndex(x => new { x.SoloarSystemId }).IsUnique(true);
            modelBuilder.Entity<WHSystem>().HasIndex(x => new { x.Name }).IsUnique(true);
            modelBuilder.Entity<WHSystem>().HasOne<WHMap>().WithMany(x => x.WHSystems).HasForeignKey(x =>x.WHMapId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WHSystem>().HasMany<WHSystemLink>().WithOne().HasForeignKey(x=>x.IdWHSystemFrom).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WHSystem>().HasMany<WHSystemLink>().WithOne().HasForeignKey(x => x.IdWHSystemTo).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WHSystemLink>().ToTable("SystemLinks");
            modelBuilder.Entity<WHSystemLink>().HasIndex(x => new { x.IdWHSystemFrom, x.IdWHSystemTo }).IsUnique(true);
            modelBuilder.Entity<WHSystemLink>().HasOne<WHMap>().WithMany(x => x.WHSystemLinks).HasForeignKey(x =>x.WHMapId).IsRequired().OnDelete(DeleteBehavior.Cascade);
      

            modelBuilder.Entity<WHSignature>().ToTable("Signatures");
            modelBuilder.Entity<WHSignature>().HasIndex(x => new { x.WHId,x.Name }).IsUnique(true);
            modelBuilder.Entity<WHSignature>().HasOne<WHSystem>().WithMany(x => x.WHSignatures).HasForeignKey(x=>x.WHId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WHNote>().ToTable("Notes");
            modelBuilder.Entity<WHNote>().HasIndex(x => new { x.SoloarSystemId }).IsUnique(true);

        }
    }
}

