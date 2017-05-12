<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Abiturient/PersonalOffice.Master" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.Dialog>" %>

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
        .d_dialog {
            max-height: 305px;
            overflow: hidden; 
            overflow-y: auto;
            padding-top:10px;
        }
        #form_mes{
            height:auto;
        }
        .isread {
        } 
        td {
            padding-left:5px;
        }
        .td_photo
        {
            text-align: left;
            vertical-align : top;
            width: 60px;
        }
        .isread {
            background-color: white;
        }
        .d_photo { 
            width: 50px;
            height: 50px; 
            overflow: hidden;
        }
        .d_photo img  {
            height:100%;
            width:auto;
            border-radius:50%;
        }
        .td_time {
            text-align: left;

        }
        .td_author {
            height: 20px;
            width: auto;
        }
        .form{
            margin: 0px;
        }
        .dialog_header
        {
            border-bottom: 1px solid white;  
        }
 </style>
  <script> 
      $(function () {
         GetCountMessages();
         setInterval('CheckNewMessages()', 3000);
         var block = document.getElementById("d_dialog");
         block.scrollTop = block.scrollHeight;
      });
      function CheckNewMessages() {
          var param = new Object();
          param['gId'] = $('#DialogId').val();
          $.post('/Inbox/CheckNewMessages', param, function (res) {
              for (i = 0 ; i < res.MyCnt.length; i++) {
                  var msg = res.MyCnt[i];
                  NewMsgShow(msg);
              }
              if (res.MyCnt.length>0)
                $.post('/Inbox/SetIsRead', param, function (r) { }, 'json');
          }, 'json');
      } 
      function SendMessage()
      { 
          var param = new Object();
          param['dId'] = $('#DialogId').val();
          param['Text'] = $('#NewMessage').val();
          param['Files'] = $('#Files').val();
          $.post('/Inbox/SendMessage', param, function (res) { 
          }, 'json');
      }
      function ShowFiles(id)
      {
          $param = $('#table_' + id).find('tfoot');
          $param.toggle();
      }
      function CheckForm() {
          var ret = false;
          if ($('#NewMessage').val() == '') {
              var elem = document.getElementsByName("Files");
              for (index = 0; index < elem.length; ++index) {
                  if (elem[index].value != null && elem[index].value != '') {
                      ret = true;
                      break;
                  }
              }
          }
          if (!ret && $('#NewMessage').val() == '') {
              $('#NewMessage').addClass('input-validation-error');
          }
          else {
              ret = true;
              $('#NewMessage').removeClass('input-validation-error');
          }
          return ret;
      }
  </script>
   <div class="dialog_header">
       <h4><%=Model.Theme %></h4>
   </div>
   <div class="message error" id ="derrormsg" style="display:none;"> </div>
<div class ="d_dialog" id ="d_dialog"> 
    <%=Html.Partial("~/Views/support/message.aspx", Model.Partial) %>
</div> 
<form class="panel form" id="form_mes" action="../../Inbox/SendMessage" method="post" encType="multipart/form-data"  onsubmit="return CheckForm();">
    <%=Html.HiddenFor(x=>x.DialogId) %>
    <%=Html.Partial("~/Views/support/addmessage.aspx", Model) %>
</form>

</asp:Content>
