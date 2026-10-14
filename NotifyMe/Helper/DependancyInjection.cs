using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using NotifyMe.Repository.Data.Contexts;
using NotifyMe.Repository.UnitOfWork;
using NotifyMe.Core.Services.Interface;
using NotifyMe.Services.AdminService;
using NotifyMe.Services.CommonService;
using NotifyMe.Services.CustomerService;
using NotifyMe.Services.MonitorService;
using NotifyMe.Services.BackgroundServices;
using NotifyMe.Services.AlertService;
using NotifyMe.Services.SendAlertToThirdParty;
using NotifyMe.Core.Entities;
using NotifyMe.Services.SubscriptionService;
using NotifyMe.Services.NotificationService;
using NotifyMe.Services.MonitorErrorService;

namespace NotifyMe.Core.Helper.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependency(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextService(configuration);
            services.AddBuiltInService();
            services.AddRolesService();
            services.AddBackgroundService();
            services.AddUserDefineService();
            services.AddIdentityService();
            return services;
        }

        private static IServiceCollection AddBuiltInService(this IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddSignalR();
            return services;
        }

        private static IServiceCollection AddDbContextService(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly("NotifyMe.App")));

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            return services;
        }

        private static IServiceCollection AddRolesService(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy =>
                {
                    policy.RequireRole("Admin");
                });
                options.AddPolicy("Customer", policy =>
                {
                    policy.RequireRole("Customer");
                });
            });
            return services;
        }

        private static IServiceCollection AddBackgroundService(this IServiceCollection services)
        {
            services.AddHostedService<MonitorCheckerService>();
            services.AddHostedService<SubscriptionExpiredService>();

            return services;
        }

        private static IServiceCollection AddIdentityService(this IServiceCollection services)
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.Events.OnRedirectToAccessDenied = context => {
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            return services;
        }

        private static IServiceCollection AddUserDefineService(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IMonitorService, MonitorService>();
            services.AddScoped<IAlertService, AlertService>();
            services.AddTransient<ISendAlertToThirdParty, SendAlertToThirdParty>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IMonitorErrorService, MonitorErrorService>();

            return services;
        }
    }
}
