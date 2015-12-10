<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="OnlineAbit2013.Models" %>
<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Abiturient/PersonalOffice.Master" Inherits="System.Web.Mvc.ViewPage<ApplicationModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <%= GetGlobalResourceObject("NewApplication", "PageTitle")%>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="Subheader" runat="server">
   <h2> <%= GetGlobalResourceObject("NewApplication", "PageSubheader")%></h2>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<script type="text/javascript">
    //$(document).ready(function () {
    function Submit1() {
        $('#val_h').val("1");
        document.forms['form'].submit();
    }
    function Submit2() {
        $('#val_h').val("2");
        document.forms['form'].submit();
    }
    function Submit3() {
        $('#val_h').val("3");
        document.forms['form'].submit();
    }
    function Submit4() {
        $('#val_h').val("4");
        document.forms['form'].submit();
    }
    function Submit5() {
        $('#val_h').val("5");
        document.forms['form'].submit();
    }
    function Submit6() {
        $('#val_h').val("6");
        document.forms['form'].submit();
    }
    function Submit7() {
        $('#val_h').val("7");
        document.forms['form'].submit();
    }
    function Submit8() {
        $('#val_h').val("8");
        document.forms['form'].submit();
    }
    function Submit9() {
        $('#val_h').val("9");
        document.forms['form'].submit();
    }
    function Submit10() {
        $('#val_h').val("10");
        document.forms['form'].submit();
    }
    function Submit11() {
        $('#val_h').val("11");
        document.forms['form'].submit();
    }
    //});
</script>
      
    <%= Html.ValidationSummary() %>
    <div class = "form panel">
    <%  if (Model.Applications.Count == 0)
        { %>
         <div class="message error">
        <b><%= GetGlobalResourceObject("NewApplication", "NoApplications")%></b>
        </div>
        <%}
        else
        {%>
    <h3><%= GetGlobalResourceObject("NewApplication", "ApplicationList")%></h3>
    <hr />
    <table class="paginate full">
        <thead>
            <tr>
                <th></th>
                <th><%= GetGlobalResourceObject("NewApplication", "ApplicationLevel")%></th>
                <%--<th>Тип поступления</th>--%>
                <th><%= GetGlobalResourceObject("NewApplication", "ApplicationView")%></th>
            </tr>
        </thead>
    <% foreach (OnlineAbit2013.Models.SimpleApplicationPackage app in Model.Applications.ToList())
       { %>
         <tr>
            <td><%if (app.isApproved){ %><span title ="Одобрено комиссией"><img src="../../Content/themes/base/images/isApproved.gif"/></span><%} %></td>
            <td style="vertical-align:middle; text-align:center;"><%= Html.Encode(app.StudyLevel)%></td>
            <%--<td style="vertical-align:middle; text-align:center;"><%= Html.Encode(app.PriemType)%></td>--%>
            <td style="vertical-align:middle; text-align:center;"><a href="<%= string.Format("../../Application/Index/{0}", app.Id.ToString("N")) %>"><%= GetGlobalResourceObject("NewApplication", "View")%></a></td>
         </tr>
     <% } %>
     </table>
     <% } %>
     </div>
    <br />

    <div class = "form panel"> 
    <h3><%= GetGlobalResourceObject("PersonStartPage", "AppSelectHeader")%></h3>
    <hr />
    
    <form id="form" method="post" action="/Abiturient/NewApplicationSelect"> 
    <input name="val_h" id="val_h" type="hidden" value="1" />
        <div class="message info">
            <% if (Model.VuzAddType == 2){
                if (!String.IsNullOrEmpty(Model.Message)) {%><b><%=Model.Message%></b><br /><%}%>
                <%= GetGlobalResourceObject("PersonalOffice_Step4", "CurrentLicenceProgram")%>: <%= Model.LicenseProgramName %><br />
                <%= GetGlobalResourceObject("PersonalOffice_Step4", "CurrentObrazProgram")%>: <%= Model.ObrazProgramName %> <br />
                <hr />
                <%} %>
            <%= GetGlobalResourceObject("NewApplication", "Educ_Mes2")%> 
        </div>
        <%if (Model.VuzAddType == 2) // перевод
          { %>
             <!-- Перевод ОСНОВА -->
            <input type="button" class="button button-blue" name="Val" onclick="Submit3()" style="width:45em;" value="<%= GetGlobalResourceObject("PersonStartPage", "AbiturientType3") %>" /><br /><br /> 
            <!-- смена образ программы -->
            <input type="button" class="button button-blue" name="Val" onclick="Submit7()" style="width:45em;" value="<%= GetGlobalResourceObject("PersonStartPage", "AbiturientType7") %>" /><br /><br />
       <%}
          else if (Model.VuzAddType == 4) // перевод
          {%> 
            <!-- Перевод -->
            <input type="button" class="button button-blue" name="Val" onclick="Submit4()" style="width:45em;" value="<%= GetGlobalResourceObject("PersonStartPage", "AbiturientType4") %>" /><br /><br />
          <%}
          else if (Model.VuzAddType == 3) // восстановление
          {%>
            <!-- восстановление -->
            <input type="button" class="button button-blue" name="Val" onclick="Submit5()" style="width:45em;" value="<%= GetGlobalResourceObject("PersonStartPage", "AbiturientType5") %>" /><br /><br />
       <% } %> 

       <% else
          { 
               if (Model.AbitTypeList.Contains(AbitType.AG))
              { %>
                <!--ag-->
                <input type="button" class="button button-blue" name="Val" onclick="Submit8()" style="width:45em; "value="<%= GetGlobalResourceObject("PersonStartPage", "AbiturientType8") %>" /><br /><br />
           <% } %>
           <% if (Model.AbitTypeList.Contains(AbitType.SPO)) //9
              {%>
                <!-- СПО -->     
                <input type="button" class="button button-blue" name="Val" onclick="Submit9()" style="width:45em;" value="<%= GetGlobalResourceObject("PersonStartPage", "AbiturientType9") %>" /><br /><br />
           <% }%>
           <%  if (Model.AbitTypeList.Contains(AbitType.FirstCourseBakSpec))  
              { %>
                <!-- 1 курс -->
                <input type="button" class="button button-blue" name="Val" onclick="Submit1()" style="width:45em;" value="<%= GetGlobalResourceObject("PersonStartPage", "AbiturientType1") %>" /><br /><br />
           <% } %>
           <%  if (Model.AbitTypeList.Contains(AbitType.Mag))  
              { %>
                <!-- Магистратура -->
                <input type="button" class="button button-blue" name="Val" onclick="Submit2()" style="width:45em;" value="<%= GetGlobalResourceObject("PersonStartPage", "AbiturientType2") %>" /><br /><br />
           <% } %>    
            <%  if (Model.AbitTypeList.Contains(AbitType.Aspirant))  
              { %>
                <!-- аспирантура -->
                <input type="button" class="button button-blue" name="Val" onclick="Submit10()" style="width:45em;" value="<%= GetGlobalResourceObject("PersonStartPage", "AbiturientType10") %>" /><br /><br />
           <% } %>    
             <%  if (Model.AbitTypeList.Contains(AbitType.Ord))  
              { %>
                <!-- ординатура -->
                <input type="button" class="button button-blue" name="Val" onclick="Submit11()" style="width:45em;" value="<%= GetGlobalResourceObject("PersonStartPage", "AbiturientType11") %>" /><br /><br />
           <% }
          } %>   
</form>
 </div>
</asp:Content>
