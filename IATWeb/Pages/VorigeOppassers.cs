using System.Data;
using IATWeb.Pages.Components;
using Microsoft.AspNetCore.Http;

namespace IATWeb.Pages;

public static class VorigeOppassers
{
    public static void Create()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        HttpResponse response = thread.HTTPContext.Response;
        
        Sidebar.Create("Vorige oppassers");

        DataTable data = SQL.DoSearch("Requests", "*", "owner", thread.Session.SessionData.user, "status", 2);

        DataTable animalFK = SQL.DoSearch("Animals", "*", "owner", thread.Session.SessionData.user);
        DataTable userFK = SQL.DoSearch("Users", "id,name", "id", thread.Session.SessionData.user);
        
        response.WriteAsync(BuildString.NewString("<div id=\"content\">",
            List.Create(data, "", "", false, false, new Dictionary<string, string>()
            {
                {"id", "ID"},
                {"owner", "Eigenaar"},
                {"animal", "Dier"},
                {"start", "Start"},
                {"end", "Eind"},
                {"status", "Status"}
            },new Dictionary<string, Type>()
            {
                {"start", typeof(DateTime)},
                {"end", typeof(DateTime)},
                {"status", typeof(RequestStatus)}
            },new Dictionary<string, ForeignKeyObject>()
            {
                {"pet", new ForeignKeyObject(animalFK, "id", "name")},
                {"acceptedBy", new ForeignKeyObject(userFK, "id", "name")}
            }, "", "pet", "startdate", "enddate", "acceptedBy"),
            "</div>"
        ));
        
        Sidebar.CloseSidebar();
    }
}