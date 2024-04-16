using System.Data;
using IATWeb.Pages.Components;
using Microsoft.AspNetCore.Http;

namespace IATWeb.Pages;

public static class OppasGeschiedenis
{
    public static void Create()
    {
        // Show the history of requests accepted by the user
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        
        HttpResponse response = thread.HTTPContext.Response;
        
        Sidebar.Create("Oppas geschiedenis");

        DataTable animalFK = SQL.DoSearch("Animals", "*", "!owner", thread.Session.SessionData.user);
        
        bool inAfwachtingFilter = thread.HTTPContext.Request.Query.ContainsKey("-status") && thread.HTTPContext.Request.Query["-status"].ToString() == "0";
        bool goedgekeurdFilter = thread.HTTPContext.Request.Query.ContainsKey("-status") && thread.HTTPContext.Request.Query["-status"].ToString() == "1";
        bool afgerondFilter = thread.HTTPContext.Request.Query.ContainsKey("-status") && thread.HTTPContext.Request.Query["-status"].ToString() == "2";
        bool actieVereistFilter = thread.HTTPContext.Request.Query.ContainsKey("-status") && thread.HTTPContext.Request.Query["-status"].ToString() == "3";

        string statusFilter = "2";
        if (goedgekeurdFilter) statusFilter = "1";
        if (afgerondFilter) statusFilter = "2";
        if (actieVereistFilter) statusFilter = "3";

        DataTable data = SQL.DoSearch("Requests", "*", "acceptedBy", thread.Session.SessionData.user, "status", statusFilter);
        
        // String object with select <div class="field "><label for="pet">Huisdier</label><select data-required="True" name="pet" class="ui fluid search dropdown"><option value="4">Test</option><option value="5">Test2</option></select></div>
        string status = BuildString.NewString(
            "<div class=\"ui form\">",
            "<div class=\"field\">",
            "<label for=\"pet\">Huisdier</label>",
            "<select onchange=\"location.href='?-status='+event.target.value\" data-required=\"True\" name=\"pet\" class=\"ui fluid search dropdown\">",
            $"<option {(afgerondFilter ? "selected" : "")} value=\"2\">Afgerond</option>",
            $"<option {(goedgekeurdFilter ? "selected" : "")} value=\"1\">Goedgekeurd</option>",
            $"<option {(actieVereistFilter ? "selected" : "")} value=\"3\">In afwachting</option>",
            "</select>",
            "</div>",
            "</div>"
        );

        List<string> showItems = new()
        {
            "pet", "startdate"
        };
        
        if(!afgerondFilter) showItems.Add("expectedDuration");
        else showItems.Add("enddate");
        
        response.WriteAsync(BuildString.NewString("<div id=\"content\">",
            List.Create(data, "", "", false, false, new Dictionary<string, string>()
            {
                {"id", "ID"},
                {"owner", "Eigenaar"},
                {"pet", "Dier"},
                {"startdate", "Start"},
                {"enddate", "Eind"},
                {"status", "Status"},
                {"acceptedBy", "Geaccepteerd door"},
                {"expectedDuration", "Verwachte duur"}
            },new Dictionary<string, Type>()
            {
                {"startdate", typeof(DateTime)},
                {"enddate", typeof(DateTime)},
                {"status", typeof(RequestStatus)}
            },new Dictionary<string, ForeignKeyObject>()
            {
                {"pet", new ForeignKeyObject(animalFK, "id", "name")}
            }, status, new(), showItems.ToArray()),
            "</div>"
        ));
        
        Sidebar.CloseSidebar();
    }

}