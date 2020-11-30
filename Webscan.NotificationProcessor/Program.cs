using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Webscan.NotificationProcessor.Datastore;
using Webscan.NotificationProcessor.Models;
using Webscan.NotificationProcessor.Models.Repository;

namespace Webscan.NotificationProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;

                    services.AddDbContext<WebscanContext>(options =>
                        options.UseSqlServer(configuration.GetConnectionString("Webscan")));

                    services.AddScoped<IStatusCheckRepository<StatusCheck>, StatusCheckRepository>();
                    services.AddScoped<IUserRepository<User>, UserRepository>();

                    services.AddHostedService<Worker>();
                });
    }
}
