<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Abiturient/PersonalOffice.Master" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.InboxDialogList>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <%= GetGlobalResourceObject("Locales", "LeftMenuMessages")%>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Subheader" runat="server">
    <h2><%= GetGlobalResourceObject("Locales", "LeftMenuMessages")%></h2>
</asp:Content>

<asp:Content ID="HeaderScripts" ContentPlaceHolderID="HeaderScriptsContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .td_author
        {
            width: 80px;
            text-align: left;
            vertical-align : top;
        }
         .td_photo
        {
            width: 50px;
            text-align: left;
            vertical-align : top;
        }
        .d_photo { 
            width: 30px;
            height: 30px; 
            overflow: hidden;
            margin: 0px 5px;
        }
        .d_photo img  {
            height:100%;
            width:auto;
            border-radius:50%;
        }
        .waiting
        {
            background: url("../../Content/myimg/status_icons.png") left -32px no-repeat;
            background-position: left -32px;
            padding-left: 49px;
            height: 32px;  
        }
        .done {
            background: url("../../Content/myimg/status_icons.png") no-repeat 0 0;
            background-position: left 0;
            padding-left: 49px;
            height: 32px; 
        }
        .d_dialog
        {
            height: 30px;
            padding: 10px 0px 25px 10px ;
            border-bottom: 1px solid white;  
            color: #42648b;
        }
        .d_dialog:hover, .notread
        {
            background-color: #DFEDFE; 
        }
        .vert_al_mid
        {
            vertical-align:middle;
            color: #5A5A5A;
        }
    </style>
    <script>
    function OpenDialog(id) {
            location.href = ("../../Inbox/dialog/"+id);
        }
    function CreateDialog()
        {
            location.href = ("../../Inbox/createdialog");
        }
    </script>

<div class ="info message"> Вы можете задать вопрос оператору </div>
<input type="button" class="button button-green" value ="Задать новый вопрос" onclick ="CreateDialog()"/>
    <br />
    <br />
<% foreach (var d in Model.DialogList.OrderBy(x=>x.isRead).ThenByDescending(x=>x.LastMes.time)) {
       string Date;
       string IsHasAnswer = !d.HasAnswer ? "waiting" : "done";
       Date = d.LastMes.DateToWords(true);
       string IsRead = d.isRead ? "" : "notread";
       string Photo = Model.Photolst.Where(p => p.UserId == d.LastMes.UserId).Select(p => p.Photo).FirstOrDefault();
       string Text = d.LastMes.Text;
       if (String.IsNullOrEmpty(Text) && d.LastMes.HasFiles)
           Text = "<span>документ</span>";
           
      %>
    <div class="d_dialog <%=IsRead %>" onclick ="OpenDialog('<%=d.Id.ToString("N")%>')">
        <div class ="<%=IsHasAnswer%>"  >
                 <table style="width: 580px;">
                     <tr>
                             <td colspan="2">
                                 <div style="float:left;"><b><%=d.Theme %></b> </div>
                                 <div style="float:right;"><%=Date%></div>
                             </td>
                     </tr>
                     <tr>
                         <td style="width:50px;">
                             <div class="d_photo">
                                            <%if (!String.IsNullOrEmpty(Photo)) { %> <img src="<%=Photo%>" /> <% }
                                 else {%> <img src="../../Content/themes/base/images/user_no_photo.png" alt="<%=GetGlobalResourceObject("Communication", "NoPhoto")%>" /><%} %>
                                        </div>
                         </td>
                         <td class ="vert_al_mid"><%=Text%></td>
                     </tr>
                 </table>
        </div>
    </div>
    <%  } %>


</asp:Content>
