<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Communication/PersonalOffice.Master" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.CommunicationStat>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <%= GetGlobalResourceObject("Communication", "Statisctics")%>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Subheader" runat="server">
    <h2><%= GetGlobalResourceObject("Communication", "Statisctics")%></h2>
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
   td{
       padding:3px;
   }
 </style>
    <table style="margin:2px;padding:5px;"> 
        <%foreach (var x in Model.columns ) {%>
        <tr>
            <td><%=x.Key.ToString() %></td>
            <td><%=x.Value.ToString() %></td>
        </tr>
        <%} %>
        
    </table>


</asp:Content>
