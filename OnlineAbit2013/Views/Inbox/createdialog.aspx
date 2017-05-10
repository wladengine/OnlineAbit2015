<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Abiturient/PersonalOffice.Master" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.NewDialog>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <%= GetGlobalResourceObject("Locales", "LeftMenuMessages")%>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Subheader" runat="server">
    <h2><%= GetGlobalResourceObject("Locales", "LeftMenuMessages")%></h2>
</asp:Content>

<asp:Content ID="HeaderScripts" ContentPlaceHolderID="HeaderScriptsContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <script>
        function AddFile()
        {
            var files = document.getElementById('files').tBodies[0];
            var p = document.createElement("tr");
            p.innerHTML = "<td><input id='Files' name='Files' type='file'/></td>";
            files.appendChild(p);
        }
        function CheckForm()
        {
            var ret = true;
            if ($('#Theme').val() == '') {
                ret = false;
                $('#Theme').addClass('input-validation-error'); 
            }
            else {
                $('#Theme').removeClass('input-validation-error');
            }
            if ($('#Text').val() == '') {
                ret = false;
                $('#Text').addClass('input-validation-error');
            }
            else {
                $('#Text').removeClass('input-validation-error');
            }
            return ret;
        }
        
    </script>
  <%if (!String.IsNullOrEmpty(Model.Error)) {%>
   <div class ="message error"><%=Model.Error %></div>
    <%} %>
  <form class="panel form" action="../../Inbox/SendQuestion" method="post" encType="multipart/form-data" onsubmit="return CheckForm();">
    <div class="clearfix">
        <%=Html.LabelFor(x=>x.Theme, "Введите тему вопроса:") %> 
        <%=Html.TextBoxFor(x=>x.Theme, new SortedList<string, object>() { { "class", "noresize" }, {"style", "width: 437px"} } ) %>
    </div>
    
    <div class="clearfix">
        <%=Html.LabelFor(x=>x.Text, "Введите вопрос:") %> 
        <%=Html.TextAreaFor(x=>x.Text, 5, 85, new SortedList<string, object>() { { "class", "noresize" }, {"style", "width: 437px"} } ) %>
    </div>
    <div class="clearfix">
        <%=Html.Label("Прикрепите файлы:") %> 
        <table id="files" >
            <tr>
                <td><%=Html.TextBoxFor(m => m.Files, new { type = "file", name = "Files" }) %>  </td>
            </tr>
        </table>
        <p onclick="AddFile()">Добавить еще файл...</p>
    </div> 
      <br /><br />
    <div class="clearfix">
        <input type="submit" value="Отправить" class="button button-blue"/>
    </div>
      </form>
</asp:Content>
