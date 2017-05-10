<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Support/PersonalOffice.Master" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.SupportOperator>" %>

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
   .d_photo {
        
   }
   .d_photo img  {
        width: 70px;
        height: 70px;  
        border-radius:50%;
   }
   .Selected{
       border: 3px solid #82CEF0;
   }
   .tr_photo
   {
       height: 100px; 
       vertical-align: middle;
   }
   .td_photo
   { 
       min-width: 100px;
       text-align: center;
       vertical-align:middle;
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
      function ChangeSelected(id)
      {
          var elem = document.getElementsByClassName("Selected");
          for (ind = 0; ind < elem.length; ++ind) {
              var x = elem[ind].id;
              $('#' + x).removeClass('Selected');
          }
          $('#' + id).addClass('Selected');
          var param = new Object();
          param['id'] = id;
          $.post('/Support/SaveOperatorProfilePhoto', param, function (res) {
          }, 'json');
      }
  </script> 
    <h4><%=Model.Name %></h4>
    <hr />
    <table>
    <%  int i = 0;
        int max = 4;
        foreach (var img in Model.Photolst.OrderBy(x=>x.Img))
        {
          string Selected =  img.Selected ? "Selected" : "";
          if (i % max == 0)
          {
              %><tr class="tr_photo"><%
          }
          %>
       <td class="td_photo">
    <div class="d_photo">
        <img  src="../../<%=img.Img%>" id="<%=img.Id.ToString()%>" class="<%=Selected%>" onclick="ChangeSelected('<%=img.Id%>')"/>
    </div>
           </td> 
          <%if (i % max == max-1)
          {
              %></tr><%
          }
          i++;
     } %>
    </table>
</asp:Content>
