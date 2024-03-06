using Microsoft.AspNetCore.Http;

namespace IATWeb.Pages.Components;

public static class Sidebar
{
    public static void Create(string header = "")
    {
        HttpResponse response = ThreadConfig.GetWebThread().HTTPContext.Response;

        response.WriteAsync(BuildString.NewString(
            "<div id=\"leftSideBar\" class=\"ui vertical inverted sidebar menu left\">",
            GetMenuItems(),
                "</div>"
            )
        );

        response.WriteAsync(BuildString.NewString(
                "<div class=\"ui fixed inverted main menu\">",
                "  <div class=\"ui container\" style=\"width:100%\">",
                "    <a class=\"launch icon item\" id=\"ToggleLeftSidebar\">",
                "      <i class=\"content icon\"></i>",
                "    </a>",
                "    <div class=\"item\">",
                "      PassenOpJeDier.nl",
                "    </div>",
                "    <div class=\"item\">",
                $"      {header}",
                "    </div>",
                "    <div class=\"right menu\">",
                "      <div class=\"vertically fitted borderless item\">",
                "        <a href=\"/logout\" class=\"icon item\">",
                "          <i class=\"logout icon\"></i>",
                "        </a>",
                "      </div>",
                "    </div>",
                "  </div>",
                "</div>"
            )
        );

        response.WriteAsync(BuildString.NewString(
                "<div class=\"pusher\">"
            )
        );
        
        response.WriteAsync(BuildString.NewString(
            "<script>",
            "$('.ui.left.sidebar').sidebar({",
            " transition: 'overlay'",
            " });",
            "",
            " $('.ui.left.sidebar')",
            ".sidebar('attach events', '#ToggleLeftSidebar');",
            "</script>"
        ));
    }
    
    public static void CloseSidebar()
    {
        HttpResponse response = ThreadConfig.GetWebThread().HTTPContext.Response;
        
        response.WriteAsync(BuildString.NewString(
                "</div>"
            )
        );
    }

    private static string GetMenuItems()
    {
        return BuildString.NewString(
            "    <a class=\"item\" href=\"/\">",
            "        Home",
            "    </a>",
            "    <a class=\"item\" href=\"/mijndieren\">",
            "        Mijn dieren",
            "    </a>",
            "    <a class=\"item\" href=\"/mijnaanvragen\">",
            "        Mijn aanvragen",
            "    </a>",
            "    <a class=\"item\" href=\"/vorigeoppassers\">",
            "        Vorige oppassers",
            "    </a>",
            "    <a class=\"item\" href=\"/mijnoppasgeschiedenis\">",
            "        Mijn oppasgeschiedenis",
            "    </a>"
        );
    }
}