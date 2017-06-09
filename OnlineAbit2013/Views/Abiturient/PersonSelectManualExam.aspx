 <%@ Page Title="" Language="C#"  Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.PersonalOffice>" %>
 <script>
     function CheckCategory()
     {
         var Pass = $('#PassExamInSpbu').is(':checked');
         var Cat = $('input[name="AddEducationInfo.ManualExamInfo.PersonManualExamCategoryId"]:checked').val();
         if ((Pass && Cat > 0) || (!Pass)) {
             $('#sManualExamCategory').hide();
             return true;
         }
         else {
             $('#sManualExamCategory').show();
             return false;
         }
     }
     function ChangeVisibleManualExams() {
         $('#dManualExams').toggle();
     }
     function SetEnabled() {
         $('#AddManualExam').removeAttr('disabled');
     }
     function AddEgeManualExam()
     {
         var params = new Object();
         params['EgeExamId'] = $('#AddEducationInfo_ManualExamInfo_EgeExamId').val();
         $.post('Abiturient/AddEgeManualExam', params, function (res) {
             if (res.IsOk) {
                 $('#tblManualExams').show();
                 var info = '<tr id="manualexam_' + res.Data.Id + '">';
                 info += '<td>' + res.Data.Name + '</td>'; 
                 info += '<td><span class="link" onclick="DeleteEgeManualExam(\'' + res.Data.Id + '\')"><img src="../../Content/themes/base/images/delete-icon.png" alt="Удалить" /></span></td>';
                 info += "</tr>";
                 $('#tblManualExams tbody').append(info);
             }
             else {
                 alert(res.ErrorMsg);
             }
         }, 'json');
     }
     function DeleteEgeManualExam(ExamId) {
         var params = new Object();
         params['id'] = ExamId;
         $.post('Abiturient/DeleteEgeManualExam', params, function (res) {
             if (res.IsOk) {
                 $('#manualexam_'+ExamId).hide();
             }
             else {
                 alert(res.ErrorMsg);
             }
         }, 'json');
     }
 </script>
<div>
    <div  class="clearfix">
        <%=Html.CheckBoxFor(x=>Model.AddEducationInfo.ManualExamInfo.PassExamInSpbu, new SortedDictionary<string, object>() {{"Id","PassExamInSpbu"}, { "onchange", "ChangeVisibleManualExams()" } })%>
        <!--<%=GetGlobalResourceObject("PersonalOffice_Step4", "HasAccreditation").ToString() %>-->
        Планирую сдавать экзамены в СПбГУ
        <br />
    </div>
    <div id ="dManualExams" <% if (!Model.AddEducationInfo.ManualExamInfo.PassExamInSpbu)
                               { %>style="display:none;"<%} %>> 
        <div  class="clearfix">
            Как лицо, имеющее право сдавать вступительное испытание в СПбГУ в соответствии с пунктом 7.2.2 Правил приема:
            <br />
            <% foreach (var item in Model.AddEducationInfo.ManualExamInfo.PersonManualExamCategory)
               { %> 
                     <input type="radio" name="AddEducationInfo.ManualExamInfo.PersonManualExamCategoryId" value="<%=item.Value%>" <%if (item.Selected) {%>checked="checked" <%} %>/><%=item.Text%>
                     <br /> 
                     <br /> 
            <% }  %>
            <div class="message error" style="display:none" id="sManualExamCategory">
                <span class="Red">
                    Выберите категорию
                </span>
            </div>
        </div>
        <div  class="clearfix">
            Прошу допустить меня к вступительным испытаниям по следующим предметам и не засчитывать по данным предметам результаты ЕГЭ (при наличии результатов):
            <br />
            <%=Html.LabelFor(x=>Model.AddEducationInfo.ManualExamInfo.EgeExamId, "Предмет:") %>
            <%=Html.DropDownListFor(x=>Model.AddEducationInfo.ManualExamInfo.EgeExamId, Model.AddEducationInfo.ManualExamInfo.EgeManualExam) %>
            <% string disability = (!Model.AddEducationInfo.ManualExamInfo.PersonManualExamCategoryId.HasValue) ? "disabled=\"disabled\" title=\"Выберите категорию\"" : "";  %>
                <input type="button" value="Добавить" onclick="AddEgeManualExam()" id="AddManualExam" class="button button-blue"  <%=disability%> />
        </div>
        <div >
            <% string visibility = (Model.AddEducationInfo.ManualExamInfo.SelectedEgeManualExam.Count == 0) ? "display: none;" : "";%>
            <table id="tblManualExams"  class="paginate" style="<%=visibility %> width:100%; text-align: center;">
               <thead>
                    <tr>
                        <th>Предмет</th>
                        <th>Удалить</th>
                    </tr>
               </thead>
               <tbody>
                    <% foreach (var s in Model.AddEducationInfo.ManualExamInfo.SelectedEgeManualExam)
                       { %>
                        <tr id="manualexam_<%=s.Id.ToString() %>"> 
                            <td> <%=s.Name%> </td>
                            <td><span class="link" onclick="DeleteEgeManualExam(<%=s.Id.ToString()%>)"><img src="../../Content/themes/base/images/delete-icon.png" alt="Удалить" /></span></td>
                        </tr>
                    <% }%>
                </tbody>
            </table>
        </div>
    </div> 
<hr /> 
</div> 