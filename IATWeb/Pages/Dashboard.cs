using IATWeb.Pages.Components;
using Microsoft.AspNetCore.Http;

namespace IATWeb.Pages;

public static class Dashboard
{
    public static void Create()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.GetUserData();
        HttpResponse response = thread.HTTPContext.Response;
        
        Sidebar.Create();

        List<StatObject> stats = new()
        {
            new StatObject()
            {
                Title = "Openstaande oppasverzoeken",
                Value = SQL.Count("Requests", "status", "0", "owner", thread.Session.UserProfile.Username).ToString(),
            },
            new StatObject()
            {
                Title = "Aantal keer opgepast",
                Value = SQL.Count("Requests", "status", "2", "acceptedBy", thread.Session.UserProfile.Username).ToString(),
            },
            new StatObject()
            {
                Title = "Aantal keer oppasverzoeken afgerond",
                Value = SQL.Count("Requests", "status", "2", "owner", thread.Session.UserProfile.Username).ToString(),
            },
            new StatObject()
            {
                Title = "Totale oppasverzoeken",
                Value = SQL.Count("Requests", "owner", thread.Session.UserProfile.Username).ToString(),
            },
        };
        
        List<ContentObject> content = new()
        {
            new ContentObject()
            {
                HTMLContent = List.Create(SQL.DoSearch("Requests", "*", "status", 0, "owner", thread.Session.UserProfile.Username), "", "", false, false, new Dictionary<string, string>()
                {
                    {"name", "Naam"},
                    {"pet", "Huisdier"},
                    {"startdate", "Startdatum"},
                    {"enddate", "Einddatum"},
                    {"acceptedBy", "Geaccepteerd door"}
                }, new Dictionary<string, Type>(), new Dictionary<string, ForeignKeyObject>()
                {
                    {"pet", new ForeignKeyObject(SQL.DoSearch("Animals", "*", "owner", thread.Session.UserProfile.Username), "id", "name")}
                }, "", new()
                {
                    {"icon check green",""},
                    {"icon cancel red",""}
                }, "pet", "startdate", "enddate", "acceptedBy"),
            },
        };
        
        response.WriteAsync(BuildString.NewString(
            "<div class=\"ui padded relaxed grid\" id=\"content\">",
            "    <div class=\"row\">",
            "        <div class=\"column\">",
            $"            <h1 class=\"ui header center aligned\">Welkom, {thread.Session.UserProfile.Username}</h1>",
            "        </div>",
            "    </div>",
            StatContainer.GetString(stats.ToArray()),
            ContentContainer.GetString(content.ToArray()),
            "    </div>",
            "</div>"
            )
        );
        
        Sidebar.CloseSidebar();
    }
}