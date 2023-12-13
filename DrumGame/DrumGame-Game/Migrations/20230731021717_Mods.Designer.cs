﻿// <auto-generated />
using DrumGame.Game.Stores.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DrumGame.Game.Migrations
{
    [DbContext(typeof(DrumDbContext))]
    [Migration("20230731021717_Mods")]
    partial class Mods
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.1");

            modelBuilder.Entity("DrumGame.Game.Stores.DB.BeatmapInfo", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<double>("LocalOffset")
                        .HasColumnType("REAL");

                    b.Property<long>("PlayTime")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Rating")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Beatmaps");
                });

            modelBuilder.Entity("DrumGame.Game.Stores.DB.ReplayInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("AccuracyHit")
                        .HasColumnType("INTEGER");

                    b.Property<long>("AccuracyTotal")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Bad")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Combo")
                        .HasColumnType("INTEGER");

                    b.Property<long>("CompleteTimeTicks")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Extra")
                        .HasColumnType("TEXT");

                    b.Property<int>("Good")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MapId")
                        .HasColumnType("TEXT");

                    b.Property<int>("MaxCombo")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Miss")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Mods")
                        .HasColumnType("TEXT");

                    b.Property<int>("Perfect")
                        .HasColumnType("INTEGER");

                    b.Property<double>("PlaybackSpeed")
                        .HasColumnType("REAL");

                    b.Property<long>("Score")
                        .HasColumnType("INTEGER");

                    b.Property<int>("StartNote")
                        .HasColumnType("INTEGER");

                    b.Property<double>("StartPosition")
                        .HasColumnType("REAL");

                    b.Property<long>("StartTimeTicks")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Replays");
                });
#pragma warning restore 612, 618
        }
    }
}
