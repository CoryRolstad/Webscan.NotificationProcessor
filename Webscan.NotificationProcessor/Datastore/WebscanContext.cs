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
    }
}
