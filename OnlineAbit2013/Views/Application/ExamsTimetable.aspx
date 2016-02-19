<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Abiturient/PersonalOffice.Master" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.ApplicationExamsTimeTableModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <%= GetGlobalResourceObject("PriorityChangerForeign", "ManualExamTitle").ToString()%>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<style>
	#sortable { list-style-type: decimal; margin: 10px; padding: 10px; width: 90%; cursor: move; }
	#sortable li { margin: 0 5px 5px 5px; padding: 5px; font-size: 1.2em; /*height: 1.5em; */}
	html>body #sortable li { /*height: 1.5em; */line-height: 1.2em; }
	.ui-state-highlight { /*height: 1.5em; */line-height: 1.2em; }
</style>
<script>
    $(function () {
        $("#sortable").sortable({
            placeholder: "message warning"
        });
        $("#sortable").disableSelection();
    });
</script>
<script type="text/javascript" src="../../Scripts/jquery-ui-1.8.11.js"></script>
     <div class="message info">
         Регистрация на экзамены
     </div>
    <form action="/Application/ExamsTimetableSave?id=<%=Model.gCommId %>" method="post">
    <% foreach (var app in Model.lst) {%>
        <div>
        <h4><%=app.ExamInEntryBlockUnitName %></h4>
        <%foreach (var exam in app.lstTimeTable) { %>
        <div class ="info panel" style="margin:6px;">
            <input type="radio" value="<%=exam.Id%>" name ="app_<%=app.ExamInEntryBockUnitId.ToString()%>" <% if (app.SelectedTimeTableId == exam.Id) { %>checked ="checked"<% } %>/>
            <b><% = exam.ExamDate.ToString("dd.MM.yyyy HH:mm") %></b><br/>
            <% = exam.Address%><br/>
        </div>
        
    <% } %>
       </div>
    <% } %>
    <br/>
    <input id="btnSubmit" type="submit" value=<%= GetGlobalResourceObject("NewApplication", "btnSubmit")%> class="button button-green"/>
</form>


</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="Subheader" runat="server">
    <h2><%= GetGlobalResourceObject("PriorityChangerForeign", "ManualExamTitle").ToString()%></h2>
</asp:Content>
