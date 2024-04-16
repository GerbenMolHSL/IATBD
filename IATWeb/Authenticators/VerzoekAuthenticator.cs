namespace IATWeb.Authenticators;

public class VerzoekAuthenticator : Authenticator
{
    public VerzoekAuthenticator(string column, string value, string ownerColumn, string ownerValue)
    {
        this.column = column;
        this.value = value;
        this.ownerColumn = ownerColumn;
        this.ownerValue = ownerValue;
    }
    
    public override bool AuthenticateGet()
    {
        return true;
    }
    
    public override bool AuthenticatePost()
    {
        if (value == null) return true;
        return SQL.Exists("Requests", column, value, ownerColumn, ownerValue, "status", 3);
    }
    
    public override bool AuthenticateDelete()
    {
        return false;
    }
}