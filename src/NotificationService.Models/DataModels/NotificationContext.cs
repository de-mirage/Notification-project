using Microsoft.EntityFrameworkCore;

namespace NotificationService.Models.DataModels
{
    public class NotificationContext : DbContext
    {
        public NotificationContext(DbContextOptions<NotificationContext> options) : base(options)
        {
        }

        public DbSet<NotificationRecord> NotificationRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(255);
                entity.Property(e => e.Recipient).HasMaxLength(500);
                entity.Property(e => e.Subject).HasMaxLength(100);
                entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
                
                // Configure the Metadata property - ignore for now to avoid build issues
                entity.Ignore(e => e.Metadata);
                
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.NotificationType);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
