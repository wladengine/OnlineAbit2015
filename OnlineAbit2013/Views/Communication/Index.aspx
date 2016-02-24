<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Communication/PersonalOffice.Master" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.GlobalCommunicationModelApplicantList>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <%= GetGlobalResourceObject("ApplicationInfo", "Title")%>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Subheader" runat="server">
    <h2><%= GetGlobalResourceObject("ApplicationInfo", "Title")%></h2>
</asp:Content>

<asp:Content ID="HeaderScripts" ContentPlaceHolderID="HeaderScriptsContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<style> 
   .grid_2
   {
       width: 100px;
       /*display: none;*/
   }
   .wrapper
   {
       width: 1290px;
   }
   .grid_6
   {
       width: 1290px;
   }
   .first 
   {
       width: 1290px;
   }
</style>
    <table>
        <tr>
            <th>Show card</th>

            <th>Number</th>
            <th>Surname</th>
            <th>Name</th>
            <th>SecondName</th>
            <th>isComplete</th>
            <th>Portfolio Assessment Ru</th>
            <th>Portfolio Assessment De</th>
            <th>Portfolio Assessment Common</th>
            <th>Interview Assessment Ru</th>
            <th>Interview Assessment De</th>
            <th>Interview Assessment Common</th>
            <th>Overall Results</th>
            <th>Status</th>
            <th>Print</th>
        </tr>
<% int ind = 1;
    foreach (var x in Model.ApplicantList) {%>
    <tr>
        <td><a href =<%=string.Format("../../Communication/ApplicantCard/{0}", x.Number.ToString()) %>><img src="../../Content/themes/base/images/open.ico" alt="Скачать файл" /></a></td>
        <td><%=ind.ToString() %></td>
        <td><%=x.Surname %></td>
        <td><%=x.Name %></td>
        <td><%=x.SecondName %></td>
        <td><%=x.isComplete ? "Y" : "N" %></td>
        <td><%=x.PortfolioAssessmentRu %></td>
        <td><%=x.PortfolioAssessmentDe %></td>
        <td><%=x.PortfolioAssessmentCommon %></td>
        <td><%=x.InterviewAssessmentRu %></td>
        <td><%=x.InterviewAssessmentDe %></td>
        <td><%=x.InterviewAssessmentCommon %></td>
        <td><%=x.OverallResults %></td>
        <td><%=x.Status %></td>
        <td><button>Print</button></td>
    </tr>
<% ind++; } %>
    </table>


</asp:Content>
