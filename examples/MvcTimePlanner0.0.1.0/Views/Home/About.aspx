<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="aboutTitle" ContentPlaceHolderID="TitleContent" runat="server">
    About | MVC Time Planner
</asp:Content>

<asp:Content ID="aboutContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>About</h2>
    <p>MVC Time Planner is built using the following great pieces of software:</p>

    <ul>
    <li>Visual Studio 2010 Ultimate</li>
    <li>SQL Server 2008</li>
    <li>jQuery</li>
    <li>FullCalendar</li>
    <li>jQueryUI</li>
    <li>NHibernate</li>
    <li>Unity</li>
    </ul>
</asp:Content>
