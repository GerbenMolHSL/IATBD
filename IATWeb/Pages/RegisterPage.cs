using IATWeb.Authenticators;
using IATWeb.Pages.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace IATWeb.Pages;

public static class RegisterPage
{
    public static void Create(params KeyValuePair<string, string>[] errors)
    { 
        HttpResponse response = ThreadConfig.GetWebThread().HTTPContext.Response;

        RequestHandler.WriteHeader();

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
                "                            Maak nieuw account aan",
                "                        </div>",
                "                    </h2>",
                "<div style=\"text-align: left;\" class=\"ui stacked segment\">",
                // Create edit for new user with List.Create
                List.CreateEdit(null, "Users", null, "/register/submit", "",new() { "name" }, new(), new(){ "id", "name", "psswrd", "psswrdRepeat" }, new Dictionary<string, string>()
                {
                    {"name", "Naam"},
                    {"psswrd", "Wachtwoord"},
                    {"psswrdRepeat", "Herhaal wachtwoord"},
                    { "id", "Gebruikersnaam"},
                },new Dictionary<string, Type>()
                {
                    {"psswrd", typeof(Password)},
                    {"psswrdRepeat", typeof(Password)},
                }, null, errors),
                "</div>",
                "                </div>",
                "            </div>"
            )
        );
        
        RequestHandler.WriteFooter();
    }

    public static void Submit()
    {
        WebThread thread = ThreadConfig.GetWebThread();
        
        AccountAuthenticator authenticator = new AccountAuthenticator("id", null, "id", null);

        if (string.IsNullOrEmpty(thread.HTTPContext.Request.Form["psswrd"]))
        {
            Create(new KeyValuePair<string, string>("psswrd", "Wachtwoord is Verplicht"));
            return;
        }
        
        if(thread.HTTPContext.Request.Form["psswrd"] != thread.HTTPContext.Request.Form["psswrdRepeat"])
        {
            Create(new KeyValuePair<string, string>("psswrd", "Wachtwoorden komen niet overeen"));
            return;
        }
        
        Dictionary<string, StringValues> form = new();
        
        foreach (var key in thread.HTTPContext.Request.Form.Keys)
        {
            form.Add(key, thread.HTTPContext.Request.Form[key]);
        }

        string hashedPassword = Helpers.GenerateSHA256(thread.HTTPContext.Request.Form["psswrd"]);
        form.Add("hshdPsswrd", hashedPassword);
        
        IFormCollection formCollection = new FormCollection(form);

        thread.HTTPContext.Request.Form = formCollection;

        if (SQL.InsertOrUpdateForm("Users", false, authenticator, out KeyValuePair<string,string>[] errors,null, new List<string>(){"id","name","hshdPsswrd"},"id","name","hshdPsswrd"))
        {
            thread.HTTPContext.Response.WriteAsync(BuildString.NewString("<script>",$"window.location.replace(\"/\");","</script>"));
        }
        else
        {
            Create(errors);
        }
    }
}