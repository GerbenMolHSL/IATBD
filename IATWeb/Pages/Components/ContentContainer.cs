namespace IATWeb.Pages.Components;

public static class ContentContainer
{
    public static string GetString(ContentObject[] stats)
    {
        string statString = "";
        foreach (ContentObject stat in stats)
        {
            statString += stat.GetStatString();
        }

        return BuildString.NewString(
            "<div class=\"centered aligned row\">",
            statString,
            "</div>");
    }
}

public class ContentObject
{
    public string HTMLContent { get; set; }
    public string Title { get; set; }
    public string GetStatString()
    {
        return ContentCard.GetString(HTMLContent, Title);
    }
}

public static class ContentCard
{
    public static string GetString(string HTMLContent, string Title)
    {
        return BuildString.NewString(
            "        <div style=\"padding: 1rem;\" class=\"sixteen wide mobile sixteen wide tablet eight wide computer column\">",
            $"                <h2 class=\"ui header\">{Title}</h2>",
            HTMLContent,
            "        </div>"
        );
    }
}