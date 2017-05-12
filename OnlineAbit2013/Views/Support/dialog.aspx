<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Support/PersonalOffice.Master" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.SupportDialog>" %>

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
        .td_time {
            width: 120px;
            text-align: right;
        }
        .td_photo
        {
            text-align: left;
            vertical-align : top;
            width: 60px;
        }
         td {
            padding-left:5px;
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
        .d_dialog {
            max-height: 305px;
            overflow: hidden; 
            overflow-y: auto;
            padding-top:10px;
        }
        .form{
            margin: 0px;
        }
        .dialog_header
        {
            border-bottom: 1px solid white;  
        }
    </style>
  <script > 
      $(function () {
          GetCountMessages();
          setInterval('GetCountMessages()', 3000);
          setInterval('CheckNewMessages()', 3000);
          var block = document.getElementById("d_dialog");
          block.scrollTop = block.scrollHeight;
      });
      
      function GetCountMessages()
      {
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
          function SubmitQuestion()
          {
              var param = new Object();
              param['dId'] = $('#DialogId').val();
              $.post('/Support/SubmitQuestion', param, function (res) {
                  if (res.OK) {
                      $('#dialogform').show();
                      $('#buttonSubmit').hide();
                  }
                  else {
                      $('#derrormsg').show();
                      $('#derrormsg').text(res.Error);
                  }
              }, 'json');
          }
          function ShowFiles(id) {
              $param = $('#table_' + id).find('tfoot');
              $param.toggle();
          }
          function CheckForm() {
              var ret = false;
              if ($('#NewMessage').val() == '') {
                  var elem = document.getElementsByName("Files");
                  for (index = 0; index < elem.length; ++index) {
                      if (elem[index].value != null && elem[index].value != '' )  {
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
    <a href="/Support/index?act=my"><div id="my" class ="button button-green">Мои вопросы</div></a>
    <a href="/Support/index?act=all"><div  id="all"  class ="button button-green">Новые вопросы</div></a>
   
   <div class="dialog_header">
       <h4><%=Model.Theme %></h4>
   </div>
     <div class="message error" id ="derrormsg" style="display:none;"> </div>
      
    <div class ="d_dialog" id ="d_dialog"> 
        <%=Html.Partial("message", Model.Partial) %>
    </div> 
    <% if (Model.IsNew || Model.IsMine) {%>
    <% if (Model.IsNew) { %>
                <input type="button" id="buttonSubmit" class ="button button-green" value ="Принять" onclick ="SubmitQuestion()"/>
           <%} %> 
        <form  id="dialogform" class="panel form" action="../../Support/SendMessage" method="post" encType="multipart/form-data" <% if (Model.IsNew) { %> style="display:none;"<%} %> onsubmit="return CheckForm();">
            <%=Html.HiddenFor(x=>x.DialogId) %>
            <%=Html.Partial("addmessage", Model) %>
        </form>
      <%} %>
</asp:Content>
