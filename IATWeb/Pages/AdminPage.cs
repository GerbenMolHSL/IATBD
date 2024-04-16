using System.Data;
using IATWeb.Pages.Components;
using Microsoft.AspNetCore.Http;

namespace IATWeb.Pages;

public static class AdminPage
{
    public static void Create()
    {
        // <div class="ui top attached tabular menu">
        //     <div class="active item">Tab</div>
        //     </div>
        //     <div class="ui bottom attached active tab segment">
        //     <p></p>
        //     <p></p>
        //     </div>

        WebThread thread = ThreadConfig.GetWebThread();
        thread.Session.GetUserData();
        HttpResponse response = thread.HTTPContext.Response;
        
        Sidebar.Create("Admin");

        DataTable RequestData = SQL.DoSearch("Requests", "*");
        DataTable petFK = SQL.DoSearch("Animals", "*");

        DataTable UserData = SQL.DoSearch("Users", "id,name,isAdmin,active");
        
        string strHTML = "<div id=\"content\" class=\"ui main container\">";
        strHTML += "<div class=\"tabMenu\">";
        strHTML += "<div class=\"ui top attached tabular menu\">";
        strHTML += "<div name=\"oppas\" onclick=\"changeTabItem(event,'oppas')\" class=\"active item\">Oppasverzoeken</div>";
        strHTML += "<div name=\"gebruikers\" id=\"gebruikerButton\" onclick=\"changeTabItem(event,'gebruikers')\" class=\"item\">Gebruikers</div>";
        strHTML += "</div>";
        strHTML += "<div name=\"oppas\" class=\"ui active bottom attached tab segment\">";
        strHTML += List.Create(RequestData,"/admin/request","/admin/request/delete",true,false,new()
        {
            {"owner","Eigenaar"}
            ,{"pet","Huisdier"}
            ,{"startdate","Startdatum"}
            ,{"enddate","Einddatum"}
            ,{"status","Status"}
        },new()
        {
            {"status", typeof(RequestStatus)}
        },new ()
        {
            {"pet", new ForeignKeyObject(petFK,"id","name")}
        },"",new(),"owner","pet","startdate","enddate","status");
        strHTML += "</div>";
        strHTML += "<div name=\"gebruikers\" class=\"ui bottom attached tab segment\">";
        strHTML += List.Create(UserData,"/admin/user","",false,false,new()
        {
            {"name","Naam"},
            {"isAdmin","Is administrator"},
            {"active","Actief"}
        },new(),new(),"",new(),"name","isAdmin","active");
        strHTML += "</div>";
        strHTML += "</div>";
        strHTML += "</div>";
        
        response.WriteAsync(strHTML);
        
        Sidebar.CloseSidebar();
    }
}