using System.Data;
using IATWeb.Authenticators;
using IATWeb.Pages.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace IATWeb.Pages;

public static class MijnAanvragen
{
    public static void Create()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        HttpResponse response = thread.HTTPContext.Response;
        
        Sidebar.Create("Mijn aanvragen");
        
        DataTable data = SQL.DoSearch("Requests", "*", "owner", thread.Session.SessionData.user);
        
        DataTable animalFK = SQL.DoSearch("Animals", "*", "owner", thread.Session.SessionData.user);
        
        response.WriteAsync(BuildString.NewString("<div id=\"content\">",
            List.Create(data, "/mijnaanvragen/edit", "/mijnaanvragen/delete", true, true, new Dictionary<string, string>()
            {
                {"name", "Naam"},
                {"pet", "Huisdier"},
                {"startdate", "Startdatum"},
                {"enddate", "Einddatum"}
            },new Dictionary<string, Type>(),new Dictionary<string, ForeignKeyObject>()
            {
                {"pet", new ForeignKeyObject(animalFK, "id", "name")}
            },"name", "pet", "startdate", "enddate"),
            "</div>"
        ));
    }

    public static void CreateEdit(params KeyValuePair<string,string>[] errors)
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        
        MijnAanvragenAuthenticator authenticator = new MijnAanvragenAuthenticator("id", thread.HTTPContext.Request.Query["id"], "owner", thread.Session.SessionData.user);
        
        if(!authenticator.AuthenticateGet())
        {
            NoAccess.Create();
            return;
        }
        
        HttpResponse response = thread.HTTPContext.Response;
        
        Sidebar.Create("Mijn aanvragen");
        
        DataRow data = SQL.Get("Requests", "*", "id", thread.HTTPContext.Request.Query["id"]);
        
        DataTable animalFK = SQL.DoSearch("Animals", "*", "owner", thread.Session.SessionData.user);

        response.WriteAsync(BuildString.NewString("<div id=\"content\">",
            List.CreateEdit(data, "Requests", thread.HTTPContext.Request.Query["id"], "submit", "/mijnaanvragen",new() { "pet", "startdate", "enddate" }, new(){ "id" }, new(){ "pet", "startdate", "enddate" }, new Dictionary<string, string>()
            {
                {"name", "Naam"},
                {"pet", "Huisdier"},
                {"startdate", "Startdatum"},
                {"enddate", "Einddatum"}
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

    public static void Submit()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        
        MijnAanvragenAuthenticator authenticator = new MijnAanvragenAuthenticator("id", thread.HTTPContext.Request.Query["id"], "owner", thread.Session.SessionData.user);
        
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
        
        if (SQL.InsertOrUpdateForm("Requests", authenticator, out KeyValuePair<string,string>[] errors,"pet","owner","startdate","enddate"))
        {
            thread.HTTPContext.Response.WriteAsync(BuildString.NewString("<script>",$"window.location.replace(\"edit?id={id}\");","</script>"));
        }
        else
        {
            CreateEdit(errors);
        }
    }
    
    public static void Delete()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        
        MijnAanvragenAuthenticator authenticator = new MijnAanvragenAuthenticator("id", thread.HTTPContext.Request.Query["id"], "owner", thread.Session.SessionData.user);
        
        if (SQL.Delete("Requests", authenticator, "id", thread.HTTPContext.Request.Query["id"]))
        {
            thread.HTTPContext.Response.WriteAsync(BuildString.NewString("<script>",$"window.location.replace(\"/mijnaanvragen\");","</script>"));
        }
        else
        {
            NoAccess.Create();
        }
    }
}