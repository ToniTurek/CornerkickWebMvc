<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%
    if (Request.IsAuthenticated) {
%>
        Welcome <b><%= Html.Encode(Page.User.Identity.Name) %></b>!
        [ <a href="/Authentication/LiveIdHandler.ashx?action=logout">Log off</a> ]
<%
    }
    else {
%> 
        [ <a href="/Authentication/LiveIdHandler.ashx?action=login">Log on</a> ]
<%
    }
%>
