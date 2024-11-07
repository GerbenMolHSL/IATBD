using IATWeb.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

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
            else if (currentPath == "/register")
            {
                RegisterPage.Create();
            }
            else if (currentPath == "/register/submit")
            {
                RegisterPage.Submit();
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

                bool userExists = SQL.Exists("Users", $"id", username, "hshdPsswrd", passwordEncrypted, "active", true);
                
                if(userExists)
                {
                    Guid sessionID = Thread.Session.GenerateSession(username);
                    Thread.HTTPContext.Response.Cookies.Append("shadowSessionIATBD", sessionID.ToString());
                    Thread.HTTPContext.Response.Redirect("/");
                }
                //else if (userInActive && userExists)
                //{
                //    LoginPage.Create(username, "Jou account is gedeactiveerd. Neem contact op met de beheerder.");
                //}
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
                else if (currentPath == "/profile")
                {
                    WriteHeader();
                    
                    Profiel.Create(false);
                    
                    WriteFooter();
                }
                else if (currentPath == "/profile/submit")
                {
                    WriteHeader();
                    
                    Profiel.Submit();
                    
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
                    
                    MijnAanvragen.CreateEdit(false);
                    
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
                else if (currentPath == "/acceptRequest")
                {
                    WriteHeader();
                    
                    RequestHelpers.AcceptRequest();
                    
                    WriteFooter();
                }
                else if (currentPath == "/finishRequest")
                {
                    WriteHeader();

                    FinishRequests.Create();
                    
                    WriteFooter();;
                }
                else if (currentPath == "/finishRequest/edit")
                {
                    WriteHeader();

                    FinishRequests.CreateEdit(false);
                    
                    WriteFooter();;
                }
                else if (currentPath == "/finishRequest/submit")
                {
                    WriteHeader();

                    FinishRequests.Submit(false);
                    
                    WriteFooter();;
                }
                else if (currentPath == "/denyRequest")
                {
                    WriteHeader();
                    
                    RequestHelpers.DenyRequest();
                    
                    WriteFooter();
                }
                else if (currentPath.StartsWith("/admin"))
                {
                    // Admin pages
                    Thread.Session.GetUserData();

                    if (!Thread.Session.UserProfile.IsAdmin)
                    {
                        WriteHeader();

                        NoAccess.Create();
                        
                        WriteFooter();
                    }
                    else if (currentPath.StartsWith("/admin/request/delete"))
                    {
                        WriteHeader();
                        
                        MijnAanvragen.Delete(true);
                        
                        WriteFooter();
                    }
                    else if (currentPath.StartsWith("/admin/request/submit"))
                    {
                        WriteHeader();
                        
                        MijnAanvragen.Submit(true);
                        
                        WriteFooter();
                    }
                    else if (currentPath.StartsWith("/admin/request"))
                    {
                        WriteHeader();

                        MijnAanvragen.CreateEdit(true);
                        
                        WriteFooter();
                    }
                    else if (currentPath.StartsWith("/admin/user/submit"))
                    {
                        WriteHeader();

                        Profiel.Submit(true);
                        
                        WriteFooter();
                    }
                    else if (currentPath.StartsWith("/admin/user"))
                    {
                        WriteHeader();
                        
                        Profiel.Create(true);
                        
                        WriteFooter();
                    }
                    else
                    {
                        WriteHeader();

                        AdminPage.Create();
                        
                        WriteFooter();
                    }
                }
                else if (currentPath.StartsWith("/aanvragen"))
                {
                    WriteHeader();
                    
                    Aanvragen.Create();
                    
                    WriteFooter();
                }
                else if (currentPath.StartsWith("/showPetProfile"))
                {
                    WriteHeader();
                    
                    MijnAanvragen.CreateProfile();
                    
                    WriteFooter();
                }
                else if (currentPath.StartsWith("/acceptPetRequest"))
                {
                    WriteHeader();
                    
                    MijnAanvragen.AcceptPetRequest();
                    
                    WriteFooter();
                }
                else if (currentPath.StartsWith("/upload"))
                {
                    FileHandler.Upload();
                }
                else if (currentPath.StartsWith("/download"))
                {
                    FileHandler.Download();
                }
                else if (currentPath.StartsWith("/view"))
                {
                    FileHandler.View();
                }
                else if (currentPath.StartsWith("/delete"))
                {
                    FileHandler.Delete();
                }
                else if (currentPath.StartsWith("/bestandsbeheer"))
                {
                    WriteHeader();
                    
                    FileHandler.CreateList();
                    
                    WriteFooter();
                }
                else if (currentPath.StartsWith("/myreviews"))
                {
                    WriteHeader();
                    
                    Reviews.CreateMyRatings();
                    
                    WriteFooter();
                }
                else if (currentPath.StartsWith("/reviews/edit"))
                {
                    WriteHeader();
                    
                    Reviews.CreateEdit();
                    
                    WriteFooter();
                }
                else if (currentPath.StartsWith("/reviews/submit"))
                {
                    WriteHeader();
                    
                    Reviews.Submit();
                    
                    WriteFooter();
                }
                else if (currentPath.StartsWith("/reviews"))
                {
                    WriteHeader();
                    
                    Reviews.Create();
                    
                    WriteFooter();
                }
                else if (currentPath.StartsWith("/searchFiles"))
                {
                    // check if post request
                    if (Thread.HTTPContext.Request.Method == "POST")
                    {
                        string searchValue = Thread.HTTPContext.Request.Form["search"];
                        string files = Thread.HTTPContext.Request.Form["files"];
                        Thread.HTTPContext.Response.WriteAsync(FileHandler.getFileDivs(searchValue, files));
                    }
                    else
                    {
                        Thread.HTTPContext.Response.StatusCode = 404;
                    }
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

    public static void WriteFile(string file)
    {
        WebThread Thread = ThreadConfig.GetWebThread();
        
        if (File.Exists(file))
        {
            string tmpFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(file));
            File.Copy(file, tmpFile, true);

            Thread.HTTPContext.Response.ContentType = MimeType.GetMimeType(Path.GetExtension(tmpFile));
            Thread.HTTPContext.Response.SendFileAsync(tmpFile).Wait();
        }
        else
        {
            Thread.HTTPContext.Response.StatusCode = 404;
        }
    }
    
    public static void DownloadFile(string file)
    {
        WebThread Thread = ThreadConfig.GetWebThread();
    
        if (File.Exists(file))
        {
            string tmpFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(file));
            File.Copy(file, tmpFile, true);
            
            // Set content type
            Thread.HTTPContext.Response.ContentType = "application/octet-stream";
        
            // Set content disposition header for attachment
            var contentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = Path.GetFileName(file)
            };
            Thread.HTTPContext.Response.Headers.Add(HeaderNames.ContentDisposition, contentDisposition.ToString());

            // Set content length
            var fileInfo = new FileInfo(tmpFile);
            Thread.HTTPContext.Response.Headers.Add(HeaderNames.ContentLength, fileInfo.Length.ToString());

            // Return file content
            using (var fileStream = new FileStream(tmpFile, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyToAsync(Thread.HTTPContext.Response.Body).Wait();
            }
        }
        else
        {
            Thread.HTTPContext.Response.StatusCode = 404;
        }
    }
}