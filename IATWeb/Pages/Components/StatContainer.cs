namespace IATWeb.Pages.Components;

public static class StatContainer
{
    public static string GetString(StatObject[] stats)
    {
        string statString = "";
        foreach (StatObject stat in stats)
        {
            statString += stat.GetStatString();
        }

        return BuildString.NewString(
            "<div class=\"centered aligned row\">",
            statString,
            "</div>");
    }
}

public class StatObject
{
    public string Title { get; set; }
    public string Value { get; set; }
    public string GetStatString()
    {
        return Statcard.GetString(Title, Value);
    }
}

public static class Statcard
{
    public static string GetString(string title, string value)
    {
        return BuildString.NewString(
            "        <div style=\"padding: 1rem;\" class=\"eight wide mobile two wide tablet two wide computer column\">",
            "<div class=\"stat-container\">",            
            $"    <div class=\"description\">{title}</div>",
            $"    <div class=\"stat\">{value}</div>",
            "</div>",
            "        </div>"
        );
    }
}