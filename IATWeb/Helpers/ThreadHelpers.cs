using System.Data;

namespace IATWeb;

using Microsoft.AspNetCore.Http;

public static class ThreadConfig
{
    public static IHttpContextAccessor HttpContextAccessor;

    private static Dictionary<HttpContext, WebThread> _threads = new();

    public static WebThread GetWebThread()
    {
        if(_threads.ContainsKey(HttpContextAccessor.HttpContext))
        {
            return _threads[HttpContextAccessor.HttpContext];
        }
        WebThread thread = new WebThread(HttpContextAccessor.HttpContext);
        _threads.Add(HttpContextAccessor.HttpContext, thread);
        return thread;
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
            DataRow sessionRow = SQL.Get("Sessions", "session,user", "session", SessionID);
            blnReturn = sessionRow != null;

            if (blnReturn)
            {
                blnReturn = SQL.Exists("Users", "id", sessionRow["user"].ToString(), "active", true);
            }
        }

        IsValid = blnReturn;
        _sessionChecked = true;

        return blnReturn;
    }

    public bool CheckInactive()
    {
        bool blnReturn = true;

        if (SessionID != null)
        {
            DataRow sessionRow = SQL.Get("Sessions", "session,user", "session", SessionID);
            if (sessionRow != null)
            {
                blnReturn = SQL.Exists("Users", "id", sessionRow["user"].ToString(), "active", false);
            }
        }
        
        return blnReturn;
    }

    public void GetUserData()
    {
        LoadSessionData();
        if(UserProfile == null)
        {
            UserProfile = new UserProfile();
            
            DataRow userProfileData = SQL.Get("Users", "id,name,isAdmin", "id",SessionData.user);
            UserProfile.Username = userProfileData["name"].ToString();
            if(userProfileData["isAdmin"].GetType() != typeof(DBNull))UserProfile.IsAdmin = Convert.ToBoolean(userProfileData["isAdmin"]);
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
    public bool IsAdmin { get; set; }
}