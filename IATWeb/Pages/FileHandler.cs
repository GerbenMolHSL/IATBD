using System.Data;
using IATWeb.Authenticators;
using IATWeb.Pages.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace IATWeb.Pages;

public class FileHandler
{
    public static void Upload()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.GetUserData();
        HttpRequest request = thread.HTTPContext.Request;
        
        // Check if request is POST
        if (request.Method == "POST")
        {
            string documentID = "";
            // Loop over files
            foreach (var file in request.Form.Files)
            {
                // Get the file name
                string fileName = file.FileName;

                // Get the file extension
                string fileExtension = Path.GetExtension(fileName);

                string id = SQL.GetNewID("Documents");

                // Generate a new document ID
                documentID += id;

                // Filename without -id
                string origFile = Path.Combine(Directory.GetCurrentDirectory(), "web", "uploads",
                    DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString(), DateTime.Now.Day.ToString(),
                    fileName);

                // If exists delete
                if(SQL.Exists("Documents", "%fileName", fileName, "owner", thread.Session.SessionData.user, "%filePath", origFile))
                {
                    // SQL.Delete("Documents", authenticator, "%fileName", fileName, "owner", thread.Session.SessionData.user);
                    // Delete from disk
                    // System.IO.File.Delete(filePath);
                    documentID = SQL.Get("Documents", "id", "%fileName", fileName, "owner", thread.Session.SessionData.user, "%filePath", origFile)["id"].ToString();
                }
                else
                {
                    // Save the file in path year/month/day with name as: filename-id.extension
                    string filePathWithNewID = Path.Combine(Directory.GetCurrentDirectory(), "web", "uploads",
                        DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString(), DateTime.Now.Day.ToString(),
                        fileName + "-" + id + fileExtension);
                    
                    // Insert the document in the database
                    Dictionary<string, StringValues> form = new();

                    form["owner"] = thread.Session.SessionData.user;
                    form["fileName"] = fileName;
                    form["filePath"] = filePathWithNewID;

                    IFormCollection formCollection = new FormCollection(form);

                    FileAuthenticator authenticator =
                        new FileAuthenticator("id", id, "owner", thread.Session.SessionData.user);
                    
                    if (!SQL.InsertOrUpdateForm("Documents", true, authenticator, out var errors, formCollection, new List<string>(){"owner", "fileName"},"owner", "fileName",
                            "filePath"))
                    {
                        thread.HTTPContext.Response.StatusCode = 500;
                        thread.HTTPContext.Response.WriteAsync("Internal server error");
                    }   
                }

                // Save the file in path year/month/day with name as: filename-id.extension
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "web", "uploads",
                    DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString(), DateTime.Now.Day.ToString(),
                    fileName + "-" + documentID + fileExtension);
                
                // Create directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

            }
            
            thread.HTTPContext.Response.StatusCode = 200;
            thread.HTTPContext.Response.ContentType = "application/json";
            thread.HTTPContext.Response.WriteAsync("{\"id\":\"" + documentID + "\"}");
        }
    }

    public static void CreateList()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        HttpResponse response = thread.HTTPContext.Response;
        
        FileAuthenticator authenticator = new FileAuthenticator("owner", thread.Session.SessionData.user, "owner", thread.Session.SessionData.user);

        if (!authenticator.AuthenticateGet())
        {
            NoAccess.Create();
            return;
        }
        
        Sidebar.Create("Bestanden");
        
        DataTable dt = SQL.DoSearch("Documents", "*", "owner", thread.Session.SessionData.user);
        
        response.WriteAsync(BuildString.NewString("<div id=\"content\">",
            List.Create(dt, "", "", false, false, new Dictionary<string, string>()
            {
                {"id", "ID"},
                {"fileName", "Bestandsnaam"}
            }, new Dictionary<string, Type>()
            {
                {"id", typeof(string)},
                {"fileName", typeof(string)}
            }, new(), "", new()
            {
                {"icon trash red", "Weet u zeker dat u dit bestand wilt verwijderen?|delete"}
            }, "fileName")
            )
        );
        
        Sidebar.CloseSidebar();
    }

    public static string GetFileUpload(string columnName, string data, bool isLocked)
    {
        string strFiles = "";

        if (!isLocked)
        {
            // Add button to upload files
            strFiles += "<button type=\"button\" class=\"ui button primary\" onclick=\"uploadFile(this)\">";
            strFiles += "Upload nieuw bestand";
            strFiles += "</button>";
        
            // Add search button to search for files
            strFiles += $"<button type=\"button\" class=\"ui button secondary\" onclick=\"$('#{columnName}FileSelect.ui.modal').modal('show')\">";
            strFiles += "Zoek bestand";
            strFiles += "</button>";
        }

        if (!string.IsNullOrEmpty(data))
        {
            try
            {
                List<File> files = resolveFiles(data);
                foreach (File file in files)
                {
                    // Each file gets own content div
                    strFiles += generateFileDiv(file, columnName, isLocked);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        string strReturn = "";
        
        strReturn += $"<div class=\"FileUpload\">";
        if(!isLocked) strReturn += $"<input id=\"{columnName}\" type=\"file\" id=\"fileInput\" style=\"display: none;\">";
        if(!isLocked) strReturn += $"<input name=\"{columnName}\" value=\"{data}\" type=\"text\" style=\"display: none;\">";
        strReturn += strFiles;
        strReturn += "</div>";
        
        strReturn += generateShowFileDiv(columnName, data);
        
        return strReturn;
    }
    
    private static string generateFileDiv(File file, string columnName, bool locked)
    {
        string strReturn = "";

        if (file == null)
        {
            file = new File();
        }

        strReturn += $"<div class=\"content\" data-id=\"{file.ID}\">";
        strReturn += "<div class=\"buttons\">";
        strReturn += $"<input type=\"text\" class=\"disabled-text\" value=\"{file.FileName}\" disabled>";
        if (!locked)
        {
            strReturn += $"<button type=\"button\" class=\"btn delete-btn\" onclick=\"deleteFile('{file.ID}','{columnName}')\">";
            strReturn += "<i class=\"fas fa-trash-alt\"></i>";
            strReturn += "</button>";
            strReturn += $"<button type=\"button\" class=\"btn download-btn\" onclick=\"downloadFile('{file.ID}','{columnName}')\">";
            strReturn += "<i class=\"fas fa-download\"></i>";
            strReturn += "</button>";
        }
        strReturn += $"<button type=\"button\" class=\"btn download-btn\" onclick=\"viewFile('{file.ID}','{columnName}')\">";
        strReturn += "<i class=\"fas fa-magnifying-glass\"></i>";
        strReturn += "</button>";
        strReturn += "</div>";
        strReturn += "</div>";
        
        return strReturn;
    }

    private static string generateShowFileDiv(string id, string data)
    {
        string strReturn = "";
            
        strReturn += $"<div id=\"{id}FileSelect\" class=\"ui modal\">";
        strReturn += "<div class=\"header\">Bestand</div>";
        strReturn += "<div class=\"content ui form FileSelect\">";
        strReturn += "<div class=\"search\">";
        strReturn += "<div class=\"field\">";
        strReturn += $"<label for=\"filter\">Zoeken</label>";
        strReturn += $"<input type=\"text\" id=\"filter\">";
        strReturn += "</div>";
        strReturn += "</div>";
        strReturn += "<div class=\"files\">";
        strReturn += getFileDivs("", data);
        strReturn += "</div>";
        strReturn += "</div>";
        
        strReturn += "<div class=\"actions\">";
        strReturn += $"<button type=\"button\" class=\"ui positive button\">Selecteren</button>";
        strReturn += "</div>";
        strReturn += "</div>";

        strReturn += "<script>";
        strReturn += $"initiateFileSelect('{id}FileSelect');";
        strReturn += "</script>";

        return strReturn;
    }

    public static string generateViewFileDiv()
    {
        string strReturn = "";

        strReturn += $"<div id=\"FileViewDiv\" class=\"ui modal\">";
        strReturn += "<div class=\"header\">Bestand weergeven</div>";
        strReturn += "<div class=\"content\">";
        strReturn += "<iframe id=\"fileView\" src=\"\" style=\"width: 100%; height: 60vh;\"></iframe>";
        strReturn += "</div>";
        
        strReturn += "<div class=\"actions\">";
        strReturn += $"<button type=\"button\" class=\"ui positive button\">Sluiten</button>";
        strReturn += "</div>";
        strReturn += "</div>";
        
        return strReturn;
    }

    public static string getFileDivs(string search = "", string files = "")
    {
        ThreadConfig.GetWebThread().Session.LoadSessionData();
        DataTable dt = SQL.DoSearch("Documents", "id,fileName,filePath", "owner", ThreadConfig.GetWebThread().Session.SessionData.user, "%fileName", search); 
        
        string strReturn = "";
        
        // Generate the file divs, these need to contain a radio button to select the file, and the filename
        
        foreach (DataRow row in dt.Rows)
        {
            if(string.IsNullOrEmpty(files) || !files.Split(";").Contains(row["id"].ToString()))
            {
                strReturn += "<div class=\"file\">";
                strReturn += "<div class=\"field\">";
                strReturn += $"<input type=\"checkbox\" name=\"file\" data-name=\"{row["fileName"]}\" data-id=\"{row["id"]}\">";
                strReturn += "</div>";
                strReturn += "<div class=\"field\">";
                strReturn += $"<div>{row["fileName"]}</div>";
                strReturn += "</div>";
                strReturn += "</div>";   
            }
        }

        return strReturn;
    }
    
    private static List<File> resolveFiles(string data)
    {
        List<File> files = new();
        string[] fileData = data.Split(";");
        List<string> whereClause = new();
        
        foreach(string file in fileData)
        {
            if (file != "")
            {
                whereClause.Add("|id");
                whereClause.Add(file);
            }
        }
        
        DataTable dt = SQL.DoSearch("Documents","id,fileName,filePath", whereClause.ToArray());
        
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        
        foreach (DataRow row in dt.Rows)
        {
            FileAuthenticator authenticator = new FileAuthenticator("id", row["id"].ToString(), "owner", thread.Session.SessionData.user);
            if (authenticator.AuthenticateGet())
            {
                File file = new();
                file.FileName = row["fileName"].ToString();
                file.FilePath = row["filePath"].ToString();
                file.ID = row["id"].ToString();
                files.Add(file);
            }
        }
        
        return files;
    }

    public static void Download()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.GetUserData();
        HttpRequest request = thread.HTTPContext.Request;
        
        string id = request.Query["id"];
        
        FileAuthenticator authenticator = new FileAuthenticator("id", id, "owner", thread.Session.SessionData.user);

        if (authenticator.AuthenticateGet())
        {
            // Get the file path
            string filePath = SQL.Get("Documents", "filePath", "id", id)["filePath"].ToString();
            
            RequestHandler.DownloadFile(filePath);
        }
        else
        {
            NoAccess.Create();
        }
    }

    public static void View()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.GetUserData();
        HttpRequest request = thread.HTTPContext.Request;
        
        string id = request.Query["id"];
        
        FileAuthenticator authenticator = new FileAuthenticator("id", id, "owner", thread.Session.SessionData.user);

        if (authenticator.AuthenticateGet())
        {
            // Get the file path
            string filePath = SQL.Get("Documents", "filePath", "id", id)["filePath"].ToString();
            
            RequestHandler.WriteFile(filePath);
        }
        else
        {
            NoAccess.Create();
        }
    }

    public static void Delete()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.GetUserData();
        
        string id = thread.HTTPContext.Request.Query["id"];
        
        FileAuthenticator authenticator = new FileAuthenticator("id", id, "owner", thread.Session.SessionData.user);
        
        if (authenticator.AuthenticateDelete())
        {
            string filePath = SQL.Get("Documents", "filePath", "id", id)["filePath"].ToString();
            
            if (SQL.Delete("Documents", authenticator, "id", id))
            {
                System.IO.File.Delete(filePath);
                thread.HTTPContext.Response.WriteAsync(BuildString.NewString("<script>",$"window.location = document.referrer;","</script>"));
            }
            else
            {
                NoAccess.Create();
            }
        }
        else
        {
            NoAccess.Create();
        }

    }
    
    private class File
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string ID { get; set; }
    }
}