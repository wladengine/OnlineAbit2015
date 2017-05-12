<%@ Page Title="" Language="C#"  Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.Dialog>" %>
<script>
    function AddFile() {
        var files = document.getElementById('files').tBodies[0];
        var p = document.createElement("tr");
        p.innerHTML = "<td><input id='PartialNewMessage_Files' name='PartialNewMessage.Files' type='file'/></td>";
        files.appendChild(p);
    }
</script>
            
<div class="clearfix">
            <%=Html.LabelFor(x=>x.PartialNewMessage.NewMessage, "Введите сообщение") %>
            <%=Html.TextAreaFor(x=>x.PartialNewMessage.NewMessage, 5, 85, new SortedList<string, object>() { { "class", "noresize" }, {"style", "width: 437px"} } ) %>
          </div>
          <br />
          <div class="clearfix">
            <%=Html.Label("Прикрепите файлы:") %> 
            <table id="files" >
                <tr>
                    <td><%=Html.TextBoxFor(m => m.PartialNewMessage.Files, new { type = "file", name = "Files" }) %>  </td>
                </tr>
            </table>
            <p onclick="AddFile()">Добавить еще файл...</p>
        </div>  
        <br />
        <div class="clearfix">
            <input type="Submit" value="Отправить" class="button button-blue"/>
        </div>
