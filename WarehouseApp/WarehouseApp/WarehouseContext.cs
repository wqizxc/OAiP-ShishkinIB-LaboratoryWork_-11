using Microsoft.EntityFrameworkCore;
using WarehouseApp.Models;

namespace WarehouseApp
{
    public class WarehouseContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Waybill> Waybills { get; set; }
        public DbSet<Product> Products { get; set; }

        private static WarehouseContext? _context;
        public static WarehouseContext GetContext()
        {
            if (_context == null) _context = new WarehouseContext();
            return _context;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder oP)
        {
            oP.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=WarehouseDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder mB)
        {
            mB.Entity<Employee>(e =>
            {
                e.ToTable("Employees");
                e.HasKey(e => e.Id);
                e.Property(e => e.Id).ValueGeneratedOnAdd();

                e.Property(e => e.FirstName).IsRequired().HasMaxLength(50).HasColumnName("First_Name");
                e.Property(e => e.LastName).IsRequired().HasMaxLength(50).HasColumnName("Last_Name");
                e.Property(e => e.BirthDate).IsRequired().HasColumnName("Birth_Date");
                e.Property(e => e.Email).IsRequired().HasMaxLength(100);
                e.Property(e => e.PasswordHash).IsRequired().HasMaxLength(128);
                e.Property(e => e.Phone).HasMaxLength(20);
                e.Property(e => e.Position).IsRequired().HasMaxLength(50);

                e.HasIndex(e => e.Email).IsUnique();

                e.HasMany(emp => emp.Waybills)
                 .WithOne(w => w.Employee)
                 .HasForeignKey(w => w.EmployeeId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            mB.Entity<Waybill>(w =>
            {
                w.ToTable("Waybills");
                w.HasKey(w => w.Id);
                w.Property(w => w.Id).ValueGeneratedOnAdd();

                w.Property(w => w.Number).IsRequired().HasMaxLength(50).HasColumnName("Doc_Number");
                w.Property(w => w.Date).IsRequired().HasColumnName("Doc_Date");
                w.Property(w => w.Type).IsRequired().HasMaxLength(10).HasColumnName("Doc_Type");

                w.HasMany(wb => wb.Products)
                 .WithOne(p => p.Waybill)
                 .HasForeignKey(p => p.WaybillId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            mB.Entity<Product>(p =>
            {
                p.ToTable("Products");
                p.HasKey(p => p.Id);
                p.Property(p => p.Id).ValueGeneratedOnAdd();

                p.Property(p => p.Name).IsRequired().HasMaxLength(100).HasColumnName("Prod_Name");
                p.Property(p => p.Article).IsRequired().HasMaxLength(50).HasColumnName("SKU");
                p.Property(p => p.Quantity).IsRequired().HasColumnName("Quantity");
            });

            base.OnModelCreating(mB);
        }
    }
}