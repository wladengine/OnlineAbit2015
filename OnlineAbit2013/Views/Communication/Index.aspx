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
   .wrapper
   {
       width: 1100px;
   }
   .grid_6
   {
       width: 1100px;
   }
   .first 
   {
       width: 1100px;
   }
   .YesNo
       {
           float:left; 
           width: 50px; 
           text-align:center;
       }
       .mini
       {
           padding: 2px;
           width: 45px;
       }
        .Checked
       {
           opacity: 1;
       }
       .NotChecked
       {
           opacity: 0.5;
       }
       td{
           text-align : center;
           padding: 3px;
           vertical-align: middle;
           min-height: 36px;
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

        function YesNoClick(id1, id2, Number) {
            var result;
            var obj1 = $('#' + id1 + "_" + Number);
            var obj2 = $('#' + id2 + "_" + Number);

            if (obj1.hasClass('Checked')) {
                obj1.removeClass("Checked");
                obj1.addClass("NotChecked");
                result = 0;
                obj2.removeClass("NotChecked");
                obj2.addClass("Checked");
            }
            else {
                obj1.removeClass("NotChecked");
                obj1.addClass("Checked");
                result = 1;
                obj2.removeClass("Checked");
                obj2.addClass("NotChecked");
            }
            $.post("../../Communication/ChangeBoolValue", { Barcode: Number, result: result, type: id1 }, function (data) { }, 'json');
        }
        function PrintList()
        {
            var val = $("#SortOrder").val();
            window.open('../../Communication/PrintListToPDF?sort='+val,'');
        }
        function PrintListXLS() {
            var val = $("#SortOrder").val();
            window.open('../../Communication/PrintListToXLS?sort=' + val, '');
        }
    </script>
    <div style="float:left; padding:5px;">
        <button value ="print" onclick ="PrintList()" class="button button-green"> Print as PDF</button>
    </div>
    <div style="padding:5px;">
        <button value ="print" onclick ="PrintListXLS()" class="button button-green"> Print (Interview=Y) as XLS</button>
    </div>
    <hr /> 
    <%=Html.HiddenFor(x=>x.SortOrder) %>
     <table style="width:100%;"> 
        <tr>
            <% List<string> Collst = new List<string>() {
                   "Number", "Applicant",  
                   "Is complete", "Portfolio Ru", "Portfolio De", "Portfolio Common", "Interview",
                   "Interview Ru", "Interview De", "Interviw Common", "Overall", "Status", "Print"
               };

               for (int i = 0; i < Collst.Count; i++)
               {
                   if ( i == Collst.Count - 1)
                   { %>
            <th ><a id= "<%=i.ToString()%>n" onclick="AddSortOrder(<%=i.ToString()%>, 'u' )" ><%=Collst[i]%></a>
            </th>
            <% }
                   else
                   { %>               
             <th ><a id= "<%=i.ToString()%>u" onclick="AddSortOrder(<%=i.ToString()%>, 'd' )" <%if (!Model.SortOrder.Contains("_" + i.ToString() + "u_"))
                                                                                                  {%>style="display:none;"<%} %> ><%=Collst[i]%> ▾</a>
                 <a id= "<%=i.ToString()%>d" onclick="AddSortOrder(<%=i.ToString()%>, 'n' )" <%if (!Model.SortOrder.Contains("_" + i.ToString() + "d_"))
                                                                                                 {%>style="display:none;"<%} %> ><%=Collst[i]%> ▴</a>
                 <a id= "<%=i.ToString()%>n" onclick="AddSortOrder(<%=i.ToString()%>, 'u' )" <%if (Model.SortOrder.Contains("_" + i.ToString() + "d_") || Model.SortOrder.Contains("_" + i.ToString() + "u_"))
                                                                                                 {%>style="display:none;"<%} %> ><%=Collst[i]%></a>
             </th>
            <%}
               } %>
        </tr>
<% int ind = 1;
    foreach (var x in Model.ApplicantList) {%>
    <tr>
        <td  style="text-align:left;"><%=x.Number %></td>
        <td  style="text-align:left;"><a href =<%=string.Format("../../Communication/ApplicantCard/{0}", x.Number.ToString()) %>><%=x.FIO %></a></td>
        <td><% if (x.isComplete) { %><img src="../../Content/themes/base/images/isComplete.png" alt="is complete" /><% }else{ %><img src="../../Content/themes/base/images/isNotComplete.png" alt="Is not complete" /><%} %></td>
        <td><%=x.PortfolioAssessmentRu %></td>
        <td><%=x.PortfolioAssessmentDe %></td>
        <td><%=x.PortfolioAssessmentCommon %></td>
        <td style="min-width: 120px;">
            <div id="Interview<%="_"+x.Number.ToString() %>" class="YesNo mini button button-blue  <%if (x.Interview) { %>Checked <%} else{%>NotChecked<%}%>" onclick="YesNoClick('Interview', 'Int', <%=x.Number.ToString() %>)"><%=GetGlobalResourceObject("Communication", "Yes")%></div>
            <div id="Int<%="_"+x.Number.ToString() %>" class="YesNo mini button button-blue  <%if (!x.Interview) { %>Checked <%} else{%>NotChecked<%}%>" onclick="YesNoClick('Interview', 'Int', <%=x.Number.ToString() %>)"><%=GetGlobalResourceObject("Communication", "No")%></div>
        </td>
        <td><%=x.InterviewAssessmentRu %></td>
        <td><%=x.InterviewAssessmentDe %></td>
        <td><%=x.InterviewAssessmentCommon %></td>
        <td><%=x.OverallResults %></td>
        <td><span title="<%=x.StatusAlt%>"><%=x.Status %></span></td>
        <td><div><button class="button button-green mini" onclick="<%= string.Format("window.open('/Communication/GetPrint?Barcode={0}','')", x.Number.ToString()) %>"/>Print</button></div></td>
    </tr>
<% ind++; } %>
    </table>


</asp:Content>
