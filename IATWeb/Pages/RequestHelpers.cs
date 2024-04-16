using IATWeb.Authenticators;
using IATWeb.Pages.Components;
using Microsoft.AspNetCore.Http;

namespace IATWeb.Pages;

public static class RequestHelpers
{
    public static void AcceptRequest()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.GetUserData();
        
        string requestId = thread.HTTPContext.Request.Query["id"];
        
        VerzoekAuthenticator authenticator = new VerzoekAuthenticator("id", requestId, "owner", thread.Session.SessionData.user);

        if (!authenticator.AuthenticatePost())
        {
            Dashboard.Create("Verzoek niet geaccepteerd!","Misschien is het verzoek al geaccepteerd of u heeft geen toestemming om dit verzoek te accepteren.");
            return;
        }
        
        List<object> values = new();
        values.Add("status");
        values.Add("1");
        
        SQL.Update("Requests", values.ToArray(),"id", requestId);
        
            // <div class="ui message">
            // <i class="close icon"></i>
            // <div class="header">
            // Welcome back!
            // </div>
            // <p>This is a special notification which you can dismiss if you're bored with it.</p>
            // </div>
        
        Dashboard.Create("Verzoek geaccepteerd", "U heeft dit verzoek succesvol geaccepteerd!");
    }
    
    public static void DenyRequest()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.GetUserData();
        
        string requestId = thread.HTTPContext.Request.Query["id"];
        
        VerzoekAuthenticator authenticator = new VerzoekAuthenticator("id", requestId, "owner", thread.Session.SessionData.user);

        if (!authenticator.AuthenticatePost())
        {
            Dashboard.Create("Verzoek niet geaccepteerd!","Misschien is het verzoek al geaccepteerd of u heeft geen toestemming om dit verzoek te weigeren.");
            return;
        }
        
        List<object> values = new();
        values.Add("status");
        values.Add("0");
        
        SQL.Update("Requests", values.ToArray(),"id", requestId);
        
        // <div class="ui message">
        // <i class="close icon"></i>
        // <div class="header">
        // Welcome back!
        // </div>
        // <p>This is a special notification which you can dismiss if you're bored with it.</p>
        // </div>
        
        Dashboard.Create("Verzoek geweigerd!", "U heeft dit verzoek succesvol geweigerd!");
    }
}