using IATWeb.Pages.Components;
using Microsoft.AspNetCore.Http;

namespace IATWeb.Pages;

public static class NoAccess
{
    public static void Create()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        HttpResponse response = thread.HTTPContext.Response;
        
        Sidebar.Create("Geen toegang");
        
        response.WriteAsync(BuildString.NewString(
            "<div id=\"content\">",
            "    <h1 class=\"ui header center aligned\">Je hebt geen toegang tot deze pagina</h1>",
            "</div>"
        ));
        
        Sidebar.CloseSidebar();
    }

    public static void InProgress(string backurl, string text)
    {
        WebThread thread = ThreadConfig.GetWebThread();
        HttpResponse response = thread.HTTPContext.Response;
        
        Sidebar.Create("Geen toegang");
        
        response.WriteAsync(BuildString.NewString(
            "<div id=\"content\">",
            "    <h1 class=\"ui header center aligned\">" + text + "</h1>",
            "    <a href=\"" + backurl + "\" class=\"ui button\">Terug</a>",
            "</div>"
        ));
        
        Sidebar.CloseSidebar();
    }
}