
using Microsoft.EntityFrameworkCore;
using backend_server_mvc.Data;
using Google.Protobuf.WellKnownTypes;
using backend_server_mvc.Service;
using backend_server_mvc.Util;
namespace backend_server_mvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // Configure EntityFramework
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySQL(connectionString!)
            );

            // Add services to the container.
            builder.Services.AddScoped<IDeviceTokenAuthService, DeviceTokenAuthService>();
            builder.Services.AddScoped<IUserSessionAuthService, UserSessionAuthService>();
            builder.Services.AddHostedService<DatabaseCleanupService>();
            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //Lowercase routes
            builder.Services.AddRouting(options => options.LowercaseUrls = true);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            
            CreateDbIfNotExists(app);
            

            app.Run();
        }
        private static void CreateDbIfNotExists(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    //context.Database.EnsureCreated();
                    DbInitializer.Initialize(context); //remove in prod
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred creating the DB.");
                }
            }
        }
    }


}
