﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Communication/PersonalOffice.Master" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.GlobalCommunicationModelApplicantList>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <%= GetGlobalResourceObject("Locales", "AdminPanel")%>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Subheader" runat="server">
    <h2><%= GetGlobalResourceObject("Locales", "AdminPanel")%></h2>
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
            var val = $("#SortOrder").val() + "_" + ind + str + "_";
            var rf = $("#rfpriem").is(':checked');
            location.href = ("../../Communication/Index?sort=" + val + "&rf=" + rf);
        }
        function OpenCard(Num)
        {
            $("#Barcode").val(Num);
            document.fOpenCard.submit();
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
            var rf = $("#rfpriem").is(':checked');

            window.open('../../Communication/PrintListToPDF?sort=' + val + "&rf=" + rf, '');
        }
        function PrintListXLS() {
            var val = $("#SortOrder").val();
            var rf = $("#rfpriem").is(':checked');

            window.open('../../Communication/PrintListToXLS?sort=' + val + "&rf=" + rf, '');
        }
        function RFPriem()
        {
            var rf = $("#rfpriem").is(':checked');
            var val = $("#SortOrder").val();
            location.href = ("../../Communication/Index?sort=" + val+"&rf="+rf);
        }
    </script>
    <div style="float:left; padding:5px;">
        <button value ="print" onclick ="PrintList()" class="button button-green"> Print as PDF</button>
    </div>
    <div style="padding:5px;">
        <button value ="print" onclick ="PrintListXLS()" class="button button-green"> Print as XLS</button>
    </div>
    <hr /> 
    <input type ="checkbox" onchange ="RFPriem()" id ="rfpriem" <%if (Model.RFPriem) { %>checked<% } %> /> Равный прием (equal rights with Russian citizens)
    <form id="fOpenCard" name ="fOpenCard" action="../../Communication/ApplicantCard" method="post" >
    <%=Html.HiddenFor(x=>x.SortOrder) %>
    <%=Html.HiddenFor(x=>x.RFPriem) %>
    <%=Html.HiddenFor(x=>x.BarcodeList) %>
    <input type="hidden" id="Barcode" name="Barcode"/>
    </form>
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
            <th ><a id= "<%=(i+1).ToString()%>n" onclick="AddSortOrder(<%=(i+1).ToString()%>, 'u' )" ><%=Collst[i]%></a>
            </th>
            <% }
                   else
                   { %>               
             <th ><a id= "<%=(i+1).ToString()%>u" onclick="AddSortOrder(<%=(i+1).ToString()%>, 'd' )" <%if (!Model.SortOrder.Contains("_" + (i + 1).ToString() + "u_"))
                                                                                                  {%>style="display:none;"<%} %> ><%=Collst[i]%> ▾</a>
                 <a id= "<%=(i+1).ToString()%>d" onclick="AddSortOrder(<%=(i+1).ToString()%>, 'n' )" <%if (!Model.SortOrder.Contains("_" + (i + 1).ToString() + "d_"))
                                                                                                 {%>style="display:none;"<%} %> ><%=Collst[i]%> ▴</a>
                 <a id= "<%=(i+1).ToString()%>n" onclick="AddSortOrder(<%=(i+1).ToString()%>, 'u' )" <%if (Model.SortOrder.Contains("_" + (i + 1).ToString() + "d_") || Model.SortOrder.Contains("_" + (i + 1).ToString() + "u_"))
                                                                                                 {%>style="display:none;"<%} %> ><%=Collst[i]%></a>
             </th>
            <%}
               } %>
        </tr>
<% int ind = 1;
    foreach (var x in Model.ApplicantList) {%>
    <tr id="<%=x.Number.ToString()%>">
        <td  style="text-align:left;"><%=x.Number %></td>
        <td  style="text-align:left;"><a onclick="OpenCard(<%=x.Number.ToString()%>)"><%= x.FIO %></a></td>
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
