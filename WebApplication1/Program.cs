using Microsoft.EntityFrameworkCore;
using Serilog;
using WebApplication1.Models;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog for file logging
            Log.Logger = new LoggerConfiguration().MinimumLevel.Information().WriteTo.Console()
                .WriteTo.File("logs/app_log.txt")
                .CreateLogger();

            builder.Host.UseSerilog();
            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            Log.Information(builder.Configuration.GetConnectionString("DefaultConnection"));
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("Redis");
                options.InstanceName = "SampleInstance";
            });
            Log.Information(builder.Configuration.GetConnectionString("Redis"));

            var app = builder.Build();

            using (var Scope = app.Services.CreateScope())
            {
                var context = Scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.Migrate();
            }

            // Configure the HTTP request pipeline.

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
