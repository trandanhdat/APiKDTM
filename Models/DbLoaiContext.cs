using APi.Models.Role;
using Microsoft.EntityFrameworkCore;

namespace APi.Models
{
    public class DbLoaiContext: DbContext
    {
        public DbLoaiContext() { }
        public DbLoaiContext(DbContextOptions<DbLoaiContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<LoaiModel>(options =>
            {
                options.HasIndex(l => l.MaLoai).IsUnique();
            });
            modelBuilder.Entity<UserModel>(options =>
            {
                options.HasIndex(u=>u.id).IsUnique();
                options.Property(u=>u.password).IsRequired();
                options.Property(p => p.phone).HasMaxLength(11);
                options.Property(p => p.phone).HasMaxLength(11);

            
            });
            modelBuilder.Entity<RefreshToken>(options =>
            {
                options.HasIndex(u => u.Id).IsUnique();

            });
            modelBuilder.Entity<Product>(options =>
            {
                options.HasKey(e => e.Id);
                options.Property(e => e.Name).IsRequired().HasMaxLength(200);
                options.Property(e => e.Description).HasMaxLength(1000);
                options.Property(e => e.Price).HasColumnType("decimal(18,2)");
                options.Property(e => e.ImagePath).HasMaxLength(500);
                options.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                options.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            });
            modelBuilder.Entity<UserRoleHistory>()
                .HasOne(h=>h.User)
                .WithMany(u=>u.RoleHistories)
                .HasForeignKey(u=>u.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserRoleHistory>()
                .HasOne(h=>h.ChangedByUser)
                .WithMany(u=>u.ChangedRoleHistories)
                .HasForeignKey(u=>u.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        public DbSet<LoaiModel> loaiModels { get; set; }
        public DbSet<UserModel> userModels { get; set; }
        public DbSet<RefreshToken> refreshTokens { get; set; }
        public DbSet<Product> products { get; set; }
        public DbSet<UserRoleHistory> userRoleHistories { get; set; }

    }
}
