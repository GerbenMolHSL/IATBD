using System.Data;
using IATWeb.Pages.Components;
using Microsoft.AspNetCore.Http;

namespace IATWeb.Pages;

public static class Reviews
{
    public static void Create()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        HttpResponse response = thread.HTTPContext.Response;
        
        Sidebar.Create("Review");

        DataTable data = SQL.DoSearch("Requests", "*", "owner", thread.Session.SessionData.user, "status", 2, "Review", null);

        response.WriteAsync(BuildString.NewString("<div id=\"content\">",
            List.Create(data, "/reviews/edit", "", false, false, new Dictionary<string, string>()
            {
                {"pet", "Dier"},
                {"startdate", "Startdatum"},
                {"enddate", "Einddatum"}
            },new Dictionary<string, Type>()
            {
                {"type", typeof(AnimalTypes)},
                {"payment", typeof(decimal)}
            },new Dictionary<string, ForeignKeyObject>(){
                {"pet", new ForeignKeyObject(SQL.DoSearch("Animals", "id,name", "owner", thread.Session.SessionData.user), "id", "name")}
            }, "", new(), "pet","startdate", "enddate"),
            "</div>"
        ));
        
        Sidebar.CloseSidebar();
    }

    public static void CreateEdit(params KeyValuePair<string,string>[] errors)
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        
        bool allowPost = SQL.Exists("Requests", "owner", thread.Session.SessionData.user, "status", 2, "id", thread.HTTPContext.Request.Query["id"], "review", null);
        
        if(!allowPost)
        {
            NoAccess.Create();
            return;
        }
        
        HttpResponse response = thread.HTTPContext.Response;
        
        Sidebar.Create("Review");
        
        DataRow data = SQL.Get("Requests", "*", "id", thread.HTTPContext.Request.Query["id"], "status", 2, "review", null);

        if (data == null && !string.IsNullOrEmpty(thread.HTTPContext.Request.Query["id"].ToString()))
        {
            NoAccess.InProgress("/mijnaanvragen", "Deze aanvraag is al gereviewed.");
            return;
        }
        
        DataTable animalFK = SQL.DoSearch("Animals", "*", "owner", thread.Session.SessionData.user);

        response.WriteAsync(BuildString.NewString("<div id=\"content\">",
            List.CreateEdit(data, "Requests", thread.HTTPContext.Request.Query["id"], "submit", "/reviews",new() { "review" }, new(){ "id" }, new(){ "review", "rating" }, new Dictionary<string, string>()
            {
                {"name", "Naam"},
                {"pet", "Huisdier"},
                {"startdate", "Startdatum"},
                {"enddate", "Einddatum"},
                {"expectedDuration", "Verwachte duur"},
                {"review", "Review"},
                {"rating", "Rating"}
            }, new Dictionary<string, Type>()
            {
                {"startdate", typeof(DateTime)},
                {"enddate", typeof(DateTime)},
                {"review", typeof(TextArea)},
                {"rating", typeof(Rating)}
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
        
        bool allowPost = SQL.Exists("Requests", "owner", thread.Session.SessionData.user, "status", 2, "id", thread.HTTPContext.Request.Query["id"].ToString(), "review", null);
        
        Sidebar.Create("Review");
        
        if(!allowPost)
        {
            NoAccess.Create();
            return;
        }
        
        // Check if all fields are filled in
        if (string.IsNullOrEmpty(thread.HTTPContext.Request.Form["review"].ToString()) || string.IsNullOrEmpty(thread.HTTPContext.Request.Form["rating"].ToString()))
        {
            CreateEdit(new KeyValuePair<string, string>("review", "Vul een review in"), new KeyValuePair<string, string>("rating", "Vul een rating in"));
            return;
        }
        
        List<object> values = new()
        {
            "Review",
            thread.HTTPContext.Request.Form["review"].ToString(),
            "Rating",
            thread.HTTPContext.Request.Form["rating"].ToString()
        };
        
        SQL.Update("Requests", values.ToArray(), "id", thread.HTTPContext.Request.Query["id"].ToString());
        
        thread.HTTPContext.Response.WriteAsync("<script>location.replace('/reviews')</script>");
        
        Sidebar.CloseSidebar();
    }

    public static void CreateMyRatings()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        HttpResponse response = thread.HTTPContext.Response;
        
        Sidebar.Create("Mijn ratings");
        
        DataTable data = SQL.DoSearch("Requests", "*", "acceptedBy", thread.Session.SessionData.user, "status", 2, "!Review", null);

        DataTable animalFK = SQL.DoSearch("Animals", "*", "!owner", thread.Session.SessionData.user);
        
        response.WriteAsync(BuildString.NewString("<div id=\"content\">",
            List.Create(data, "", "", false, false, new Dictionary<string, string>()
            {
                {"pet", "Dier"},
                {"startdate", "Startdatum"},
                {"enddate", "Einddatum"},
                {"rating", "Rating"},
                {"review", "Review"}
            },new Dictionary<string, Type>()
            {
                {"type", typeof(AnimalTypes)},
                {"payment", typeof(decimal)}
            },new Dictionary<string, ForeignKeyObject>()
            {
                {"pet", new ForeignKeyObject(animalFK, "id", "name")}
            }, "", new(), "pet","startdate", "enddate", "rating", "review"),
            "</div>"
        ));
    }
}