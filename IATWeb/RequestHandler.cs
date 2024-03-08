using IATWeb.Pages;
using Microsoft.AspNetCore.Http;

namespace IATWeb;

public static class RequestHandler
{
    public static void HandleRequest()
    {
        try
        {
            WebThread Thread = ThreadConfig.GetWebThread();
            string currentPath = Thread.HTTPContext.Request.Path;

            if(currentPath.StartsWith("/css") || currentPath.StartsWith("/js") || currentPath.StartsWith("/images") || currentPath.StartsWith("/assets") || currentPath.StartsWith("/lib"))
            {
                string[] pathParts = currentPath.Split('/');
        
                // Serve static files
                Thread.HTTPContext.Response.StatusCode = 200;
                
                List<string> pathPartsList = new();
                pathPartsList.Add(Directory.GetCurrentDirectory());
                pathPartsList.Add("web");
                foreach (string part in pathParts)
                {
                    if (!string.IsNullOrEmpty(part))
                    {
                        pathPartsList.Add(part);
                    }
                }

                string pathCombined = Path.Combine(pathPartsList.ToArray());
                
                WriteFile(pathCombined);
            }
            else if (currentPath == "/login" && Thread.HTTPContext.Request.Method == "POST")
            {
                // Handle login
                string username = Thread.HTTPContext.Request.Form["username"];
                string password = Thread.HTTPContext.Request.Form["password"];

                List<string> errors = new();
                
                if (string.IsNullOrEmpty(username))
                {
                    errors.Add("Username is required");
                }
                
                if (string.IsNullOrEmpty(password))
                {
                    errors.Add("Password is required");
                }
                
                if (errors.Count > 0)
                {
                    LoginPage.Create(username, errors.ToArray());
                    return;
                }
                
                string passwordEncrypted = Helpers.GenerateSHA256(password);
                
                if(SQL.Exists("Users", $"id",username, "hshdPsswrd", passwordEncrypted))
                {
                    Guid sessionID = Thread.Session.GenerateSession(username);
                    Thread.HTTPContext.Response.Cookies.Append("shadowSessionIATBD", sessionID.ToString());
                    Thread.HTTPContext.Response.Redirect("/");
                }
                else
                {
                    LoginPage.Create(username, "Incorrect username and/or password");
                }
            }
            else if (currentPath == "/logout")
            {
                if(Thread.HTTPContext.Request.Cookies.ContainsKey("shadowSessionIATBD")) Thread.HTTPContext.Response.Cookies.Delete("shadowSessionIATBD");
                Thread.HTTPContext.Response.Redirect("/");
            }
            else if (Thread.Session.CheckSession())
            {
                //Successful login
                if(currentPath == "/")
                {
                    WriteHeader();
                
                    Dashboard.Create();

                    WriteFooter();   
                }
                else if (currentPath == "/mijndieren")
                {
                    WriteHeader();
                    
                    MijnDieren.Create();
                    
                    WriteFooter();
                }
                else if (currentPath == "/mijndieren/edit")
                {
                    WriteHeader();
                    
                    MijnDieren.CreateEdit();
                    
                    WriteFooter();
                }
                else if (currentPath == "/mijndieren/submit")
                {
                    WriteHeader();

                    MijnDieren.Submit();
                    
                    WriteFooter();
                }
                else if (currentPath == "/mijndieren/delete")
                {
                    WriteHeader();

                    MijnDieren.Delete();
                    
                    WriteFooter();
                }
                else if (currentPath == "/mijnaanvragen")
                {
                    WriteHeader();
                    
                    MijnAanvragen.Create();
                    
                    WriteFooter();
                }
                else if (currentPath == "/mijnaanvragen/edit")
                {
                    WriteHeader();
                    
                    MijnAanvragen.CreateEdit();
                    
                    WriteFooter();
                }
                else if (currentPath == "/mijnaanvragen/submit")
                {
                    WriteHeader();
                
                    MijnAanvragen.Submit();
                    
                    WriteFooter();
                }
                else if (currentPath == "/mijnaanvragen/delete")
                {
                    WriteHeader();
                
                    MijnAanvragen.Delete();
                    
                    WriteFooter();
                }
                else if (currentPath == "/vorigeoppassers")
                {
                    WriteHeader();
                    
                    VorigeOppassers.Create();
                    
                    WriteFooter();
                }
                else if (currentPath == "/mijnoppasgeschiedenis")
                {
                    WriteHeader();
                    
                    OppasGeschiedenis.Create();
                    
                    WriteFooter();
                }
                else
                {
                    Thread.HTTPContext.Response.StatusCode = 404;
                }
            }
            else
            {
                LoginPage.Create();
            }
        }
        catch (Exception e)
        {
            ExceptionHandler.HandleError(e);
        }
    }

    public static void WriteHeader()
    {
        HttpResponse response = ThreadConfig.GetWebThread().HTTPContext.Response;

        response.WriteAsync(BuildString.NewString(
            "<!DOCTYPE html>",
            "<html lang=\"en\">",
            "<head>",
            "    <meta charset=\"UTF-8\">",
            "    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">",
            "    <meta http-equiv=\"X-UA-Compatible\" content=\"ie=edge\">",
            "    <title>PassenOpJeDier</title>",
            "    <link rel=\"stylesheet\" href=\"/css/semantic.css\">",
            "    <link rel=\"stylesheet\" href=\"/css/style.css\">",
            "    <link rel=\"stylesheet\" href=\"/lib/fontawesome-6.4.0/css/all.css\">",
            "    <script src=\"/js/jquery-3.7.1.min.js\"></script>",
            "    <script src=\"/js/semantic.min.js\"></script>",
            "    <script src=\"/js/index.js\"></script>",
            "    <script src=\"/lib/fontawesome-6.4.0/js/all.js\"></script>",
            "    <link rel=\"icon\" href=\"/favicon.ico\" type=\"image/x-icon\">",
            "</head>",
            "<body>"));
    }

    public static void WriteFooter()
    {
        HttpResponse response = ThreadConfig.GetWebThread().HTTPContext.Response;

        response.WriteAsync(BuildString.NewString(
            "</body>",
            "</html>"));
    }

    private static void WriteFile(string file)
    {
        WebThread Thread = ThreadConfig.GetWebThread();
        
        if (File.Exists(file))
        {
            Thread.HTTPContext.Response.ContentType = MimeType.GetMimeType(Path.GetExtension(file));
            Thread.HTTPContext.Response.SendFileAsync(file).Wait();
        }
        else
        {
            Thread.HTTPContext.Response.StatusCode = 404;
        }
    }
}