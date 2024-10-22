using cldv_poe.Services;

namespace cldv_poe
{
    public class Program
    {

        public static void Main(string[] args)
        {
            // set globalization
            var cultureInfo = new System.Globalization.CultureInfo("en-ZA");
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            var builder = WebApplication.CreateBuilder(args);

            // Configure logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            var config = builder.Configuration ??
                throw new ArgumentException("Missing builder.Configuration");

            var temp = config.GetConnectionString("AzureStorage") ??
                throw new ArgumentException("Missing Configuration.ConnectionString.AzureStorage");

            string connStr = temp;

            // Add services to the container.
            QueueService queueService = new (connStr, "logs");
            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton(new BlobService(connStr, queueService));
            builder.Services.AddSingleton(new TableStorageService(connStr));
            //builder.Services.AddSingleton(new AzureFileShareService(connStr, "uploads", queueService));
            builder.Services.AddSingleton<QueueService>(sp =>
            {
                return new (connStr, "logs");
            });
            builder.Services.AddSingleton<AzureFileShareService>(sp =>
            {
                return new (connStr, "uploads", queueService);
            });

            var app = builder.Build();

            // Get logger instance
            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            // Log application start
            logger.LogInformation("Application starting up");

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.Run();
        }
    }

}
