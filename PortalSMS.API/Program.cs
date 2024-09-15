
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using PortalSMS.DAL.Data;
//using PortalSMS.DAL.Data.Context;
//using PortalSMS.DAL.Data.Models;
//using System.Text;

//namespace PortalSMS.API
//{
//    public class Program
//    {
//        public static async Task Main(string[] args)
//        {
//            var builder = WebApplication.CreateBuilder(args);

//            // Add services to the container.

//            builder.Services.AddControllers();
//            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//            builder.Services.AddEndpointsApiExplorer();
//            //builder.Services.AddSwaggerGen();
//            builder.Services.AddSwaggerGen(c =>
//            {
//                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PortalSMS.API", Version = "v1" });
//            });
//            builder.Services.AddDbContext<SystemContext>(a =>
//            {
//                a.UseSqlServer(builder.Configuration.GetConnectionString("con1"));
//            });
//            builder.Services.AddIdentity<User, IdentityRole>()
//    .AddEntityFrameworkStores<SystemContext>()
//    .AddDefaultTokenProviders();


//            builder.Services.AddCors(options =>
//            {
//                options.AddPolicy("AllowAll",
//                    builder =>
//                    {
//                        builder.AllowAnyOrigin()
//                               .AllowAnyMethod()
//                               .AllowAnyHeader();
//                    });
//            });

//            builder.Services.AddAuthentication(options =>
//            {
//                options.DefaultAuthenticateScheme = "default";
//                options.DefaultChallengeScheme = "default";
//            }).
//    AddJwtBearer("default", options =>
//    {
//        var secretKey = builder.Configuration.GetValue<string>("SecretKey");
//        var secretKeyInBytes = Encoding.ASCII.GetBytes(secretKey);
//        var key = new SymmetricSecurityKey(secretKeyInBytes);

//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = false,
//            ValidateAudience = false,
//            IssuerSigningKey = key
//        };
//    });
//            var app = builder.Build();

//            async Task InitializeAppAsync()
//            {
//                using (var scope = app.Services.CreateScope())
//                {
//                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//                    await SeedData.SeedRoles(roleManager);
//                }
//            }



//            // Execute asynchronous method
//            await InitializeAppAsync();

//            // Configure the HTTP request pipeline.
//            if (app.Environment.IsDevelopment())
//            {
//                app.UseSwagger();
//                app.UseSwaggerUI(c =>
//                {
//                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
//                });
//            }

//            app.UseHttpsRedirection();
//            app.UseAuthentication();

//            app.UseAuthorization();

//            app.UseCors("AllowAll");

//            app.MapControllers();

//            app.Run();
//        }
//    }
//}


using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PortalSMS.DAL.Data.Context;
using PortalSMS.DAL.Data.DataSeeding;
using PortalSMS.DAL.Data.Models;
using System.Text;
using Twilio.Types;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using PortalSMS.API.Service;


public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
       builder.Services.AddScoped<CsvService>();
        builder.Services.AddScoped<SmsService>();


        // Configure Swagger for JWT bearer token
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "PortalSMS.API", Version = "v1" });

            // Define the security scheme
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Please enter JWT token with Bearer prefix into the field"
            });


       //     const string accountSid = "ACce954994f855cb5249727bc0641c69e6";
       //     const string authToken = "92c74d101b8e45d9a00bbb8b324a783e";

       //     TwilioClient.Init(accountSid, authToken);


       //     var messageBody = "Hello from Twilio!";
       //var from=new PhoneNumber("+13133950727");
       //    var to= new PhoneNumber("+20 101 790 4618");

       //     try
       //     {
       //         var message = MessageResource.Create(
       //             body: messageBody,
       //             from: from,
       //             to: to
       //         );

       //         Console.WriteLine($"Message sent with SID: {message.Sid}");
       //     }
       //     catch (Exception ex)
       //     {
       //         Console.WriteLine($"Error sending message: {ex.Message}");
       //     }





            // Add security requirement
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
        });

        builder.Services.AddDbContext<SystemContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("con1"));
        });

        builder.Services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<SystemContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
        });

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
   .AddJwtBearer(options =>
   {
       var secretKey = builder.Configuration.GetValue<string>("SecretKey");
       var secretKeyInBytes = Encoding.ASCII.GetBytes(secretKey);
       var key = new SymmetricSecurityKey(secretKeyInBytes);

       options.TokenValidationParameters = new TokenValidationParameters
       {
           ValidateIssuer = false,
           ValidateAudience = false,
           IssuerSigningKey = key,
           ValidateLifetime = true, // Ensure token has not expired
           ClockSkew = TimeSpan.Zero // Optional: set to zero if you want no clock skew tolerance
       };
   });
       

        var app = builder.Build();

        async Task InitializeAppAsync()
        {
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                await SeedData.SeedRoles(roleManager);
            }
        }

        // Execute asynchronous method
        await InitializeAppAsync();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
             c.SwaggerEndpoint("/swagger/v1/swagger.json", "PortalSMS.API V1")
            );
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors("AllowAll");
        app.MapControllers();

        app.Run();
    }
}
