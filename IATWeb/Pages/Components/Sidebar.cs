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
                "        <div id=\"menuDropdownItem\" class=\"ui dropdown item\">",
                "          <i class=\"user icon\"></i>",
                "          <div class=\"menu\">",
                "            <a class=\"item\" href=\"/profile\">",
                "              Profiel instellingen",
                "            </a>",
                "            <a class=\"item\" href=\"/logout\">",
                "              Logout",
                "            </a>",
                "          </div>",
                "        </div>",
                "    </div>",
                "  </div>",
                "</div>",
                "<script>",
                "$(\"#menuDropdownItem\").dropdown(\"hide\")",
                "</script>"
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
        WebThread webThread = ThreadConfig.GetWebThread();
        webThread.Session.GetUserData();

        string adminTabs = "";
        
        if(webThread.Session.UserProfile.IsAdmin)
        {
            adminTabs = BuildString.NewString(
                "    <a class=\"item\" href=\"/admin\">",
                "        Admin",
                "    </a>"
            );
        }
        
        return BuildString.NewString(
            "    <a class=\"item\" href=\"/\">",
            "        Home",
            "    </a>",
            "    <a class=\"item\" href=\"/aanvragen\">",
            "        Bekijk aanvragen",
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
            "    </a>",
            "    <a class=\"item\" href=\"/reviews\">",
            "        Reviews",
            "    </a>",
            "    <a class=\"item\" href=\"/myreviews\">",
            "        Mijn ratings",
            "    </a>",
            "    <a class=\"item\" href=\"/bestandsbeheer\">",
            "        Bestands beheer",
            "    </a>",
            
            adminTabs
        );
    }
}