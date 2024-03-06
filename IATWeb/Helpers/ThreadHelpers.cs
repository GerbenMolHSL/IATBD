using System.Data;

namespace IATWeb;

using Microsoft.AspNetCore.Http;

public static class ThreadConfig
{
    public static IHttpContextAccessor HttpContextAccessor;

    public static WebThread GetWebThread()
    {
        return new WebThread(HttpContextAccessor.HttpContext);
    }
}

public class WebThread : IDisposable
{
    public HttpContext HTTPContext { get; private set; }
    public Session Session { get; private set; }
    
    void IDisposable.Dispose()
    {
        HTTPContext = null;
    }
    
    public WebThread(HttpContext context)
    {
        HTTPContext = context;
        Session = new Session(context);
    }
}

public class Session
{
    public Guid SessionID { get; private set; }
    public bool IsValid { get; private set; }

    private bool _sessionChecked = false;
    
    public UserProfile UserProfile { get; private set; }
    public SessionData SessionData { get; private set; }

    public Session(HttpContext context)
    {
        if (context.Request.Cookies.ContainsKey("shadowSessionIATBD") && Guid.TryParse(context.Request.Cookies["shadowSessionIATBD"], out Guid sessionID))
        {
            SessionID = sessionID;
        }
    }

    public bool CheckSession()
    {
        if (_sessionChecked) return IsValid;
        
        bool blnReturn = false;
        if (SessionID != null)
        {
            blnReturn = SQL.Exists("Sessions", "session", SessionID);
        }

        IsValid = blnReturn;
        _sessionChecked = true;

        return blnReturn;
    }

    public void GetUserData()
    {
        LoadSessionData();
        if(UserProfile == null)
        {
            UserProfile = new UserProfile();
            
            DataRow userProfileData = SQL.Get("Users", "id,name", "id",SessionData.user);
            UserProfile.Username = userProfileData["name"].ToString();
        }
    }

    public void LoadSessionData()
    {
        if(SessionData == null)
        {
            SessionData = new SessionData();
            DataRow sessionRow = SQL.Get("Sessions", "user", "session", SessionID);
            SessionData.user = sessionRow["user"].ToString();
        }
    }

    public Guid GenerateSession(string userID)
    {
        SessionID = Guid.NewGuid();
        SQL.Insert("Sessions", "session",SessionID, "user", userID);
        return SessionID;
    }
}

public class SessionData
{
    public string user { get; set; }
}

public class UserProfile
{
    public string Username { get; set; }
}