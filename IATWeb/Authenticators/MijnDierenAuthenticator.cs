namespace IATWeb.Authenticators;

public class MijnDierenAuthenticator : Authenticator
{
    public MijnDierenAuthenticator(string column, string value, string ownerColumn, string ownerValue)
    {
        this.column = column;
        this.value = value;
        this.ownerColumn = ownerColumn;
        this.ownerValue = ownerValue;
    }
    
    public override bool AuthenticateGet()
    {
        if (value == null) return true;
        return SQL.Exists("Animals", column, value, ownerColumn, ownerValue);
    }

    public override bool AuthenticatePost()
    {
        if (value == null) return true;
        return SQL.Exists("Animals", column, value, ownerColumn, ownerValue);;
    }

    public override bool AuthenticateDelete()
    {
        return SQL.Exists("Animals", column, value, ownerColumn, ownerValue);
    }
}