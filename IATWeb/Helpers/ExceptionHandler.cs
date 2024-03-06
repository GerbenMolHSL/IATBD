namespace IATWeb;

public static class ExceptionHandler
{
    public static void HandleError(Exception e)
    {
        throw new Exception(e.Message);   
    }
}