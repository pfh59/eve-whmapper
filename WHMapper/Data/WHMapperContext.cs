using System;
using Microsoft.EntityFrameworkCore;
using WHMapper.Models.Db;

namespace WHMapper.Data
{
	public class WHMapperContext : DbContext
	{
        protected readonly IConfiguration Configuration;

        public DbSet<WHMap> DbWHMaps { get; set; }
		public DbSet<WHSystem> DbWHSystems { get; set; }


		public WHMapperContext(DbContextOptions<WHMapperContext> options) : base(options)
		{

        }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WHMap>().ToTable("Maps");
            modelBuilder.Entity<WHMap>().HasIndex(x => new { x.Name }).IsUnique(true);
            modelBuilder.Entity<WHSystem>().ToTable("Systems");
            modelBuilder.Entity<WHSystem>().HasIndex(x => new { x.Name }).IsUnique(true);
            modelBuilder.Entity<WHSystem>().HasOne<WHMap>().WithMany(x => x.WHSystems).OnDelete(DeleteBehavior.Cascade);
        }
    }
}

