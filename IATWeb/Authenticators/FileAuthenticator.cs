namespace IATWeb.Authenticators;

public class FileAuthenticator : Authenticator
{
    public FileAuthenticator(string column, string value, string ownerColumn, string ownerValue)
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
        return !SQL.Exists("Documents", column, value, ownerColumn, ownerValue);
    }
    
    public override bool AuthenticateDelete()
    {
        return SQL.Exists("Documents", column, value, ownerColumn, ownerValue);
    }
}