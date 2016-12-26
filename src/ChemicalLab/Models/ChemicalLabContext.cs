using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EKIFVK.ChemicalLab.Models
{
    public class ChemicalLabContext : DbContext
    {
        public ChemicalLabContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Place>(entity =>
            {
                entity.ToTable("Place");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(45);
                entity.HasIndex(e => e.Name).HasName("UN_Name").IsUnique();
            });
            modelBuilder.Entity<Room>(entity =>
            {
                entity.ToTable("Room");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(45);
                entity.HasIndex(e => e.Name).HasName("UN_Name").IsUnique();
            });
            modelBuilder.Entity<Location>(entity =>
            {
                entity.ToTable("Location");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.HasOne(d => d.PlaceNavigation)
                    .WithMany(p => p.Locations)
                    .HasForeignKey(d => d.Place)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Location_Place");
                entity.HasOne(d => d.RoomNavigation)
                    .WithMany(p => p.Locations)
                    .HasForeignKey(d => d.Room)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Location_Room");
            });
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.ToTable("Permission");
                entity.HasKey(e => e.Name);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(45);
            });
            modelBuilder.Entity<PermissionGroup>(entity =>
            {
                entity.ToTable("PermissionGroup");
                entity.HasKey(e => e.Name);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(64);
                entity.Property(e => e.Permission).HasMaxLength(21485);
            });
            modelBuilder.Entity<UserGroup>(entity =>
            {
                entity.ToTable("UserGroup");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(45);
                entity.HasIndex(e => e.Name).HasName("UN_Name").IsUnique();
                entity.Property(e => e.Note).HasMaxLength(256);
                entity.Property(e => e.Permission).HasMaxLength(21485);
                entity.Property(e => e.Disabled).HasDefaultValue(false);
            });
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(128);
                entity.HasIndex(e => e.Name).HasName("UN_Name").IsUnique();
                entity.Property(e => e.DisplayName).HasMaxLength(128);
                entity.Property(e => e.AccessToken).HasMaxLength(36);
                entity.Property(e => e.LastAccessAddress).HasMaxLength(38);
                entity.Property(e => e.AllowMultiAddressLogin).HasDefaultValue(false);
                entity.Property(e => e.Disabled).HasDefaultValue(false);
                entity.HasOne(d => d.UserGroupNavigation)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.UserGroup)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_User_UserGroup");
            });
            modelBuilder.Entity<ModifyType>(entity =>
            {
                entity.ToTable("ModifyType");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(45);
                entity.HasIndex(e => e.Name).HasName("UN_Name").IsUnique();
            });
            modelBuilder.Entity<ModifyHistory>(entity =>
            {
                entity.ToTable("ModifyHistory");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.RecordId).HasColumnName("RecordID");
                entity.HasOne(d => d.ModifierNavigation)
                    .WithMany(p => p.ModifyHistories)
                    .HasForeignKey(d => d.Modifier)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Modify_User");
                entity.HasOne(d => d.ModifyTypeNavigation)
                    .WithMany(p => p.ModifyHistories)
                    .HasForeignKey(d => d.ModifyType)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Modify_ModifyType");
            });
            modelBuilder.Entity<ItemDetailType>(entity =>
            {
                entity.ToTable("ItemDetailType");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(45);
                entity.HasIndex(e => e.Name).HasName("UN_Name").IsUnique();
                entity.Property(e => e.RequireCas).HasColumnName("RequireCAS");
            });
            modelBuilder.Entity<Unit>(entity =>
            {
                entity.ToTable("Unit");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(45);
                entity.HasIndex(e => e.Name).HasName("UN_Name").IsUnique();
            });
            modelBuilder.Entity<PhysicalState>(entity =>
            {
                entity.ToTable("PhysicalState");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(45);
                entity.HasIndex(e => e.Name).HasName("UN_Name").IsUnique();
            });
            modelBuilder.Entity<ContainterType>(entity =>
            {
                entity.ToTable("ContainerType");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(45);
                entity.HasIndex(e => e.Name).HasName("UN_Name").IsUnique();
            });
            modelBuilder.Entity<ItemDetail>(entity =>
            {
                entity.ToTable("ItemDetail");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Prefix).HasMaxLength(256);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Cas).HasColumnName("CAS");
                entity.Property(e => e.Cas).HasMaxLength(192);
                entity.HasIndex(e => e.Cas).HasName("UN_CAS").IsUnique();
                entity.Property(e => e.Note).HasMaxLength(256);
                entity.Property(e => e.Disabled).HasDefaultValue(false);
                entity.HasOne(d => d.UnitNavigation)
                    .WithMany(p => p.ItemDetails)
                    .HasForeignKey(d => d.Unit)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ItemDetail_Unit");
                entity.HasOne(d => d.ContainterTypeNavigation)
                    .WithMany(p => p.ItemDetails)
                    .HasForeignKey(d => d.ContainerType)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ItemDetail_ContainerType");
                entity.HasOne(d => d.PhysicalStateNavigation)
                    .WithMany(p => p.ItemDetails)
                    .HasForeignKey(d => d.PhysicalState)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ItemDetail_PhysicalState");
                entity.HasOne(d => d.DetailTypeNavigation)
                    .WithMany(p => p.ItemDetails)
                    .HasForeignKey(d => d.DetailType)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ItemDetail_ItemDetailType");
            });
            modelBuilder.Entity<Experiment>(entity =>
            {
                entity.ToTable("Experiment");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(45);
                entity.HasIndex(e => e.Name).HasName("UN_Name").IsUnique();
            });
            modelBuilder.Entity<Vendor>(entity =>
            {
                entity.ToTable("Vendor");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(45);
                entity.HasIndex(e => e.Name).HasName("UN_Name").IsUnique();
                entity.Property(e => e.Number).IsRequired().HasMaxLength(45);
                entity.Property(e => e.Disabled).HasDefaultValue(false);
            });
            modelBuilder.Entity<Item>(entity =>
            {
                entity.ToTable("Item");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Disabled).HasDefaultValue(false);
                entity.HasOne(d => d.DetailNavigation)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.Detail)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Item_ItemDetail");
                entity.HasOne(d => d.LocationNavigation)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.Location)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Item_Location");
                entity.HasOne(d => d.OwnerNavigation)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.Owner)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Item_User");
                entity.HasOne(d => d.VendorNavigation)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.Vendor)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("Fk_Item_Vendor");
                entity.HasOne(d => d.ExperimentNavigation)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.Experiment)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("Fk_Item_Experiment");
            });
            modelBuilder.Entity<ItemUsage>(entity =>
            {
                entity.ToTable("ItemUsage");
                entity.Property(e => e.Id).HasColumnName("ID");
            });
        }

        public DbSet<Place> Places { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<PermissionGroup> PermissionGroups { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<ModifyType> ModifyTypes { get; set; }
        public DbSet<ModifyHistory> ModifyHistorys { get; set; }
        public DbSet<ItemDetailType> ItemDetailTypes { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<PhysicalState> PhysicalStates { get; set; }
        public DbSet<ContainterType> ContainterTypes { get; set; }
        public DbSet<ItemDetail> ItemDetails { get; set; }
        public DbSet<Experiment> Experiments { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemUsage> ItemUsages { get; set; }
    }
}
