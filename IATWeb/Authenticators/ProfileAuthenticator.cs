namespace IATWeb.Authenticators;

public class ProfileAuthenticator : Authenticator
{
    public ProfileAuthenticator(string column, string value, string ownerColumn, string ownerValue)
    {
        this.column = column;
        this.value = value;
        this.ownerColumn = ownerColumn;
        this.ownerValue = ownerValue;
    }
    
    public override bool AuthenticateGet()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.LoadSessionData();
        return SQL.Exists("Users", column, value, ownerColumn, ownerValue, "active", true) && ownerValue == thread.Session.SessionData.user;
    }
    
    public override bool AuthenticatePost()
    {
        return true;
    }
    
    public override bool AuthenticateDelete()
    {
        return false;
    }
}