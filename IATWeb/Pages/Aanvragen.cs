using System.Data;
using IATWeb.Pages.Components;
using Microsoft.AspNetCore.Http;

namespace IATWeb.Pages;

public static class Aanvragen
{
    public static void Create()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        HttpResponse response = thread.HTTPContext.Response;
        thread.Session.LoadSessionData();
        
        Sidebar.Create("Aanvragen");

        string dateFilter = thread.HTTPContext.Request.Query.ContainsKey("date") ? thread.HTTPContext.Request.Query["date"].ToString() : "";

        List<string> whereClause = new()
        {
            "!owner", thread.Session.UserProfile.Username, "status", "0"
        };
        
        if (!string.IsNullOrEmpty(dateFilter))
        {
            whereClause.Add(">startdate");
            whereClause.Add(dateFilter);
            // Add 1 day to the date filter
            whereClause.Add("<startdate");
            string nextDay = DateTime.Parse(dateFilter).AddDays(1).ToString("yyyy-MM-dd");
            whereClause.Add(nextDay);
        }
        
        DataTable data = SQL.DoSearch("Requests", "*", whereClause.ToArray());

        data.Select();
        
        // Date select met zoek knop
        string status = BuildString.NewString(
            "<div class=\"ui form\">",
            "<div class=\"field\">",
            "<label for=\"pet\">Datum</label>",
            $"<input type=\"date\" name=\"date\" value=\"{dateFilter}\" class=\"ui fluid search dropdown\">",
            "<button class=\"ui button\" onclick=\"location.href='?date='+document.querySelector('input[name=date]').value\">Zoek</button>",
            "</div>",
            "</div>"
        );
        
        response.WriteAsync(BuildString.NewString("<div id=\"content\">",status,
            List.Create(data, "", "", false, false, new Dictionary<string, string>()
            {
                {"name", "Naam"},
                {"pet", "Huisdier"},
                {"startdate", "Startdatum"},
                {"enddate", "Einddatum"},
                {"acceptedBy", "Geaccepteerd door"},
                {"expectedDuration", "Verwachte duur"}
            }, new Dictionary<string, Type>(), new Dictionary<string, ForeignKeyObject>()
            {
                {"pet", new ForeignKeyObject(SQL.DoSearch("Animals", "id,name", "!owner", thread.Session.UserProfile.Username), "id", "name")}
            }, "", new()
            {
                {"icon check green","Weet u zeker dat u deze aanvraag wilt accepteren?|acceptPetRequest"},
                {"icon user blue", "showPetProfile"}
            }, "pet", "startdate", "expectedDuration"),
            "</div>"
        ));
        
        Sidebar.CloseSidebar();
    }
}