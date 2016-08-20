using System;
using System.Data.Entity;
using Filtration.ObjectModel;

namespace Filtration.ItemFilterPreview.Data.DataContexts
{
    public class FiltrationDbContext : DbContext
    {
        public FiltrationDbContext() : base("name=FiltrationDbContext")
        {
            // Disable database initializer
            Database.SetInitializer<FiltrationDbContext>(null);
            Database.Log = Console.WriteLine;
        }

        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<ItemSet> ItemSets { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<Item>()
                .Property(e => e.BaseType)
                .IsUnicode(false);

            modelBuilder.Entity<Item>()
                .Property(e => e.ItemClass)
                .IsUnicode(false);

            modelBuilder.Entity<Item>()
                .Property(e => e.Sockets)
                .IsUnicode(false);

            modelBuilder.Entity<ItemSet>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<ItemSet>()
                .HasMany(e => e.Items)
                .WithRequired(e => e.ItemSet)
                .WillCascadeOnDelete(false);
        }
    }
}
