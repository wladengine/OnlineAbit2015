<%@ Page Title="" Language="C#"  Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.SupportDialog>" %>
<style>
    
    .newdate
    {
        text-align: center;
        font-size:12.5px;
        font-weight: 400;
        color: #828282;
    }
     .mes_attach img
    {
        vertical-align: middle;
    }
     .s_author{
            color: #42648b;
    }
    .s_time{
            color: #5A5A5A;
    }
</style>
<%
    DateTime? PrevDate = Model.Messages.FirstOrDefault().time.Date.AddDays(-1);
    foreach (var x in Model.Messages.OrderBy(x => x.time))
         {
             string Date = x.DateToWords(false);
             string iPhoto = Model.Photolst.Where(p => p.UserId == x.UserId).Select(p => p.imgPhoto).FirstOrDefault();
             string bPhoto = Model.Photolst.Where(p => p.UserId == x.UserId).Select(p => p.bPhoto).FirstOrDefault();
             if (!PrevDate.HasValue || PrevDate!= x.time.Date){
                   PrevDate = x.time.Date;
             %>
                <h5 class="newdate"><%=Date%></h5>
            <%} %>
            <div class="   "  >
                <table style="width:100%;" id="table_<%=x.Id.ToString("N")%>">
                     <tbody>
                         <tr>
                            <td rowspan ="3" class="td_photo">
                                <div class="d_photo">
                                    <%if (!String.IsNullOrEmpty(iPhoto)) { %> <img src="../../<%=iPhoto%>" /> <% }
                                else if (!String.IsNullOrEmpty(bPhoto)) { %> <img src="<% = String.Format("data:image/png;base64,{0}", bPhoto)%>" /> <% }
                                 else {%> <img src="../../Content/themes/base/images/user_no_photo.png" alt="<%=GetGlobalResourceObject("Communication", "NoPhoto")%>" /><%} %>
                                </div>
                            </td>
                            <td><span class="s_author"><b><%=x.Author%></b></span><span class="s_time"> <%=x.time.ToShortTimeString()%></span></td>
                        </tr> 
                        <tr><td colspan="2"> <%=x.Text %> </td></tr>
                    <% if (x.Files.Count>0){ %>
                         <tr><td colspan="2"><hr style="width:150px;"/>
                             <div onclick="ShowFiles('<%=x.Id.ToString("N")%>')" class="mes_attach">
                                <img src="../../Content/images/icons/attach.png" />
                                Прикреплены файлы
                             </div>
                             </td></tr>
                         <%} %>
                    <tfoot style="display:none;">
                        <% if (x.Files.Count>0) 
                              foreach (var f in x.Files) {%>
                        <tr><td></td><td> <a href="<%= "../../Inbox/GetFile?id=" + f.Id.ToString("N") %>" target="_blank"><img src="../../Content/themes/base/images/downl1.png" alt="Скачать файл" /><%=f.FileName %></a>
                        <%}%>
                    </tfoot>
                </table>
                 <br />
            </div>
       <%}  %>

