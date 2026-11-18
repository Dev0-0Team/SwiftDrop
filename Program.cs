using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SwiftDrop.Helpers;
using SwiftDrop.Models.Entities;
using SwiftDrop.Repositories.Classes;
using SwiftDrop.Repositories.Interfaces;
using SwiftDrop.Services;
using SwiftDrop.Services.Classes;
using SwiftDrop.Services.Interfaces;
using System.Threading.RateLimiting;

namespace SwiftDrop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllersWithViews();


            string? encryptionKey = "Hello World";

            if (string.IsNullOrEmpty(encryptionKey))
            {
                throw new Exception("Encryption key is missing from environment variables.");
            }



            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            builder.Services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.FromMinutes(10);
            });

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {

                options.TokenLifespan = TimeSpan.FromHours(1);
            });



            static RateLimitPartition<string> IpAndUserAgentPartition(HttpContext context, int permitLimit, int windowMinutes)
            {
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
                var userAgent = context.Request.Headers.UserAgent.ToString() ?? "unknown-ua";

                var clientFingerprint = $"{clientIp}|{userAgent}";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientFingerprint,
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromMinutes(windowMinutes),
                        QueueLimit = 0
                    });
            }



            builder.Services.AddRateLimiter(options =>
            {
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json; charset=utf-8";

                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        Message = "Please try again later."
                    }, token);
                };

                options.AddPolicy("RegisterLimiting", context => IpAndUserAgentPartition(context, permitLimit: 2, windowMinutes: 1));

                options.AddPolicy("EmailConfirmationLimiting", context => IpAndUserAgentPartition(context, permitLimit: 1, windowMinutes: 1));

                options.AddPolicy("LoggingLimiting", context => IpAndUserAgentPartition(context, permitLimit: 10, windowMinutes: 1));

                options.AddPolicy("ForgetPasswordOTPLimiting", context => IpAndUserAgentPartition(context, permitLimit: 1, windowMinutes: 1));

                options.AddPolicy("VerityOTPLimiting", context => IpAndUserAgentPartition(context, permitLimit: 10, windowMinutes: 1));

                options.AddPolicy("InstructorRequestLimiting", context => IpAndUserAgentPartition(context, permitLimit: 3, windowMinutes: 1));

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                    context => IpAndUserAgentPartition(context, permitLimit: 500, windowMinutes: 1));
            });

            builder.Services.AddMemoryCache();



            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "RequestVerificationToken";
            });





            string encryptedDbConnection = builder.Configuration.GetConnectionString("DefaultConnection")!;

            if (encryptedDbConnection != null)
            {
                string decryptedDbConnection = EncryptionHelper.DecryptString(encryptedDbConnection, encryptionKey);

                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(decryptedDbConnection)
                );
            }



            var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>();
            if (emailSettings != null)
            {
                emailSettings.SenderPassword = EncryptionHelper.DecryptString(emailSettings.SenderPassword, encryptionKey);

                builder.Services.Configure<EmailSettings>(options =>
                {
                    options.SenderEmail = emailSettings.SenderEmail;
                    options.SenderPassword = emailSettings.SenderPassword;
                    options.SmtpServer = emailSettings.SmtpServer;
                    options.SmtpPort = emailSettings.SmtpPort;
                });
            }





            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();





            string googleClientId = builder.Configuration["Authentication:Google:ClientId"]!;
            string encryptedGoogleSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
            string decryptedGoogleSecret = null!;

            if (encryptedGoogleSecret != null)
            {
                decryptedGoogleSecret = EncryptionHelper.DecryptString(encryptedGoogleSecret, encryptionKey);
            }

            if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(decryptedGoogleSecret))
            {
                builder.Services.AddAuthentication()
                    .AddGoogle(options =>
                    {
                        options.ClientId = googleClientId;
                        options.ClientSecret = decryptedGoogleSecret;
                    });
            }
            else
            {
                builder.Services.AddAuthentication();
            }


            var cloudinarySettings = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
            if (cloudinarySettings != null)
            {
                cloudinarySettings.ApiSecret = EncryptionHelper.DecryptString(cloudinarySettings.ApiSecret, encryptionKey);

                builder.Services.Configure<CloudinarySettings>(options =>
                {
                    options.CloudName = cloudinarySettings.CloudName;
                    options.ApiKey = cloudinarySettings.ApiKey;
                    options.ApiSecret = cloudinarySettings.ApiSecret;
                });
            }


            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.LoginPath = "/auth/login";
                options.LogoutPath = "/auth/logout";
                options.AccessDeniedPath = "/auth/access-denied";
                options.SlidingExpiration = true;


                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };

            });




            builder.Services.AddScoped<IAuditRepository, AuditRepository>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICacheService, CacheService>();
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IUserService, UserService>();





            var app = builder.Build();


            app.UseForwardedHeaders();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }


            app.UseHttpsRedirection();



            //in production 
            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    OnPrepareResponse = context =>
            //    {

            //        context.Context.Response.Headers.Append("Cache-Control", "public,max-age=18000");
            //    }
            //});

            app.UseStaticFiles();
            app.UseRouting();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Home}/{id?}");

            app.Run();
        }
    }
}