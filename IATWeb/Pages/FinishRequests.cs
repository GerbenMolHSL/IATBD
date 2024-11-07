using System.Data;
using IATWeb.Authenticators;
using IATWeb.Pages.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace IATWeb.Pages;

public static class FinishRequests
{
    public static void Create()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        HttpResponse response = thread.HTTPContext.Response;
        
        Sidebar.Create("Mijn aanvragen");
        
        DataTable data = SQL.DoSearch("Requests", "*", "acceptedBy", thread.Session.SessionData.user, "status", 1);
        
        DataTable animalFK = SQL.DoSearch("Animals", "*", "!owner", thread.Session.SessionData.user);
        
        
        
        response.WriteAsync(BuildString.NewString("<div id=\"content\">",
            List.Create(data, true ? "/finishRequest/edit" : "", "", false, false, new Dictionary<string, string>()
            {
                {"name", "Naam"},
                {"pet", "Huisdier"},
                {"startdate", "Startdatum"},
                {"enddate", "Einddatum"},
                {"acceptedBy", "Geaccepteerd door"},
                {"expectedDuration", "Verwachte duur"}
            },new Dictionary<string, Type>(),new Dictionary<string, ForeignKeyObject>()
            {
                {"pet", new ForeignKeyObject(animalFK, "id", "name")}
            },"", new(), "pet", "enddate"),
            "</div>"
        ));
    }

    public static void CreateEdit(bool isAdminPage, params KeyValuePair<string,string>[] errors)
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        
        MijnAanvragenAuthenticator authenticator = new MijnAanvragenAuthenticator("id", thread.Session.SessionData.user, "acceptedBy", thread.Session.SessionData.user);
        
        if(!authenticator.AuthenticateAccept() && !isAdminPage)
        {
            NoAccess.Create();
            return;
        }
        
        HttpResponse response = thread.HTTPContext.Response;
        
        Sidebar.Create("Voltooien");
        
        DataRow data = SQL.Get("Requests", "*", "id", thread.HTTPContext.Request.Query["id"], "status", 1);

        if (data == null && !string.IsNullOrEmpty(thread.HTTPContext.Request.Query["id"].ToString()) && !isAdminPage)
        {
            NoAccess.InProgress("/finishRequest", "Dit verzoek is al afgerond");
            return;
        }
        
        DataTable animalFK = SQL.DoSearch("Animals", "*", "!owner", thread.Session.SessionData.user);

        string strSubmitUrl = "submit";
        if (isAdminPage) strSubmitUrl = "request/submit";
        
        response.WriteAsync(BuildString.NewString("<div id=\"content\">",
            List.CreateEdit(data, "Requests", thread.HTTPContext.Request.Query["id"], strSubmitUrl, !isAdminPage ? "/mijnaanvragen" : "/admin",new() { "enddte" }, new(){ "id", "pet", "expectedDuration" }, new(){ "enddate", "expectedDuration" }, new Dictionary<string, string>()
            {
                {"name", "Naam"},
                {"pet", "Huisdier"},
                {"startdate", "Startdatum"},
                {"enddate", "Einddatum"},
                {"expectedDuration", "Verwachte duur"}
            }, new Dictionary<string, Type>()
            {
                {"startdate", typeof(DateTime)},
                {"enddate", typeof(DateTime)}
            }, new Dictionary<string, ForeignKeyObject>()
            {
                {"pet", new ForeignKeyObject(animalFK, "id", "name")}
            }, errors),
            "</div>"
        ));
    }

    public static void Submit(bool isAdminPage = false)
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        
        MijnAanvragenAuthenticator authenticator = new MijnAanvragenAuthenticator("id", thread.HTTPContext.Request.Query["id"], "acceptedBy", thread.Session.SessionData.user);
        
        Dictionary<string, StringValues> form = new();
        
        foreach (var key in thread.HTTPContext.Request.Form.Keys)
        {
            form.Add(key, thread.HTTPContext.Request.Form[key]);
        }

        form["owner"] = thread.Session.SessionData.user;

        IFormCollection formCollection = new FormCollection(form);
        
        thread.HTTPContext.Request.Form = formCollection;
        
        string id = thread.HTTPContext.Request.Form["id"];
        if (string.IsNullOrEmpty(id)) id = SQL.GetNewID("Requests");
        form["status"] = "2";
        
        if (SQL.InsertOrUpdateForm("Requests", true, authenticator, out KeyValuePair<string,string>[] errors,null, new(){"enddate"},"enddate","status"))
        {
            string strReturnUrl = $"window.location.replace(\"/finishRequest\");";
            thread.HTTPContext.Response.WriteAsync(BuildString.NewString("<script>",strReturnUrl,"</script>"));
        }
        else
        {
            CreateEdit(thread.Session.UserProfile.IsAdmin,errors);
        }
    }
}