using Microsoft.AspNetCore.Http;

namespace IATWeb.Pages;

public static class LoginPage
{
    public static void Create(string username = "", params string[] errors)
    {
        HttpResponse response = ThreadConfig.GetWebThread().HTTPContext.Response;

        RequestHandler.WriteHeader();

        bool hasErrors = errors.Length > 0;
        
        string errorDiv = "";
        
        if (hasErrors)
        {
            errorDiv = BuildString.NewString(
                "                        <div class=\"ui error message\">",
                "                            <ul class=\"list\">"
            );
            
            foreach (string error in errors)
            {
                errorDiv += "                                <li>" + error + "</li>";
            }
            
            errorDiv += BuildString.NewString(
                "                            </ul>",
                "                        </div>"
            );
        }
        
        response.WriteAsync(BuildString.NewString(
                "<style>",
                "    body {",
                "        background-color: #DADADA;",
                "    }",
                "",
                "    body > .grid {",
                "        height: 100%;",
                "    }",
                "",
                "    .image {",
                "        margin-top: -100px;",
                "    }",
                "",
                "    .column {",
                "        max-width: min(450px, 90vw);",
                "    }",
                "</style>",
                "            <div class=\"ui middle aligned center aligned grid\">",
                "                <div class=\"column\">",
                "                    <h2 class=\"ui teal image header\">",
                "                        <img src=\"assets/images/logo.png\" class=\"image\">",
                "                        <div class=\"content\">",
                "                            Log-in to your account",
                "                        </div>",
                "                    </h2>",
                $"                    <form method=\"post\" action=\"/login\" class=\"ui large form {(hasErrors ? "error" : "")}\">",
                "                        <div class=\"ui stacked segment\">",
                $"                            <div class=\"field {(hasErrors ? "error" : "")}\">",
                "                                <div class=\"ui left icon input\">",
                "                                    <i class=\"user icon\"></i>",
                $"                                    <input type=\"text\" name=\"username\" value=\"{username}\" placeholder=\"Username\">",
                "                                </div>",
                "                            </div>",
                $"                            <div class=\"field {(hasErrors ? "error" : "")}\">",
                "                                <div class=\"ui left icon input\">",
                "                                    <i class=\"lock icon\"></i>",
                "                                    <input type=\"password\" name=\"password\" placeholder=\"Password\">",
                "                                </div>",
                "                            </div>",
                "                            <button class=\"ui fluid large teal submit button\">Login</button>",
                "                        </div>",errorDiv,
                "                    </form>",
                "                    <div class=\"ui message\">",
                "                        New to us? <a href=\"/register\">Sign Up</a>",
                "                    </div>",
                "                </div>",
                "            </div>"
            )
        );
        
        RequestHandler.WriteFooter();
    }
}