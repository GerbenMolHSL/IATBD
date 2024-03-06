namespace IATWeb.Authenticators;

abstract public class Authenticator
{
    public string column { get; set; }
    public string value { get; set; }
    public string ownerColumn { get; set; }
    public string ownerValue { get; set; }

    public abstract bool AuthenticatePost();
    public abstract bool AuthenticateGet();
    public abstract bool AuthenticateDelete();
}