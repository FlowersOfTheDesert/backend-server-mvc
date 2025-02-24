using backend_server_mvc.Data;
using backend_server_mvc.Model;
using backend_server_mvc.Model.Device;
using backend_server_mvc.Model.Shade;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;

namespace backend_server_mvc.Util
{
    public class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();
            //Clear ephemeral tables
            context.ChannelHeaders.ExecuteDelete();
            context.DeviceSessions.ExecuteDelete();

            if (!context.Devices.Any())
            {
                var devices = new Device[]
                {
                    new Device {
                        Id = Guid.NewGuid().ToString(),
                        Serial="sunshade-01",
                        Label=string.Empty,
                        Psk= "secretkey", 
                        Status=ShadeStatus.OFFLINE},
                     new Device {
                        Id = Guid.NewGuid().ToString(),
                        Serial="sunshade-02",
                        Label=string.Empty,
                        Psk= "secretkey",
                        Status=ShadeStatus.OFFLINE}

                };
                context.Devices.AddRange(devices);
                context.SaveChanges();

            }

            if (!context.ShadeConfig.Any())
            {
                var device = context.Devices.First();
                context.ShadeConfig.Add(new WeatherShadeConfig
                {
                    Id = Guid.NewGuid().ToString(),
                    Longitude = 1.0f,
                    Latitude = 2.0f,
                    CloseShadeThresholdTemp =
                    25.0f,
                    OpenShadeThresholdTemp = 30.0f,
                    Device = device
                });
                context.SaveChanges();
            }

            if (!context.Users.Any())
            {

                var (hash, salt) = PasswordHelper.HashPassword("password");
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "email@gmail.com",
                    Username = "Andre",
                    Password = hash,
                    Salt = salt,
                };
                user.OwnedDevices!.Add(context.Devices.First());
                context.Users.Add(user);
                context.SaveChanges();
            }

            
        }

        
    }
}
