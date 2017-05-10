<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Support/PersonalOffice.Master" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.SupportDialogList>" %>

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
            padding: 15px;
            border-bottom: 1px solid white;  
            color: #42648b;
        }
        .d_dialog:hover
        {
            background-color: #DFEDFE; 
        }
        .td_hasanswer
        {
            color: #5A5A5A;
        }
    </style>
    <script>
        $(function () {
            GetCountMessages();
            setInterval('GetCountMessages()', 3000);
        });

        function GetCountMessages() {
            $.post('/Support/GetCountMessages', function (res) {
                if (res.MyCnt == 0)
                    $('#my').text("Мои вопросы");
                else
                    $('#my').text("Мои вопросы (" + res.MyCnt + ")");
                if (res.NewCnt == 0)
                    $('#all').text("Новые вопросы");
                else
                    $('#all').text("Новые вопросы (" + res.NewCnt + ")");

                if (res.All == 0)
                    $('#mymesmenu').text('<%=GetGlobalResourceObject("Locales", "LeftMenuMessages").ToString()%>');
                else
                    $('#mymesmenu').text('<%=GetGlobalResourceObject("Locales", "LeftMenuMessages").ToString()%>' + " (" + res.All + ")");
            }, 'json');
        }
    function OpenDialog(id) {
            location.href = ("../../Support/dialog/"+id);
        }
    </script>
    <%Model.act = Request["act"];
     %>
        <a href="/Support/index?act=my"><div id="my" class ="button button-green">Мои вопросы</div></a>
    <a href="/Support/index?act=all"><div  id="all"  class ="button button-green">Новые вопросы</div></a>
 <br />
 <br />
    <%if (Model.act == "my"  || String.IsNullOrEmpty(Model.act)) { %><%=Html.Partial("MyDialogs", Model) %><%} %>
    <%if (Model.act == "all") { %><%=Html.Partial("AllDialogs", Model) %><%} %>

</asp:Content>
