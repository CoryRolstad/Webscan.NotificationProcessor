using Microsoft.EntityFrameworkCore;
using Webscan.NotificationProcessor.Models;

namespace Webscan.NotificationProcessor.Datastore
{
    public class WebscanContext : DbContext
    {
        public WebscanContext(DbContextOptions<WebscanContext> options) : base(options)
        {

        }


        public DbSet<StatusCheck> StatusChecks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<StatusCheckUser> StatusCheckUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>()
                .HasMany(u => u.StatusChecks)
                .WithMany(sc => sc.Users)
                .UsingEntity<StatusCheckUser>(
                    scu => scu.HasOne<StatusCheck>().WithMany(),
                    scu => scu.HasOne<User>().WithMany());

        }
    }
}
