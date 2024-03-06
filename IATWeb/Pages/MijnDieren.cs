using System.Data;
using IATWeb.Pages.Components;
using Microsoft.AspNetCore.Http;

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
            List.Create(data, "/mijndieren/edit", true, new Dictionary<string, string>()
            {
                {"name", "Naam"},
                {"owner", "Eigenaar"}
            },"name", "owner"),
            "</div>"
        ));
        
        Sidebar.CloseSidebar();
    }

    public static void CreateEdit()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        
        if(!SQL.Exists("Animals", "id", thread.HTTPContext.Request.Query["id"]) || !SQL.Exists("Animals", "owner", thread.Session.SessionData.user))
        {
            NoAccess.Create();
            return;
        }
        
        HttpResponse response = thread.HTTPContext.Response;
        
        Sidebar.Create("Mijn dieren");
        
        DataRow data = SQL.Get("Animals", "*", "id", thread.HTTPContext.Request.Query["id"]);

        response.WriteAsync(BuildString.NewString("<div id=\"content\">",
            List.CreateEdit(data, "Animals", thread.HTTPContext.Request.Query["id"], "name", "/mijndieren",new() { "name" }, new(){ "id" }, new(){ "name" }, new Dictionary<string, string>()
            {
                {"name", "Naam"}
            }),
            "</div>"
            )
        );

        Sidebar.CloseSidebar();
    }
}