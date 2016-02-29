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
   
    <script>
        function AddSortOrder(ind, str) {
            $("#" + ind + "d").hide();
            $("#" + ind + "u").hide();
            $("#" + ind + "n").hide();
            $("#" + ind + str).show();
            var val = $("#SortOrder").val() + "_"+ind + str+"_";
            //$("#SortOrder").val(val);
            location.href = ("../../Communication/Index?sort=" + val);
            }
    </script>
    <%=Html.HiddenFor(x=>x.SortOrder) %>
     <table style="width:100%;"> 
        <tr>
            <% List<string> Collst = new List<string>() {
                   "Number", "FIO",  
                   "IsComplete", "RuPort", "DePort", "ComPort", "Interv",
                   "RuInt", "DeInt", "ComInt", "Overall", "Status", "Print"
               };
               for (int i = 0; i<Collst.Count; i++){ %>
             <th><a id= "<%=i.ToString() %>u" onclick="AddSortOrder(<%=i.ToString() %>, 'd' )" <%if (!Model.SortOrder.Contains("_"+i.ToString()+"u_")) 
                                                                                                 {%>style="display:none;"<%} %> ><%=Collst[i] %> ▾</a>
                 <a id= "<%=i.ToString() %>d" onclick="AddSortOrder(<%=i.ToString() %>, 'n' )" <%if (!Model.SortOrder.Contains("_" + i.ToString() + "d_"))
                                                                                                 {%>style="display:none;"<%} %> ><%=Collst[i] %> ▴</a>
                 <a id= "<%=i.ToString() %>n" onclick="AddSortOrder(<%=i.ToString() %>, 'u' )" <%if (Model.SortOrder.Contains("_" + i.ToString() + "d_") || Model.SortOrder.Contains("_" + i.ToString() + "u_"))
                                                                                                 {%>style="display:none;"<%} %> ><%=Collst[i] %></a>
             </th>
            <%} %>
        </tr>
<% int ind = 1;
    foreach (var x in Model.ApplicantList) {%>
    <tr>
        <td><%=x.Number %></td>
        <td><a href =<%=string.Format("../../Communication/ApplicantCard/{0}", x.Number.ToString()) %>><%=x.FIO %></a></td>
        <td><%=x.isComplete ? "Y" : "N" %></td>
        <td><%=x.PortfolioAssessmentRu %></td>
        <td><%=x.PortfolioAssessmentDe %></td>
        <td><%=x.PortfolioAssessmentCommon %></td>
        <td><% = (x.Interview ? "Y":"N") %></td>
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
