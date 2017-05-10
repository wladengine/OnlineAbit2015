<%@ Page Title="" Language="C#"  Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.Dialog>" %>
        <%=Html.HiddenFor(x=>x.DialogId) %>
        <div class="clearfix">
            <%=Html.LabelFor(x=>x.NewMessage, "Введите сообщение") %>
            <%=Html.TextAreaFor(x=>x.NewMessage, 5, 85, new SortedList<string, object>() { { "class", "noresize" }, {"style", "width: 437px"} } ) %>
          </div>
          <br />
          <div class="clearfix">
            <%=Html.Label("Прикрепите файлы:") %> 
            <table id="files" >
                <tr>
                    <td><%=Html.TextBoxFor(m => m.Files, new { type = "file", name = "Files" }) %>  </td>
                </tr>
            </table>
            <p onclick="AddFile()">Добавить еще файл...</p>
        </div>  
        <br />
        <div class="clearfix">
            <input type="Submit" value="Отправить" class="button button-blue"/>
        </div>
