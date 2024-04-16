using System.Data;
using IATWeb.Authenticators;
using IATWeb.Pages.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace IATWeb.Pages;

public class Profiel
{
    public static void Create(bool isAdminPage, params KeyValuePair<string,string>[] errors)
    {
        WebThread webThread = ThreadConfig.GetWebThread();
        webThread.Session.GetUserData();

        ProfileAuthenticator authenticator = new ProfileAuthenticator("id", webThread.Session.SessionData.user, "id", webThread.Session.SessionData.user);

        if(!authenticator.AuthenticateGet())
        {
            NoAccess.Create();
            return;
        }
        
        HttpResponse response = webThread.HTTPContext.Response;
        
        Sidebar.Create("Profiel instellingen");

        string strSearch = webThread.Session.SessionData.user;
        if (isAdminPage) strSearch = webThread.HTTPContext.Request.Query["id"];
        
        DataRow data = SQL.Get("Users", "*", "id", strSearch);
        
        // response.WriteAsync(BuildString.NewString("<div id=\"content\">",
        //         List.CreateEdit(data, "Animals", thread.HTTPContext.Request.Query["id"], "submit", "/mijndieren",new() { "name", "type", "payment" }, new(){ "id" }, new(){ "name", "type", "payment", "Doc" }, new Dictionary<string, string>()
        //         {
        //             {"name", "Naam"},
        //             {"type", "Soort"},
        //             {"payment", "Uurprijs"},
        //             {"Doc", "Foto's"}
        //         },new Dictionary<string, Type>()
        //         {
        //             {"type", typeof(AnimalTypes)},
        //             {"payment", typeof(decimal)},
        //             {"Doc", typeof(File)}
        //         }, null, errors),
        //         "</div>"
        //     )
        // );

        string strSubmitUrl = "/profile/submit";
        string strBackUrl = "/";
        if (isAdminPage)
        {
            strSubmitUrl = "/admin/user/submit";
            strBackUrl = "/admin";
        }

        List<string> showFields = new() { "name", "Doc" };

        if (isAdminPage)
        {
            showFields.Add("isAdmin");
            showFields.Add("active");
        }
        
        response.WriteAsync(BuildString.NewString("<div id=\"content\">",
            List.CreateEdit(data, "Users", strSearch, strSubmitUrl, strBackUrl, 
                new() { "name", "Doc" }, 
                new() { "id" },
                showFields,
                new()
                {
                    {"name", "Naam"},
                    {"Doc", "Foto's"},
                    {"isAdmin", "Is administrator"},
                    {"active", "Actief"}
                }, 
                new()
                {
                    {"Doc", typeof(File)},
                    {"isAdmin", typeof(bool)},
                    {"active", typeof(bool)}
                }, null, errors)));

        Sidebar.CloseSidebar();
    }
    
    public static void Submit(bool isAdminPage = false)
    {
        WebThread webThread = ThreadConfig.GetWebThread();
        webThread.Session.GetUserData();

        ProfileAuthenticator authenticator = new ProfileAuthenticator("id", webThread.Session.SessionData.user, "id", webThread.Session.SessionData.user);

        // Check if the user is authorized to submit the form
        if (!authenticator.AuthenticatePost())
        {
            NoAccess.Create();
            return;
        }

        // Retrieve form data from the request
        Dictionary<string, StringValues> form = new();
        foreach (var key in webThread.HTTPContext.Request.Form.Keys)
        {
            form.Add(key, webThread.HTTPContext.Request.Form[key]);
        }
        
        string strSearch = webThread.Session.SessionData.user;
        if (isAdminPage) strSearch = webThread.HTTPContext.Request.Query["id"];
        
        form.Add("id", strSearch);

        // Add additional form data if necessary
        // For example, you might want to add the user ID if it's not included in the form
        // form["id"] = webThread.Session.SessionData.user;

        // Construct a FormCollection from the form data
        IFormCollection formCollection = new FormCollection(form);

        List<string> insertFields = new()
        {
            "name",
            "Doc",
            "id"
        };

        if (isAdminPage)
        {
            insertFields.Add("isAdmin");
            insertFields.Add("active");
        }
        
        // Update or insert the form data into the database
        if (SQL.InsertOrUpdateForm("Users", true, authenticator, out KeyValuePair<string, string>[] errors, formCollection, new(){"name"},insertFields.ToArray()))
        {
            // Successful submission, redirect the user to a confirmation page or perform other actions
            // For example:
            string strBackUrl = "<script>window.location.replace('/profile');</script>";
            if (isAdminPage) strBackUrl = $"<script>window.location.replace('/admin/user?id={strSearch}');</script>";
            webThread.HTTPContext.Response.WriteAsync(strBackUrl);
        }
        else
        {
            // Submission failed due to errors, display the form again with error messages
            Create(webThread.Session.UserProfile.IsAdmin, errors);
        }
    }

}