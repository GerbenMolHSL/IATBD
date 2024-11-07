namespace IATWeb.Authenticators;

public class MijnAanvragenAuthenticator : Authenticator
{
    public MijnAanvragenAuthenticator(string column, string value, string ownerColumn, string ownerValue)
    {
        this.column = column;
        this.value = value;
        this.ownerColumn = ownerColumn;
        this.ownerValue = ownerValue;
    }

    public bool AuthenticateAccept()
    {
        if(value == null) return true;
        return SQL.Exists("Requests", ownerColumn, value, "status", 1);
    }
    
    public override bool AuthenticateGet()
    {
        if(value == null) return true;
        return SQL.Exists("Requests", column, value, "status", 0);
    }
    
    public override bool AuthenticatePost()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.GetUserData();

        if (thread.Session.UserProfile.IsAdmin) return true;
        
        if (value == null) return true;
        return SQL.Exists("Requests", column, value, ownerColumn, ownerValue, "status", 0);
    }
    
    public override bool AuthenticateDelete()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.GetUserData();

        if (thread.Session.UserProfile.IsAdmin) return true;
        
        return SQL.Exists("Requests", column, value, ownerColumn, ownerValue, "status", 0);
    }
}