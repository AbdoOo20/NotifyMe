using NotifyMe.Core.Services;

namespace NotifyMe.Core.Helper.ConfigurationMiddleware
{
    public static class ConfigurationMiddleware
    {
        public static Task<WebApplication> ConfigureMiddlewareAsync(this WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value;

                if (path == "/" || path == "/Home" || path == "/Home/Index")
                {
                    if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
                    {
                        if (context.User.IsInRole("Admin"))
                        {
                            context.Response.Redirect("/Admin");
                            return;
                        }
                        else if (context.User.IsInRole("Customer"))
                        {
                            context.Response.Redirect("/Customer");
                            return;
                        }
                    }
                }
                await next();
            });

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.MapRazorPages().WithStaticAssets();

            app.MapHub<NotificationHub>("/notificationHub");

            return Task.FromResult(app);
        }
    }
}
