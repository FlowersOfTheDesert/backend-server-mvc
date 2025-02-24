using backend_server_mvc.Model;
using backend_server_mvc.Model.Device;
using backend_server_mvc.Model.Session;
using backend_server_mvc.Model.Shade;
using Microsoft.EntityFrameworkCore;

namespace backend_server_mvc.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BaseShadeConfig>()
                .HasDiscriminator<string>("config_type")
                .HasValue<WeatherShadeConfig>("config_weather")
                .HasValue<ScheduleShadeConfig>("config_schedule");


            modelBuilder.Entity<BaseShadeConfig>()
                .HasOne(e => e.Device)           // BaseShadeConfig refers to a Device
                .WithOne(e => e.DeviceConfiguration) // Device has one related BaseShadeConfig
                .HasForeignKey<BaseShadeConfig>(e => e.DeviceId)  // Foreign key lives in BaseShadeConfig
                .IsRequired(true);               // Config is required

            modelBuilder.Entity<DeviceSession>()
                .HasOne(e => e.Device)
                .WithOne(e => e.Session)
                .HasForeignKey<DeviceSession>(e => e.DeviceId)
                .IsRequired(true);

            modelBuilder.Entity<UserSession>()
                .HasOne(us => us.User)
                .WithMany(u => u.Session)
                .HasForeignKey(us => us.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.OwnedDevices)
                .WithOne(d => d.Owner)
                .HasForeignKey(d => d.OwnerId)
                .IsRequired(false);

        }
        public DbSet<Device> Devices { get; set; }
        public DbSet<BaseShadeConfig> ShadeConfig { get; set; }
        public DbSet<ChannelHeader> ChannelHeaders { get; set; }
        public DbSet<DeviceSession> DeviceSessions {  get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
    }
}
