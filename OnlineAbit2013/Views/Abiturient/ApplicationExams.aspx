<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Abiturient/PersonalOffice.Master" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.Mag_ApplicationExams>" %>

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

    function SetAppExam(i) {
        var CurExamBlock = '#_AppExam_' + i;
        var profId = $(CurExamBlock).val();
        $('#_selectedId_' + i).val(profId);
    }
</script>
<script type="text/javascript" src="../../Scripts/jquery-ui-1.8.11.js"></script>
     <div class="message info">
         Укажите экзамены по выбору для каждого заявления.
     </div>
    <form action="/Abiturient/ApplicationExamsSave" method="post">

        <%if (Model.ErrorMsg !=null) for (int i = 0; i< Model.ErrorMsg.Count(); i++){ %>
        <div class="error message"><%= Model.ErrorMsg[i]%></div>  
        <%} %>
    <%= Html.HiddenFor(x => x.CommitId) %>
    <%= Html.Hidden("ApplicationCount", Model.Applications.Count) %>
    <% for (int i = 0; i < Model.Applications.Count; i++ )
       {
           var app = Model.Applications[i];
        %>
        <%=Html.HiddenFor(x=>x.Applications[i].Id)%>
        <li class="message success" <% if (!app.Enabled){ %>style="opacity:0.5;" id = "cancelItems" title="заявление невозможно изменить"<%}%> >
            <table  class="nopadding" cellspacing="0" cellpadding="0">
                <tr>
                    <td style="width:12em"><%= GetGlobalResourceObject("NewApplication", "ApplicationLevel").ToString()%></td>
                    <td><%=Html.DisplayFor(x=>x.Applications[i].StudyLevelGroupName) %></td>
                </tr>
                <tr>
                    <td style="width:12em"><%= GetGlobalResourceObject("PriorityChangerForeign", "LicenseProgram").ToString()%></td>
                    <td><%=Html.DisplayFor(x=>x.Applications[i].Profession)%></td>
                </tr>
                <tr>
                    <td style="width:12em"><%= GetGlobalResourceObject("PriorityChangerForeign", "ObrazProgram").ToString()%></td>
                    <td><%=app.ObrazProgram%></td>
                </tr>
                <tr>
                    <td style="width:12em"><%= GetGlobalResourceObject("PriorityChangerForeign", "Profile").ToString()%></td>
                    <td><%=app.Specialization%></td>
                </tr>
                <% if (!String.IsNullOrEmpty(app.SemesterName)){%><tr>
                    <td style="width:12em"><%= GetGlobalResourceObject("NewApplication", "Semester").ToString()%></td>
                    <td><%=app.SemesterName%></td>
                </tr>
                <%} %>
                <% if (app.IsGosLine)
                   { %>
                    <tr>
                        <td style="width:12em"><%= GetGlobalResourceObject("NewApplication", "BlockData_GosLine").ToString()%></td>
                        <td><%= GetGlobalResourceObject("NewApplication", "Yes").ToString()%></td>
                    </tr>
                <% } %>
                <tr>
                    <td style="width:12em"><%= GetGlobalResourceObject("PriorityChangerForeign", "StudyForm").ToString()%></td>
                    <td><%=app.StudyForm%></td>
                </tr>
                <tr>
                    <td style="width:12em"><%= GetGlobalResourceObject("PriorityChangerForeign", "StudyBasis").ToString()%></td>
                    <td><%=app.StudyBasis%></td>
                </tr>
            </table>
            
            <% for (int j = 0; j < app.Exams.Count; j++ )
               {
                   var block = app.Exams[j]; 
                   %>
                   <%= Html.HiddenFor(x=>x.Applications[i].Exams[j].Id)%>
                    <b><%=Model.Applications[i].Exams[j].BlockName %></b>
                    <% if (!app.Enabled) { %>
                    <%=Html.DropDownListFor(x => x.Applications[i].Exams[j].SelectedExamInBlockId, Model.Applications[i].Exams[j].ExamInBlockList,
                    new { style = "width:659px;" , size = app.Exams[j].ExamInBlockList.Count, disabled = "disabled"})%>
                    <%} else {%>
                    <%=Html.DropDownListFor(x => x.Applications[i].Exams[j].SelectedExamInBlockId, Model.Applications[i].Exams[j].ExamInBlockList,
                    new { style = "width:659px;", size = app.Exams[j].ExamInBlockList.Count })%>
                    <% }%>
            <% } %>
        </li>
    <% } %>
    </ul>
    <button id="btnSave" type="submit" class="button button-green"><%= GetGlobalResourceObject("PriorityChangerForeign", "BtnSave").ToString()%></button><br />
</form>


</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="Subheader" runat="server">
    <h2><%= GetGlobalResourceObject("PriorityChangerForeign", "ManualExamTitle").ToString()%></h2>
</asp:Content>
