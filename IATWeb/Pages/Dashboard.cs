using IATWeb.Pages.Components;
using Microsoft.AspNetCore.Http;

namespace IATWeb.Pages;

public static class Dashboard
{
    public static void Create(string messageTitle = "", string message = "")
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
                Value = SQL.Count("Requests", "status", "0").ToString(),
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
            // List of requests user accepted
            new ContentObject()
            {
                HTMLContent = List.Create(SQL.DoSearch("Requests", "*", "acceptedBy", thread.Session.SessionData.user, "status", 2), "", "", false, false, new Dictionary<string, string>()
                {
                    {"id", "ID"},
                    {"owner", "Eigenaar"},
                    {"pet", "Huisdier"},
                    {"startdate", "Startdatum"},
                    {"status", "Status"},
                    {"expectedDuration", "Verwachte duur"}
                }, new Dictionary<string, Type>()
                {
                    {"start", typeof(DateTime)},
                    {"end", typeof(DateTime)},
                    {"status", typeof(RequestStatus)}
                }, new Dictionary<string, ForeignKeyObject>()
                {
                    {"pet", new ForeignKeyObject(SQL.DoSearch("Animals", "id,name", "!owner", thread.Session.SessionData.user), "id", "name")}
                }, "", new(), "pet", "startdate", "expectedDuration"),
                Title = "Oppas geschiedenis",
            },
            new ContentObject()
            {
                HTMLContent = List.Create(SQL.DoSearch("Requests", "*", "status", 3, "owner", thread.Session.SessionData.user), "", "", false, false, new Dictionary<string, string>()
                {
                    {"name", "Naam"},
                    {"pet", "Huisdier"},
                    {"startdate", "Startdatum"},
                    {"enddate", "Einddatum"},
                    {"acceptedBy", "Geaccepteerd door"}
                }, new Dictionary<string, Type>(), new Dictionary<string, ForeignKeyObject>()
                {
                    {"pet", new ForeignKeyObject(SQL.DoSearch("Animals", "id,name", "owner", thread.Session.SessionData.user), "id", "name")}
                }, "", new()
                {
                    {"icon check green","Weet u zeker dat u deze aanvraag wilt accepteren?|acceptRequest"},
                    {"icon cancel red","Weet u zeker dat u deze aanvraag wilt weigeren?|denyRequest"}
                }, "pet", "startdate", "enddate", "acceptedBy"),
                Title = "Mijn aanvragen",
            },
            // List of requests of other users
            new ContentObject()
            {
                HTMLContent = List.Create(SQL.DoSearch("Requests", "*", "status", 0, "!owner", thread.Session.SessionData.user), "", "", false, false, new Dictionary<string, string>()
                {
                    {"name", "Naam"},
                    {"pet", "Huisdier"},
                    {"startdate", "Startdatum"},
                    {"enddate", "Einddatum"},
                    {"acceptedBy", "Geaccepteerd door"}
                }, new Dictionary<string, Type>(), new Dictionary<string, ForeignKeyObject>()
                {
                    {"pet", new ForeignKeyObject(SQL.DoSearch("Animals", "id,name", "!owner", thread.Session.SessionData.user), "id", "name")}
                }, "", new()
                {
                    {"icon check green","Weet u zeker dat u deze aanvraag wilt accepteren?|acceptPetRequest"},
                    {"icon user blue", "showPetProfile"}
                }, "pet", "startdate", "enddate", "acceptedBy"),
                Title = "Aanvragen van anderen",
            }
        };

        string messageObject = BuildString.NewString("<div class=\"ui message\">",
            "<i class=\"close icon\"></i>",
            "<div class=\"header\">",
            messageTitle,
            "</div>",
            $"<p>{message}</p>",
            "</div>");
        
        response.WriteAsync(BuildString.NewString(
            "<div class=\"ui padded relaxed grid\" id=\"content\">",
            string.IsNullOrEmpty(message) && string.IsNullOrEmpty(messageTitle) ? "" : messageObject,
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
        
        //     $('.message .close')
        //     .on('click', function() {
        //     $(this)
        //         .closest('.message')
        //         .transition('fade')
        //         ;
        // })
        // ;

        if (!string.IsNullOrEmpty(message) | !string.IsNullOrEmpty(messageTitle))
        {
            response.WriteAsync(BuildString.NewString(
                "<script>",
                "$('.message .close')",
                ".on('click', function() {",
                "$(this)",
                ".closest('.message')",
                ".transition('fade')",
                ";",
                "});",
                "</script>"
            ));
        };
        
        Sidebar.CloseSidebar();
    }
}