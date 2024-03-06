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
                Title = "Users",
                Value = "10",
            },
            new StatObject()
            {
                Title = "Users",
                Value = "10"
            },
            new StatObject()
            {
                Title = "Users",
                Value = "10"
            },
            new StatObject()
            {
                Title = "Users",
                Value = "10"
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
            "    </div>",
            "</div>"
            )
        );
        
        Sidebar.CloseSidebar();
    }
}