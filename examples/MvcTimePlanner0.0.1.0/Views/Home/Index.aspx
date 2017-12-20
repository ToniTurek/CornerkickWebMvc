<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="indexTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Home Page | MVC Time Planner
</asp:Content>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Welcome to MVC Time Planner</h2>
    <p>
        <a href="http://mvctimeplanner.codeplex.com/">MVC Time Planner</a> is open-source time planning solution for personals 
        and groups (in the near future). I am building this software from my free time and therefore don't expect any 
        commercial development speeds from me. 
    </p>
    <p>
        Technical support is available through MVC Time Planner 
        <a href="http://mvctimeplanner.codeplex.com/Thread/List.aspx">discussion forums</a>. Bugs and feature requests can be 
        reported to <a href="http://mvctimeplanner.codeplex.com/workitem/list/basic">issue tracker</a>. If you want to contact 
        me personally then you can send me an e-mail (gpeipman [att] hotmail | com).
    </p>
    <p>
        With all best wishes,<br />
        Gunnar Peipman<br />
        ASP/ASP.NET MVP
    </p>
</asp:Content>
