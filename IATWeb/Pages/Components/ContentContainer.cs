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
    public string GetStatString()
    {
        return ContentCard.GetString(HTMLContent);
    }
}

public static class ContentCard
{
    public static string GetString(string HTMLContent)
    {
        return BuildString.NewString(
            "        <div style=\"padding: 1rem;\" class=\"eight wide mobile four wide tablet three wide computer column\">",
                        HTMLContent,
            "        </div>"
        );
    }
}