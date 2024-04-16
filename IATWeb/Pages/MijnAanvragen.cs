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
        
        bool inAfwachtingFilter = thread.HTTPContext.Request.Query.ContainsKey("-status") && thread.HTTPContext.Request.Query["-status"].ToString() == "0";
        bool goedgekeurdFilter = thread.HTTPContext.Request.Query.ContainsKey("-status") && thread.HTTPContext.Request.Query["-status"].ToString() == "1";
        bool afgerondFilter = thread.HTTPContext.Request.Query.ContainsKey("-status") && thread.HTTPContext.Request.Query["-status"].ToString() == "2";
        bool allowEdit = (thread.HTTPContext.Request.Query.ContainsKey("-status") && thread.HTTPContext.Request.Query["-status"].ToString() == "0") || !thread.HTTPContext.Request.Query.ContainsKey("-status");

        string statusFilter = "0";
        if (inAfwachtingFilter) statusFilter = "0";
        if (goedgekeurdFilter) statusFilter = "1";
        if (afgerondFilter) statusFilter = "2";

        string extraField1 = null;
        string extraField2 = null;
        if (goedgekeurdFilter || afgerondFilter)
        {
            extraField1 = "acceptedBy";
        }
        if (afgerondFilter)
        {
            extraField2 = "enddate";
        }
        
        DataTable data = SQL.DoSearch("Requests", "*", "owner", thread.Session.SessionData.user, "status", statusFilter);
        
        DataTable animalFK = SQL.DoSearch("Animals", "*", "owner", thread.Session.SessionData.user);
        
        // String object with select <div class="field "><label for="pet">Huisdier</label><select data-required="True" name="pet" class="ui fluid search dropdown"><option value="4">Test</option><option value="5">Test2</option></select></div>
        string status = BuildString.NewString(
            "<div class=\"ui form\">",
                        "<div class=\"field\">",
            "<label for=\"pet\">Status</label>",
            "<select onchange=\"location.href='?-status='+event.target.value\" data-required=\"True\" name=\"status\" class=\"ui fluid search dropdown\">",
            $"<option {(inAfwachtingFilter ? "selected" : "")} value=\"0\">In afwachting</option>",
            $"<option {(goedgekeurdFilter ? "selected" : "")} value=\"1\">Goedgekeurd</option>",
            $"<option {(afgerondFilter ? "selected" : "")} value=\"2\">Afgerond</option>",
            "</select>",
            "</div>",
            "</div>"
        );
        
        
        response.WriteAsync(BuildString.NewString("<div id=\"content\">",
            List.Create(data, allowEdit ? "/mijnaanvragen/edit" : "", "/mijnaanvragen/delete", allowEdit, allowEdit, new Dictionary<string, string>()
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
            },status, new(), "pet", "startdate", "expectedDuration", extraField1, extraField2),
            "</div>"
        ));
    }

    public static void CreateEdit(bool isAdminPage, params KeyValuePair<string,string>[] errors)
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        
        MijnAanvragenAuthenticator authenticator = new MijnAanvragenAuthenticator("id", thread.HTTPContext.Request.Query["id"], "owner", thread.Session.SessionData.user);
        
        if(!authenticator.AuthenticateGet() && !isAdminPage)
        {
            NoAccess.Create();
            return;
        }
        
        HttpResponse response = thread.HTTPContext.Response;
        
        Sidebar.Create("Mijn aanvragen");
        
        DataRow data = SQL.Get("Requests", "*", "id", thread.HTTPContext.Request.Query["id"], "status", 0);

        if (data == null && !string.IsNullOrEmpty(thread.HTTPContext.Request.Query["id"].ToString()) && !isAdminPage)
        {
            NoAccess.InProgress("/mijnaanvragen", "Deze aanvraag is al goedgekeurd");
            return;
        }
        
        DataTable animalFK = SQL.DoSearch("Animals", "*", "owner", thread.Session.SessionData.user);

        string strSubmitUrl = "submit";
        if (isAdminPage) strSubmitUrl = "request/submit";
        
        response.WriteAsync(BuildString.NewString("<div id=\"content\">",
            List.CreateEdit(data, "Requests", thread.HTTPContext.Request.Query["id"], strSubmitUrl, !isAdminPage ? "/mijnaanvragen" : "/admin",new() { "pet", "startdate", "expectedDuration" }, new(){ "id" }, new(){ "pet", "startdate", "expectedDuration" }, new Dictionary<string, string>()
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
        
        if (SQL.InsertOrUpdateForm("Requests", true, authenticator, out KeyValuePair<string,string>[] errors,null, new(){"pet","owner","startdate","enddte","expectedDuration"},"pet","owner","startdate","enddate","expectedDuration"))
        {
            string strReturnUrl = $"window.location.replace(\"edit?id={id}\");";
            if(isAdminPage) strReturnUrl = $"window.location.replace(\"/admin/request?id={id}\");";
            thread.HTTPContext.Response.WriteAsync(BuildString.NewString("<script>",strReturnUrl,"</script>"));
        }
        else
        {
            CreateEdit(thread.Session.UserProfile.IsAdmin,errors);
        }
    }
    
    public static void Delete(bool isAdminPage = false)
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        
        MijnAanvragenAuthenticator authenticator = new MijnAanvragenAuthenticator("id", thread.HTTPContext.Request.Query["id"], "owner", thread.Session.SessionData.user);
        
        if (SQL.Delete("Requests", authenticator, "id", thread.HTTPContext.Request.Query["id"]))
        {
            string strReturnUrl = $"window.location.replace(\"/mijnaanvragen\")";
            if(isAdminPage) strReturnUrl = $"window.location.replace(\"/admin\")";
            thread.HTTPContext.Response.WriteAsync(BuildString.NewString("<script>",strReturnUrl,"</script>"));
        }
        else
        {
            NoAccess.Create();
        }
    }
    
    public static void CreateProfile()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        
        HttpResponse response = thread.HTTPContext.Response;
        
        string id = thread.HTTPContext.Request.Query["id"];
        
        MijnAanvragenAuthenticator authenticator = new MijnAanvragenAuthenticator("id", id, "owner", thread.Session.SessionData.user);
        
        Sidebar.Create("Dieren profiel");
        
        if (!authenticator.AuthenticateGet())
        {
            NoAccess.Create();
            return;
        }

        DataRow Request = SQL.Get("Requests", "*", "id", id);
        
        string animalID = Request["pet"].ToString();
        
        DataRow data = SQL.Get("Animals", "*", "id", animalID);

        response.WriteAsync(BuildString.NewString("<div id=\"content\">",
                List.CreateEdit(data, "Animals", animalID, "", "/aanvragen",new(), new(){ "id", "name", "type", "payment", "Doc", "notes" }, new(){ "name", "type", "payment", "Doc", "notes" }, new Dictionary<string, string>()
                {
                    {"name", "Naam"},
                    {"type", "Soort"},
                    {"payment", "Uurprijs"},
                    {"Doc", "Foto's"},
                    {"notes", "Notities"}
                },new Dictionary<string, Type>()
                {
                    {"type", typeof(AnimalTypes)},
                    {"payment", typeof(decimal)},
                    {"Doc", typeof(File)},
                    {"notes", typeof(TextArea)}
                }, null, null),
                "</div>"
            )
        );
    }

    public static void AcceptPetRequest()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        
        // MijnAanvragenAuthenticator authenticator = new MijnAanvragenAuthenticator("id", thread.HTTPContext.Request.Query["id"], "owner", thread.Session.SessionData.user);
        bool allowPost = SQL.Exists("Requests", "id", thread.HTTPContext.Request.Query["id"], "status", 0);
        
        if (!allowPost)
        {
            NoAccess.Create();
            return;
        }

        List<object> values = new()
        {
            "status",
            3,
            "acceptedBy",
            thread.Session.SessionData.user
        };
        
        SQL.Update("Requests", values.ToArray(), "id", thread.HTTPContext.Request.Query["id"]);
        
        thread.HTTPContext.Response.WriteAsync(BuildString.NewString("<script>",$"location.href = document.referrer","</script>"));
    }
}