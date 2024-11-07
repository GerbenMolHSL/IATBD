using System;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace cmNG.core.web
{
    public class Program
    {
        public static void Main(string[] pstrArgs)
        {
            // Initialize CaseMaster NextGen
            try
            {
                Task.Run(() =>
                {
                    // This will run every minute to check if 
                });

                // Set the desired culture explicitly
                CultureInfo culture = new CultureInfo("nl-NL");

                // Apply the culture to the current thread
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;

                // And build web-host
                BuildWebHost(pstrArgs).Run();
            }
            catch (Exception e)
            {
                string strError = null;

                strError = e.Message;

                throw new Exception(strError);
            }
        }
        public static IWebHost BuildWebHost(string[] pstrArgs)
        {
            return WebHost
                .CreateDefaultBuilder(pstrArgs)
                .UseStartup<Startup>()
                .Build();
        }

        public static IWebHostBuilder CreateDefaultBuilder(string[] pstrArgs)
        {
            return new WebHostBuilder()
                .UseKestrel()
                .ConfigureKestrel((context, options) => { WebHelpers.InjectKestrelOptions(options); }
            );
        }

    }
}
