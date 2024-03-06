namespace IATWeb.Pages.Components;

public static class Errors
{
    public static string GenerateErrors(Dictionary<string, string> translations, KeyValuePair<string, string>[] errors)
    {
        string strReturn = "";

        bool hasErrors = errors.Length > 0;
        
        if (hasErrors)
        {
            strReturn = BuildString.NewString(
                "                        <div class=\"ui error message\">",
                "                            <ul class=\"list\">"
            );
            
            foreach (KeyValuePair<string, string> error in errors)
            {
                string value = error.Value;
                foreach(KeyValuePair<string, string> translation in translations)
                {
                    value = value.Replace(translation.Key, translation.Value);
                }
                strReturn += "                                <li>" + value + "</li>";
            }
            
            strReturn += BuildString.NewString(
                "                            </ul>",
                "                        </div>"
            );
        }
        
        return strReturn;
    }
}