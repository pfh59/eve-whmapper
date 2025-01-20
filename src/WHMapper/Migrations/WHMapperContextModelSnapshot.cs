﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WHMapper.Data;

#nullable disable

namespace WHMapper.Migrations
{
    [DbContext(typeof(WHMapperContext))]
    partial class WHMapperContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("WHAccessWHMap", b =>
                {
                    b.Property<int>("WHAccessesId")
                        .HasColumnType("integer");

                    b.Property<int>("WHMapId")
                        .HasColumnType("integer");

                    b.HasKey("WHAccessesId", "WHMapId");

                    b.HasIndex("WHMapId");

                    b.ToTable("WHAccessWHMap");
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHAccess", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("EveEntity")
                        .HasColumnType("integer");

                    b.Property<int>("EveEntityId")
                        .HasColumnType("integer");

                    b.Property<string>("EveEntityName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("EveEntityId", "EveEntity")
                        .IsUnique();

                    b.ToTable("Accesses", (string)null);
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHAdditionnalAccount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("CharacterId")
                        .HasColumnType("integer");

                    b.Property<int>("MainAccountId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("MainAccountId");

                    b.ToTable("WHAdditionnalAccount");
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHAdmin", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("EveCharacterId")
                        .HasColumnType("integer");

                    b.Property<string>("EveCharacterName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("EveCharacterId")
                        .IsUnique();

                    b.ToTable("Admins", (string)null);
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHJumpLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("CharacterId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("JumpDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long?>("ShipItemId")
                        .HasColumnType("bigint");

                    b.Property<float?>("ShipMass")
                        .HasColumnType("real");

                    b.Property<int?>("ShipTypeId")
                        .HasColumnType("integer");

                    b.Property<int>("WHSystemLinkId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("WHSystemLinkId");

                    b.HasIndex("CharacterId", "JumpDate")
                        .IsUnique();

                    b.ToTable("JumpLogs", (string)null);
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHMainAccount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("CharacterId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CharacterId")
                        .IsUnique();

                    b.ToTable("MainAccounts", (string)null);
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHMap", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Maps", (string)null);
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHNote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<int>("MapId")
                        .HasColumnType("integer");

                    b.Property<int>("SoloarSystemId")
                        .HasColumnType("integer");

                    b.Property<int>("SystemStatus")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("MapId", "SoloarSystemId")
                        .IsUnique();

                    b.ToTable("Notes", (string)null);
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHSignature", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Group")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(7)
                        .HasColumnType("character varying(7)");

                    b.Property<string>("Type")
                        .HasColumnType("text");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UpdatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("WHId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("WHId", "Name")
                        .IsUnique();

                    b.ToTable("Signatures", (string)null);
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHSystem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("Locked")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<byte>("NameExtension")
                        .HasColumnType("smallint");

                    b.Property<double>("PosX")
                        .HasColumnType("double precision");

                    b.Property<double>("PosY")
                        .HasColumnType("double precision");

                    b.Property<float>("SecurityStatus")
                        .HasColumnType("real");

                    b.Property<int>("SoloarSystemId")
                        .HasColumnType("integer");

                    b.Property<int>("WHMapId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("WHMapId", "Name")
                        .IsUnique();

                    b.HasIndex("WHMapId", "SoloarSystemId")
                        .IsUnique();

                    b.ToTable("Systems", (string)null);
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHSystemLink", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("IdWHSystemFrom")
                        .HasColumnType("integer");

                    b.Property<int>("IdWHSystemTo")
                        .HasColumnType("integer");

                    b.Property<bool>("IsEndOfLifeConnection")
                        .HasColumnType("boolean");

                    b.Property<int>("MassStatus")
                        .HasColumnType("integer");

                    b.Property<int>("Size")
                        .HasColumnType("integer");

                    b.Property<int>("WHMapId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("IdWHSystemFrom");

                    b.HasIndex("IdWHSystemTo");

                    b.HasIndex("WHMapId", "IdWHSystemFrom", "IdWHSystemTo")
                        .IsUnique();

                    b.ToTable("SystemLinks", (string)null);
                });

            modelBuilder.Entity("WHMapper.WHRoute", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("EveEntityId")
                        .HasColumnType("integer");

                    b.Property<int>("MapId")
                        .HasColumnType("integer");

                    b.Property<int>("SolarSystemId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("MapId", "SolarSystemId", "EveEntityId")
                        .IsUnique();

                    b.ToTable("Routes", (string)null);
                });

            modelBuilder.Entity("WHAccessWHMap", b =>
                {
                    b.HasOne("WHMapper.Models.Db.WHAccess", null)
                        .WithMany()
                        .HasForeignKey("WHAccessesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WHMapper.Models.Db.WHMap", null)
                        .WithMany()
                        .HasForeignKey("WHMapId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHAdditionnalAccount", b =>
                {
                    b.HasOne("WHMapper.Models.Db.WHMainAccount", "MainAccount")
                        .WithMany("AdditionnalAccounts")
                        .HasForeignKey("MainAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MainAccount");
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHJumpLog", b =>
                {
                    b.HasOne("WHMapper.Models.Db.WHSystemLink", null)
                        .WithMany("JumpHistory")
                        .HasForeignKey("WHSystemLinkId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHNote", b =>
                {
                    b.HasOne("WHMapper.Models.Db.WHMap", null)
                        .WithMany()
                        .HasForeignKey("MapId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHSignature", b =>
                {
                    b.HasOne("WHMapper.Models.Db.WHSystem", null)
                        .WithMany("WHSignatures")
                        .HasForeignKey("WHId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHSystem", b =>
                {
                    b.HasOne("WHMapper.Models.Db.WHMap", null)
                        .WithMany("WHSystems")
                        .HasForeignKey("WHMapId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHSystemLink", b =>
                {
                    b.HasOne("WHMapper.Models.Db.WHSystem", null)
                        .WithMany()
                        .HasForeignKey("IdWHSystemFrom")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WHMapper.Models.Db.WHSystem", null)
                        .WithMany()
                        .HasForeignKey("IdWHSystemTo")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WHMapper.Models.Db.WHMap", null)
                        .WithMany("WHSystemLinks")
                        .HasForeignKey("WHMapId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WHMapper.WHRoute", b =>
                {
                    b.HasOne("WHMapper.Models.Db.WHMap", null)
                        .WithMany()
                        .HasForeignKey("MapId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHMainAccount", b =>
                {
                    b.Navigation("AdditionnalAccounts");
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHMap", b =>
                {
                    b.Navigation("WHSystemLinks");

                    b.Navigation("WHSystems");
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHSystem", b =>
                {
                    b.Navigation("WHSignatures");
                });

            modelBuilder.Entity("WHMapper.Models.Db.WHSystemLink", b =>
                {
                    b.Navigation("JumpHistory");
                });
#pragma warning restore 612, 618
        }
    }
}
