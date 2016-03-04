<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<System.Web.Mvc.HandleErrorInfo>" %>

<asp:Content ID="errorTitle" ContentPlaceHolderID="TitleContent" runat="server">
    <%= GetGlobalResourceObject("Common", "ErrorPageHeader").ToString()%>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="NavigationList" runat="server">
    <ul class="clearfix">
        <li><a href="../../Abiturient/Main"><%= GetGlobalResourceObject("Main", "PageHeader").ToString()%></a></li>
        <li class="active"><a><%= GetGlobalResourceObject("Common", "ErrorPageHeader").ToString()%></a></li>
    </ul>
</asp:Content>

<asp:Content ID="errorContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        <%= GetGlobalResourceObject("Common", "ErrorPageMessage").ToString()%>
    </h2>
    <br />
    <h4>
        Попробуйте выполнить запрос снова. <br />
        Если данная ошибка повторяется, напишите нам на электронную почту: <a href="mailto:abiturient@priem.pu.ru">abiturient@priem.pu.ru</a>, указав последовательность действий и скриншот последней страницы с введёнными данными.
    </h4>
</asp:Content>
