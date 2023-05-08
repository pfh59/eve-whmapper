using System;
using Microsoft.EntityFrameworkCore;
using WHMapper.Models.Db;

namespace WHMapper.Data
{
	public class WHMapperContext : DbContext
	{
        public DbSet<WHMap>? DbWHMaps { get; set; }
		public DbSet<WHSystem>? DbWHSystems { get; set; }
        public DbSet<WHSystemLink>? DbWHSystemLinks { get; set; }
        public DbSet<WHSignature>? DbWHSignatures { get; set; }
       

        public WHMapperContext(DbContextOptions<WHMapperContext> options) : base(options)
		{

        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WHMap>().ToTable("Maps");
            modelBuilder.Entity<WHMap>().HasIndex(x => new { x.Name }).IsUnique(true);

            modelBuilder.Entity<WHSystem>().ToTable("Systems");
            modelBuilder.Entity<WHSystem>().HasIndex(x => new { x.SoloarSystemId }).IsUnique(true);
            modelBuilder.Entity<WHSystem>().HasIndex(x => new { x.Name }).IsUnique(true);
            modelBuilder.Entity<WHSystem>().HasOne<WHMap>().WithMany(x => x.WHSystems).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WHSystemLink>().ToTable("SystemLinks");
            modelBuilder.Entity<WHSystemLink>().HasIndex(x => new { x.IdWHSystemFrom, x.IdWHSystemTo }).IsUnique(true);
            modelBuilder.Entity<WHSystemLink>().HasOne<WHMap>().WithMany(x => x.WHSystemLinks).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WHSignature>().ToTable("Signatures");
            modelBuilder.Entity<WHSignature>().HasIndex(x => new { x.Name }).IsUnique(true);
            modelBuilder.Entity<WHSignature>().HasOne<WHSystem>().WithMany(x => x.WHSignatures).OnDelete(DeleteBehavior.Cascade);
        }
    }
}

