<%@ Page Title="" Language="C#"  Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.PartialModelDialog>" %>
<script>
    function CheckNewMessages() {
        var param = new Object();
        param['gId'] = $('#DialogId').val();
        $.post('/Inbox/CheckNewMessages', param, function (res) {
            for (i = 0 ; i < res.MyCnt.length; i++) {
                var msg = res.MyCnt[i];
                NewMsgShow(msg);
            }
            if (res.MyCnt.length > 0)
                $.post('/Inbox/SetIsRead', param, function (r) { }, 'json');
        }, 'json');
    }
    function NewMsgShow(msg) {
        var files = document.getElementById('d_dialog');
        var p = document.createElement("div");
        var s = "<table style='width:100%;' id='table_" + msg.Id + "'><tbody>"
        + "<tr><td rowspan ='3' class='td_photo'><div class='d_photo'><img src='" + msg.Photo + "'/></div></td>"
        + "<td><span class='s_author'><b>" + msg.Author + "</b></span> <span class='s_time'>" + msg.time + "</span></td></tr>"
        + "<tr><td colspan='2'>" + msg.Text + "</td></tr>";
        if (msg.Files.length > 0) {
            s += "<tr><td colspan='2'><hr style='width:150px;'/>"
            + "<div onclick=\"ShowFiles('" + msg.Id + "')\" class='mes_attach'>"
            + "<img src='../../Content/images/icons/attach.png'/>Прикреплены файлы</div></td></tr>";
        }
        s += "</tbody><tfoot style='display:none;'>";
        if (msg.Files.length > 0) {

            for (f = 0; f < msg.Files.length; f++) {
                s += "<tr><td></td><td><a href=\"../../Inbox/GetFile?id=" + msg.Files[i].Id + "\" target=\"_blank\">"
                + "<img src=\"../../Content/themes/base/images/downl1.png\" alt=\"Скачать файл\" />" + msg.Files[i].FileName + "</a>";
            }
        }
        s += "</tfoot></table><br />";
        p.innerHTML = s;
        files.appendChild(p);
        files.scrollTop = files.scrollHeight;
    }
</script>
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
             string Photo = Model.Photolst.Where(p => p.UserId == x.UserId).Select(p => p.Photo).FirstOrDefault();
             string Text = String.IsNullOrEmpty(x.Text) ? "" : x.Text.Replace("\r\n", "<br/>");
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
                                    <%if (!String.IsNullOrEmpty(Photo)) { %><img src="<%=Photo%>" /> <% }
                                 else {%> <img src="../../Content/themes/base/images/user_no_photo.png" alt="<%=GetGlobalResourceObject("Communication", "NoPhoto")%>" /><%} %>
                                </div>
                            </td>
                            <td><span class="s_author"><b><%=x.Author%></b></span><span class="s_time"> <%=x.time.ToShortTimeString()%></span></td>
                        </tr> 
                        <tr><td colspan="2"> <%=Text %> </td></tr>
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

