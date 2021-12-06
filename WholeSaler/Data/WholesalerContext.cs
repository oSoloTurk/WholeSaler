using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using WholeSaler.Areas.Identity.Pages.Account;
using WholeSaler.Models;
using Action = WholeSaler.Models.Action;

#nullable disable

namespace WholeSaler.Data
{
    public partial class WholesalerContext : DbContext
    {
        public WholesalerContext()
        {
        }

        public WholesalerContext(DbContextOptions<WholesalerContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Action> Actions { get; set; }
        public virtual DbSet<Basket> Baskets { get; set; }
        public virtual DbSet<BasketItem> BasketItems { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Alert> Alerts { get; set; }
        public virtual DbSet<Operation> Operations { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<Vehicle> Vehicles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Username=postgres;Password=123456;Server=localhost;Port=5432;Database=WholeSaler;Integrated Security=true;Pooling=true;Search Path=public");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<LoginModel>();

            modelBuilder.HasPostgresEnum(null, "ACTIONS", new[] { "LOOK", "CREATE", "EDIT", "DELETE", "VERIFY" })
                .HasPostgresEnum(null, "EMAIL_STATUS", new[] { "UNVERIFIED", "VERIFIED", "VERIFYING", "MANUAL_VERIFIED" })
                .HasPostgresEnum(null, "PERMISSIONS", new[] { "LOGIN", "CREATE_NEW_USER", "VEHICLE_CREATE", "VEHICLE_EDIT", "VEHICLE_DELETE", "VEHICLE_SEE_ALL", "VEHICLE_QUEST_MAKE", "ITEM_CREATE", "ITEM_EDIT", "ITEM_DELETE", "ITEM_SEE_ALL", "REPORTS_CLEAR", "REPORTS_SEE_ALL", "PROFILE_CHANCE_PHONE", "PROFILE_CHANCE_EMAIL", "PROFILE_CHANCE_PASSWORD", "PROFILE_PERSONAL_DATA", "PROFILE_CLEAR_SELF_DATAS", "PROFILE_DOWNLOAD_SELF_DATAS", "USERS_SEE_ALL", "USERS_EDIT", "USERS_DELETE", "USERS_DETAILS", "PROFILE_EFFECTED", "PROFILE_EFFECTER" })
                .HasPostgresEnum(null, "ROLES", new[] { "MANAGER", "SUPPORTER", "DRIVER", "CUSTOMER" })
                .HasPostgresEnum(null, "STATE", new[] { "Waiting", "Transporting", "Archive" })
                .HasPostgresExtension("adminpack")
                .HasAnnotation("Relational:Collation", "Turkish_Turkey.1254");

            modelBuilder.Entity<Action>(entity =>
            {
                entity.HasIndex(e => e.AffectedUser, "IX_Actions_AffectedUser");


                entity.Property(e => e.ActionID)
                    .ValueGeneratedNever()
                    .HasColumnName("ActionID");

                entity.Property(e => e.AffectedUser).IsRequired();

                entity.Property(e => e.EffecterUser).IsRequired();

                entity.HasOne(d => d.AffectedUserNavigation)
                    .WithMany(p => p.ActionAffectedUserNavigations)
                    .HasForeignKey(d => d.AffectedUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("affected");

                entity.HasOne(d => d.EffecterUserNavigation)
                    .WithMany(p => p.ActionEffecterUserNavigations)
                    .HasForeignKey(d => d.EffecterUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("effecter");
            });
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

                entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                    .IsUnique();

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.LockoutEnd).HasColumnType("timestamp with time zone");

                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

                entity.Property(e => e.UserName).HasMaxLength(256);
            });
            modelBuilder.Entity<Basket>(entity =>
            {
                entity.ToTable("Basket");

                entity.Property(e => e.BasketID)
                    .HasColumnName("BasketID")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.UserID)
                    .IsRequired()
                    .HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Baskets)
                    .HasForeignKey(d => d.UserID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("user_id");
            });

            modelBuilder.Entity<BasketItem>(entity =>
            {
                entity.HasKey(e => e.BasketID)
                    .HasName("basket_items_pkey");

                entity.Property(e => e.BasketItemID)
                    .HasColumnName("BasketItemID")
                    .UseIdentityAlwaysColumn();

                entity.ToTable("Basket_items");

                entity.HasIndex(e => e.ItemID, "IX_Basket_items_ItemID");

                entity.Property(e => e.BasketID)
                    .ValueGeneratedNever()
                    .HasColumnName("BasketID");

                entity.Property(e => e.Amount).HasDefaultValueSql("1");

                entity.Property(e => e.ItemID).HasColumnName("ItemID");

                entity.HasOne(d => d.Basket)
                    .WithOne(p => p.BasketItem)
                    .HasForeignKey<BasketItem>(d => d.BasketID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("basket_id");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.BasketItems)
                    .HasForeignKey(d => d.ItemID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("item_id");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");

                entity.Property(e => e.CategoryID)
                    .HasColumnName("CategoryID")
                    .UseIdentityAlwaysColumn();
            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("Country");

                entity.Property(e => e.CountryID)
                    .HasColumnName("CountryID")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.CountryName).IsRequired();
            });

            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasIndex(e => e.CategoryID, "IX_Items_CategoryID");

                entity.Property(e => e.ItemID)
                    .HasColumnName("ItemID")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.CategoryID).HasColumnName("CategoryID");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.CategoryID)
                    .HasConstraintName("category");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasIndex(e => e.LocationOwnerID, "IX_Locations_LocationOwnerID");

                entity.Property(e => e.LocationID)
                    .HasColumnName("LocationID")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.LocationOwnerID)
                    .IsRequired()
                    .HasColumnName("LocationOwnerID");

                entity.Property(e => e.CityID).HasColumnName("CityID");

                entity.HasOne(d => d.LocationOwner)
                    .WithMany(p => p.Locations)
                    .HasForeignKey(d => d.LocationOwnerID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("owner_id");

                entity.HasOne(d => d.City)
                    .WithMany(p => p.Locations)
                    .HasForeignKey(d => d.CityID)
                    .HasConstraintName("state_id");
            });

            modelBuilder.Entity<Alert>(entity =>
            {
                entity.HasIndex(e => e.AlertID, "IX_Messages_DateID");

                entity.HasOne(d => d.User)
                .WithMany(u => u.Alerts)
                .HasForeignKey(d => d.UserID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_id");
            });

            modelBuilder.Entity<Operation>(entity =>
            {
                entity.Property(e => e.OperationID)
                    .ValueGeneratedNever()
                    .HasColumnName("OperationID");

                entity.Property(e => e.BasketID).HasColumnName("BasketID");

                entity.Property(e => e.LocationID).HasColumnName("LocationID");

                entity.Property(e => e.OwnerID).HasColumnName("OwnerID");

                entity.Property(e => e.VehicleID).HasColumnName("VehicleID");

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.OperationNavigations)
                    .HasForeignKey(d => d.OwnerID)
                    .HasConstraintName("owner_id");

                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.Operations)
                    .HasForeignKey(d => d.VehicleID)
                    .HasConstraintName("vehicle_id");
            });

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.Property(e => e.VehicleID)
                    .HasColumnName("VehicleID")
                    .UseIdentityAlwaysColumn();
            });

            modelBuilder.Entity<City>(entity =>
            {
                entity.ToTable("City");

                entity.Property(e => e.CityID)
                    .HasColumnName("CityID")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.CityName).IsRequired();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}
