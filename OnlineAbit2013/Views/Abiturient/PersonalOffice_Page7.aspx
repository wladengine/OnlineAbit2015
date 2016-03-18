<%@ Import Namespace="OnlineAbit2013" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Abiturient/PersonalOffice.Master" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.PersonalOffice>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <%= GetGlobalResourceObject("Main", "PersonalOfficeHeader").ToString()%>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<% if (1 == 0)/* типа затычка, чтобы VS видела скрипты */
   {
%>
    <script type="text/javascript" src="../../Scripts/jquery-1.5.1-vsdoc.js"></script>
    <script type="text/javascript" src="../../Scripts/jquery.validate-vsdoc.js"></script>
<% } %>
    <script type="text/javascript"> 
      
        $(function () {
            $('form').submit(function () { 
                var FZAgree = $('#AddInfo_FZ_152Agree').is(':checked'); 
                if (FZAgree) {
                    $('#FZ').hide();
                    return true&&AddInfoCheck() &&ContactPersonCheck();
                }
                else {
                    $('#FZ').show();
                    return false && AddInfoCheck() &&ContactPersonCheck();
                }  
            });
            $('#AddInfo_ExtraInfo').keyup(function (){ setTimeout(AddInfoCheck) });
            $('#AddInfo_ContactPerson').keyup(function() { setTimeout(ContactPersonCheck) });
            $('#AddInfo_ExtraInfo').change(function (){ setTimeout(AddInfoCheck) });
            $('#AddInfo_ContactPerson').change(function() { setTimeout(ContactPersonCheck) });
            $('#AddInfo_ExtraInfo').blur(function (){ setTimeout(AddInfoCheck) });
            $('#AddInfo_ContactPerson').blur(function() { setTimeout(ContactPersonCheck) });
        }); 

        function AddInfoCheck() {
            var ret = true;
            var val = $('#AddInfo_ExtraInfo').val(); 
            if (val.length > <%=Model.ConstInfo.AddInfo %>) { 
                var len = val.length-<%=Model.ConstInfo.AddInfo %> ;
                $('#AddInfo_ExtraInfo_Message').text('<%= GetGlobalResourceObject("PersonInfo", "MaxLengthLimitPart1").ToString()%> '+len+' <%= GetGlobalResourceObject("PersonInfo", "MaxLengthLimitPart2").ToString()%> ');
                $('#AddInfo_ExtraInfo_Message').show();
                $('#AddInfo_ExtraInfo').addClass('input-validation-error');
                ret = false;
            } 
            else{  
                if (val.length < <%=Model.ConstInfo.AddInfo %>-100) { 
                    $('#AddInfo_ExtraInfo_Message').hide();
                    $('#AddInfo_ExtraInfo').removeClass('input-validation-error'); 
                } else { 
                    var len = <%=Model.ConstInfo.AddInfo %> - val.length;
                    $('#AddInfo_ExtraInfo_Message').text(len+' <%= GetGlobalResourceObject("PersonInfo", "MaxLengthLimitPart3").ToString()%>');
                    $('#AddInfo_ExtraInfo_Message').show();  
                    $('#AddInfo_ExtraInfo').removeClass('input-validation-error'); 
                } 
            }
            return ret;
        } 
        function ContactPersonCheck() {
            var ret = true;
            var val = $('#AddInfo_ContactPerson').val(); 
            if (val.length > <%=Model.ConstInfo.Parents %>) { 
                var len = val.length-<%=Model.ConstInfo.Parents %> ;
                $('#AddInfo_ContactPerson_Message').text('<%= GetGlobalResourceObject("PersonInfo", "MaxLengthLimitPart1").ToString()%> '+len+' <%= GetGlobalResourceObject("PersonInfo", "MaxLengthLimitPart2").ToString()%>');
                $('#AddInfo_ContactPerson_Message').show();
                $('#AddInfo_ContactPerson').addClass('input-validation-error');
                ret = false;
            } 
            else{  
                if (val.length < <%=Model.ConstInfo.Parents %>-100) { 
                    $('#AddInfo_ContactPerson_Message').hide();
                    $('#AddInfo_ContactPerson').removeClass('input-validation-error'); 
                } else { 
                    var len = <%=Model.ConstInfo.AddInfo %> - val.length;
                    $('#AddInfo_ContactPerson_Message').text(len+' <%= GetGlobalResourceObject("PersonInfo", "MaxLengthLimitPart3").ToString()%>');
                    $('#AddInfo_ContactPerson_Message').show();  
                    $('#AddInfo_ContactPerson').removeClass('input-validation-error'); 
                } 
            } 
            return ret;
        } 
    </script>
    <div class="grid">
        <div class="wrapper">
            <div class="grid_4 first">
            <% if (!Model.Enabled)
                { %>
                <div id="Message" class="message warning">
                    <span class="ui-icon ui-icon-alert"></span><%= GetGlobalResourceObject("PersonInfo", "WarningMessagePersonLocked").ToString()%>
                </div>
            <% } %>
                <form class="panel form" action="Abiturient/NextStep" method="post">
                    <h3>7. <%= GetGlobalResourceObject("PersonalOffice_Step6", "ParentsPageHeader").ToString()%></h3>
                    <hr /><hr />

                    <%= Html.ValidationSummary() %>
                    <%= Html.HiddenFor(x => x.Stage) %>
                    <div class="clearfix">
                        <h4><%= GetGlobalResourceObject("PersonalOffice_Step6", "HostelHeader").ToString()%></h4>
                        <%= Html.CheckBoxFor(x => x.AddInfo.HostelAbit)%>
                        <span><%= GetGlobalResourceObject("PersonalOffice_Step6", "HostelAbit").ToString()%></span>
                    </div>
                    <div class="clearfix">
                        <%= Html.CheckBoxFor(x => x.AddInfo.HostelEduc)%>
                        <span><%= GetGlobalResourceObject("PersonalOffice_Step6", "HostelEduc").ToString()%></span>
                    </div>
                    <div class="clearfix">
                        <h4><%= GetGlobalResourceObject("PersonalOffice_Step6", "Privileges").ToString()%></h4>
                        <%= Html.CheckBoxFor(x => x.AddInfo.HasPrivileges) %>
                        <span><%= GetGlobalResourceObject("PersonalOffice_Step6", "PrivilegesFull").ToString()%></span>
                    </div>
                    <div class="clearfix">
                        <h4><%= GetGlobalResourceObject("PersonalOffice_Step6", "ContactPerson").ToString()%></h4>
                        <span><%= GetGlobalResourceObject("PersonalOffice_Step6", "ContactPerson_SubHeader").ToString()%></span><br />
                        <%= Html.TextAreaFor(x => x.AddInfo.ContactPerson, 5, 85, new SortedList<string, object>() { { "class", "noresize" }, {"style", "width: 437px"} }) %>
                        <br /><p></p>
                        <span id="AddInfo_ContactPerson_Message" class="Red" style="display:none"> 
                        </span>
                    </div>
                    <%if  (Model.AddInfo.VisibleParentBlock) { %>
                    <div> 
                       <h4><%= GetGlobalResourceObject("PersonalOffice_Step6", "Parent_Title").ToString()%></h4>
                       <div> 
                           <label for="Parent_Surname"><%= GetGlobalResourceObject("PersonalOffice_Step6", "Parent_Surname") %></label>
                           <%=Html.TextBoxFor(x=>x.AddInfo.Parent_Surname) %>
                       </div>
                       <div> 
                           <label for="Parent_Name"><%= GetGlobalResourceObject("PersonalOffice_Step6", "Parent_Name") %></label>
                           <%=Html.TextBoxFor(x=>x.AddInfo.Parent_Name) %>
                       </div>
                        <div> 
                           <label for="Parent_SecondName"><%= GetGlobalResourceObject("PersonalOffice_Step6", "Parent_SecondName") %></label>
                           <%=Html.TextBoxFor(x=>x.AddInfo.Parent_SecondName) %>
                       </div>
                       <div> 
                           <label for="Parent_Phone"><%= GetGlobalResourceObject("PersonalOffice_Step6", "Parent_Phone") %></label>
                           <%=Html.TextBoxFor(x=>x.AddInfo.Parent_Phone) %>
                       </div>
                       <div> 
                           <label for="Parent_Email"><%= GetGlobalResourceObject("PersonalOffice_Step6", "Parent_Email") %></label>
                           <%=Html.TextBoxFor(x=>x.AddInfo.Parent_Email) %>
                       </div>
                       <div> 
                           <label for="Parent_Work"><%= GetGlobalResourceObject("PersonalOffice_Step6", "Parent_Work") %></label>
                           <%=Html.TextBoxFor(x=>x.AddInfo.Parent_Work) %>
                       </div>
                       <div> 
                           <label for="Parent_WorkPosition"><%= GetGlobalResourceObject("PersonalOffice_Step6", "Parent_WorkPosition") %></label>
                           <%=Html.TextBoxFor(x=>x.AddInfo.Parent_WorkPosition) %>
                       </div>
                    </div>
                    <%} %>
                    <div class="clearfix">
                        <h4><%= GetGlobalResourceObject("PersonalOffice_Step6", "ExtraInfo").ToString()%></h4>
                        <%= Html.TextAreaFor(x => x.AddInfo.ExtraInfo, 5, 85, new SortedList<string, object>() { { "class", "noresize" }, {"style", "width: 437px"} })%>
                        <br /><p></p>
                        <span id="AddInfo_ExtraInfo_Message" class="Red" style="display:none"> 
                        </span>
                    </div>
                    <h4><%= GetGlobalResourceObject("PersonalOffice_Step6", "DocumentsReturn").ToString()%></h4>
                    <div class="clearfix">
                    <%
                        foreach (SelectListItem SLI in Model.AddInfo.ReturnDocumentTypeList)
                        { 
                            %><input type="radio" name="AddInfo.ReturnDocumentTypeId" value= <%="\""+SLI.Value+"\"" %>
                            <% if (SLI.Value==Model.AddInfo.ReturnDocumentTypeId) { %> checked="checked" <% } %>
                            /> <%=SLI.Text%> <br /><p></p> <%
                        }
                    %>
                    </div>
                    <div class="clearfix">
                        <h4><%= GetGlobalResourceObject("PersonalOffice_Step6", "FZ152_Header").ToString()%></h4>
                        <%= Html.CheckBoxFor(x => x.AddInfo.FZ_152Agree) %>
                        <span><%= GetGlobalResourceObject("PersonalOffice_Step6", "FZ_152Agree").ToString()%></span>    
                    </div>
                    <span id="FZ" class="Red" style="display:none;"><%= GetGlobalResourceObject("PersonalOffice_Step6", "FZ_152_Message").ToString()%></span>
                    <hr />
                    <div class="clearfix">
                        <input id="Submit5" class="button button-green" type="submit" value="<%= GetGlobalResourceObject("PersonalOffice_Step6", "btnValue_EndRegisration").ToString()%>" />
                    </div>
                </form>
            </div>
            <div class="grid_2">
                <ol>
                    <li><a href="../../Abiturient?step=1"><%= GetGlobalResourceObject("PersonInfo", "Step1")%></a></li>
                    <li><a href="../../Abiturient?step=2"><%= GetGlobalResourceObject("PersonInfo", "Step2")%></a></li>
                    <li><a href="../../Abiturient?step=3"><%= GetGlobalResourceObject("PersonInfo", "Step3")%></a></li>
                    <li><a href="../../Abiturient?step=4"><%= GetGlobalResourceObject("PersonInfo", "Step4")%></a></li>
                    <li><a href="../../Abiturient?step=5"><%= GetGlobalResourceObject("PersonInfo", "Step5")%></a></li>
                    <li><a href="../../Abiturient?step=6"><%= GetGlobalResourceObject("PersonInfo", "Step6")%></a></li>
                    <li><a href="../../Abiturient?step=7"><b><%= GetGlobalResourceObject("PersonInfo", "Step7")%></b></a></li>
                </ol>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderScriptsContent" runat="server">
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="Subheader" runat="server">
        <h2><%= GetGlobalResourceObject("PersonInfo", "QuestionnaireData")%></h2>
</asp:Content>
