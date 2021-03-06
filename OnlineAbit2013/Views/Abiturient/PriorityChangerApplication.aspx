﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Abiturient/PersonalOffice.Master" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.PriorityChangerApplicationModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Смена приоритетов внутри конкурса
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
    <% if (!string.IsNullOrEmpty(Model.ErrorText)) { %>
    <div class="message error"><span style="font:bold"><%= Model.ErrorText %></span></div>
    <% } %>
    <% if (!string.IsNullOrEmpty(Model.MessageText)) { %>
    <div class="message info"><b><%= Model.MessageText %></b></div>
    <% } %>
    <a href="../Abiturient/PriorityChanger?ComId=<%= Model.CommitId.ToString("N") %>"><%= Model.CommitName %></a>
    -> 
    <span><%= GetGlobalResourceObject("PriorityChangerForeign", "ObrazPr_Message1").ToString() %></span>
    <p class="message info">
        <%= GetGlobalResourceObject("PriorityChangerForeign", "ObrazPr_Message2").ToString() %>
    </p>
    <form action="../Abiturient/PriorityChangeApplication" method="post">
        <%= Html.HiddenFor(x => x.ApplicationId) %>
        <%= Html.HiddenFor(x => x.ApplicationVersionId) %>
        <%= Html.HiddenFor(x => x.CommitId) %>
        <%= Html.HiddenFor(x => x.CommitName) %>
        <ul id="sortable">
        <% for (int i = 0; i < Model.lstInnerEntries.Count; i++) { %>
            <li class="message success">
                <table style="font-size:0.75em;" class="nopadding" cellspacing="5" cellpadding="5">
                    <tr>
                        <td style="width: 20em;"><%= GetGlobalResourceObject("PriorityChangerForeign", "ObrazProgram").ToString()%>: <%= Model.lstInnerEntries[i].Value.ObrazProgramName %></td>
                        <td><%= GetGlobalResourceObject("PriorityChangerForeign", "Profile").ToString()%>: <%= Model.lstInnerEntries[i].Value.ProfileName %></td>
                    </tr>
                </table>
                <input type="hidden" name="<%= Model.lstInnerEntries[i].Key.ToString("N") %>" />
            </li>
        <% } %>
        </ul>
        <button id="btnSave" type="submit" class="button button-green"><%= GetGlobalResourceObject("PriorityChangerForeign", "BtnSave").ToString()%></button><br />
    </form>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="Subheader" runat="server">
    <h2><%= GetGlobalResourceObject("PriorityChangerForeign", "ObrazPr_Message1").ToString()%></h2>
</asp:Content>
