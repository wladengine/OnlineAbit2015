<%@ Page Title="" Language="C#"Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.SupportDialogList>" %>

<% foreach (var d in Model.NewDialogList.OrderBy(x=>x.HasAnswer).ThenByDescending(x=>x.LastMes.time)) {
       string Date = d.LastMes.DateToWords(true);
       string HasAnswer = (!d.HasAnswer) ? "Вопрос ожидает обработки." : "Есть ответ.";
       string IsHasAnswer = !d.HasAnswer ? "waiting" : "done";
       string Photo = Model.Photolst.Where(p => p.UserId == d.LastMes.UserId).Select(p => p.Photo).FirstOrDefault();
      %>
<div class="d_dialog" onclick ="OpenDialog('<%=d.Id.ToString("N")%>')">
    <div class ="<%=IsHasAnswer%>"> 
             <table style="width: 580px;">
                 <tr><td ><b><%=d.Theme %></b></td>
                     <td rowspan ="2" class="td_photo">
                         <div class="d_photo">
                                    <%if (!String.IsNullOrEmpty(Photo)) { %> <img src="<%=Photo%>" /> <% }
                                 else {%> <img src="../../Content/themes/base/images/user_no_photo.png" alt="<%=GetGlobalResourceObject("Communication", "NoPhoto")%>" /><%} %>
                                </div></td>
                     <td class="td_author"><b><%=d.LastMes.Author%></b></td>
                 </tr>
                 <tr>
                     <td class="td_hasanswer"> <%=HasAnswer %></td>
                     <td><%=Date%></td>
                 </tr>
             </table>
     </div>
</div>
<%  } %> 
 