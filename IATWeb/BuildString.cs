using System.Text;

namespace IATWeb;

public static class BuildString
{
    public static string NewString(params string[] strings)
    {
        StringBuilder sb = new StringBuilder();
        
        foreach (string str in strings)
        {
            sb.AppendLine(str);
        }

        return sb.ToString();
    }
}