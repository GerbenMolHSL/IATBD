using System.Data;
using IATWeb.Authenticators;
using IATWeb.Pages.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace IATWeb.Pages;

public static class MijnDieren
{
    public static void Create()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        HttpResponse response = thread.HTTPContext.Response;
        
        Sidebar.Create("Mijn dieren");

        DataTable data = SQL.DoSearch("Animals", "*", "owner", thread.Session.SessionData.user);

        response.WriteAsync(BuildString.NewString("<div id=\"content\">",
            List.Create(data, "/mijndieren/edit", "/mijndieren/delete", true, true, new Dictionary<string, string>()
            {
                {"name", "Naam"},
                {"owner", "Eigenaar"},
                {"type", "Soort"},
                {"payment", "Uurprijs"}
            },new Dictionary<string, Type>()
            {
                {"type", typeof(AnimalTypes)},
                {"payment", typeof(decimal)}
            },new Dictionary<string, ForeignKeyObject>(), "", new(), "name", "owner", "type", "payment"),
            "</div>"
        ));
        
        Sidebar.CloseSidebar();
    }

    public static void CreateEdit(params KeyValuePair<string,string>[] errors)
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();

        MijnDierenAuthenticator authenticator = new MijnDierenAuthenticator("id", thread.HTTPContext.Request.Query["id"], "owner", thread.Session.SessionData.user);
        
        if(!authenticator.AuthenticateGet())
        {
            NoAccess.Create();
            return;
        }
        
        HttpResponse response = thread.HTTPContext.Response;
        
        Sidebar.Create("Mijn dieren");
        
        string pk = thread.HTTPContext.Request.Query["id"];

        DataRow data = SQL.Get("Animals", "*", "id", thread.HTTPContext.Request.Query["id"]);

        response.WriteAsync(BuildString.NewString("<div id=\"content\">",
            List.CreateEdit(data, "Animals", thread.HTTPContext.Request.Query["id"], "submit", "/mijndieren",new() { "name", "type", "payment" }, new(){ "id" }, new(){ "name", "type", "payment" }, new Dictionary<string, string>()
            {
                {"name", "Naam"},
                {"type", "Soort"},
                {"payment", "Uurprijs"}
            },new Dictionary<string, Type>()
            {
                {"type", typeof(AnimalTypes)},
                {"payment", typeof(decimal)}
            }, null, errors),
            "</div>"
            )
        );

        Sidebar.CloseSidebar();
    }

    public static void Submit()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();

        MijnDierenAuthenticator authenticator = new MijnDierenAuthenticator("id", thread.HTTPContext.Request.Form["id"], "owner", thread.Session.SessionData.user);

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
        
        if (SQL.InsertOrUpdateForm("Animals", authenticator, out KeyValuePair<string, string>[] errors, "name","owner","type","payment"))
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
        
        MijnDierenAuthenticator authenticator = new MijnDierenAuthenticator("id", thread.HTTPContext.Request.Query["id"], "owner", thread.Session.SessionData.user);
        
        if (SQL.Delete("Animals", authenticator, "id", thread.HTTPContext.Request.Query["id"]))
        {
            thread.HTTPContext.Response.WriteAsync(BuildString.NewString("<script>",$"window.location.replace(\"/mijndieren\");","</script>"));
        }
        else
        {
            NoAccess.Create();
        }
    }
}