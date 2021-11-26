using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WorldCities
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Execute().Wait();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json",
                            optional: false,
                            reloadOnChange: true)
                .AddJsonFile(string.Format("appsettings.{0}.json",
                            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                                ?? "Production"),
                            optional: true,
                            reloadOnChange: true)
                .AddUserSecrets<Startup>(optional: true, reloadOnChange: true)
                .Build();
            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer(
                    connectionString:
                        configuration.GetConnectionString("DefaultConnection"),
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "LogEvents",
                        AutoCreateSqlTable = true
                    })
            .WriteTo.Console()
            .CreateLogger();

            CreateHostBuilder(args).UseSerilog().Build().Run();
        }

        /// <summary>
        /// SendGrid implementation test
        /// </summary>
        static async Task Execute()
        {
            // WorldCities_API_Key
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("test@example.com", "Example User");
            var subject = "Sending with SendGrid is Fun";
            var to = new EmailAddress("test@example.com", "Example User");
            var plainTextContent = "and easy to do anywhere, even with C#";
            var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
