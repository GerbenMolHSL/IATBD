using System.Globalization;
using System.Text;
using IATWeb;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace cmNG.core.web;

public static class Characters
{
    public const string LF = "\n";
    public const string TAB = "\t";
    public const string CR = "\r";
    public const string CRLF = "\r\n";
    public const string EMPTYSTRING = "";
    public const string BACKSLASH = "\\";
    public const string SPACE = " ";
}

public static class WebHelpers
{
    // Handle HTTP request
    public static async Task HandleRequest(HttpContext pobjContext)
    {
        try
        {
            using (WebThread thread = ThreadConfig.GetWebThread())
            {
                // Set the desired culture explicitly
                CultureInfo culture = new CultureInfo("nl-NL");

                // Apply the culture to the current thread
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;

                HttpResponse objResponse = pobjContext.Response;
                
                RequestHandler.HandleRequest();
            }
        }
        catch (System.Exception objException)
        {
            __WriteError(objException);
        }

        await Task.CompletedTask;

        void __WriteError(Exception pobjException)
        {
            try
            {
                string strMessage = pobjException.Message;

                try
                {
                    pobjContext.Response.Clear();
                }
                catch
                {
                    // Empty try / catch in case we are not allowed to clear the response
                }
                try
                {
                    pobjContext.Response.StatusCode = 500;
                }
                catch
                {
                    // Empty try / catch in case we are not allowed to set status code
                }

                HttpResponse objResponse = pobjContext.Response;

                objResponse.WriteAsync("<!DOCTYPE html>" + Characters.CRLF);
                objResponse.WriteAsync("<html>" + Characters.CRLF);
                objResponse.WriteAsync("<head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1, shrink-to-fit=no\"></head>" + Characters.CRLF);
                objResponse.WriteAsync("<body style=\"font-family: verdana; margin-top:40px; max-width: 90vw; margin-left: 10vw;\">" + Characters.CRLF);
                objResponse.WriteAsync(strMessage.Replace(Characters.CRLF, "<br>") + "<p>");
                objResponse.WriteAsync("</body>" + Characters.CRLF);
                objResponse.WriteAsync("</html>" + Characters.CRLF);
            }
            catch
            {
                // Rethrow to Kestrel
                throw pobjException;
            }

        } // WriteError
    } // HandleRequest

    // Read Kestrel options from config file and inject in pipeline options
    public static void InjectKestrelOptions(KestrelServerOptions pobjOptions)
    {
        pobjOptions.AllowSynchronousIO = true;
    }
    
    public static IApplicationBuilder UseStaticHttpContext(IApplicationBuilder pobjBuilder)
    {
        ThreadConfig.HttpContextAccessor = (IHttpContextAccessor)pobjBuilder.ApplicationServices.GetRequiredService(typeof(IHttpContextAccessor));
        return pobjBuilder;
    }

} // Static class WebHelpers

public class Startup
{
    public IConfiguration Configuration { get; }
    public IWebHostEnvironment CurrentEnvironment { get; }
    public Startup(IConfiguration pobjConfiguration, IWebHostEnvironment pobjCurrentEnvironment)
    {
        // Save just in case
        Configuration = pobjConfiguration;
        CurrentEnvironment = pobjCurrentEnvironment;
    }

    public void ConfigureServices(IServiceCollection pcolServices)
    {
        pcolServices.AddHttpContextAccessor();

        // If using Kestrel:
        pcolServices.Configure<KestrelServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
            WebHelpers.InjectKestrelOptions(options);
        });

        // Add compression
        pcolServices.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });
        
        // Add MVC services
        pcolServices.AddControllers();
    } // ConfigureServices

    public void Configure(IApplicationBuilder pobjApp)
    {
        WebHelpers.UseStaticHttpContext(pobjApp);
        
        pobjApp.Run(
            async (context) =>
            {
                await WebHelpers.HandleRequest(context);
            }
        );

    } // Configure

} // class Startup