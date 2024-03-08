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

        DataTable animalFK = SQL.DoSearch("Animals", "*", "owner", thread.Session.SessionData.user);
        
        bool inAfwachtingFilter = thread.HTTPContext.Request.Query.ContainsKey("-status") && thread.HTTPContext.Request.Query["-status"].ToString() == "0";
        bool goedgekeurdFilter = thread.HTTPContext.Request.Query.ContainsKey("-status") && thread.HTTPContext.Request.Query["-status"].ToString() == "1";
        bool afgerondFilter = thread.HTTPContext.Request.Query.ContainsKey("-status") && thread.HTTPContext.Request.Query["-status"].ToString() == "2";

        string statusFilter = "2";
        if (goedgekeurdFilter) statusFilter = "1";
        if (afgerondFilter) statusFilter = "2";        
        
        DataTable data = SQL.DoSearch("Requests", "*", "acceptedBy", thread.Session.SessionData.user, "status", statusFilter);
        
        // String object with select <div class="field "><label for="pet">Huisdier</label><select data-required="True" name="pet" class="ui fluid search dropdown"><option value="4">Test</option><option value="5">Test2</option></select></div>
        string status = BuildString.NewString(
            "<div class=\"ui form\">",
            "<div class=\"field\">",
            "<label for=\"pet\">Huisdier</label>",
            "<select onchange=\"location.href='?-status='+event.target.value\" data-required=\"True\" name=\"pet\" class=\"ui fluid search dropdown\">",
            $"<option {(afgerondFilter ? "selected" : "")} value=\"2\">Afgerond</option>",
            $"<option {(goedgekeurdFilter ? "selected" : "")} value=\"1\">Goedgekeurd</option>",
            "</select>",
            "</div>",
            "</div>"
        );
        
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
                {"pet", new ForeignKeyObject(animalFK, "id", "name")}
            }, status, new(), "pet", "startdate", "enddate"),
            "</div>"
        ));
        
        Sidebar.CloseSidebar();
    }

}