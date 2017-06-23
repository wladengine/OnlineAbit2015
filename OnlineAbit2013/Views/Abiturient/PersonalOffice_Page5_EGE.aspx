 <%@ Page Title="" Language="C#"  Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.PersonalOffice>" %>
 <script>
     function updateIs2014() {
         if ($('#EgeYear').val() == <%=DateTime.Now.Year%> ) 
         {
            $('#_IsInUniversity').show();
            $('#_IsSecondWave').show();
            updateIsInUniversity();
        }
        else { 
            $('#_EgeMark').show();
            $('#_IsSecondWave').hide();
            $('#_IsInUniversity').hide(); 
        }
        if ($('#EgeYear').val() > 2013) {
            $('#EgeCert').attr('disabled', 'disabled');
        }
        else {
            $('#EgeCert').removeAttr('disabled');
        }
     }

     function updateIsInUniversity() {
         if (($("#IsInUniversity").is(':checked')) || ($("#IsSecondWave").is(':checked'))) {
             $('#EgeCert').attr('disabled', 'disabled');
             $('#_EgeMark').hide();
         }
         else {
             if ($("#EgeYear").val() > 2013) {
                 $('#EgeCert').attr('disabled', 'disabled');
             }
             else {
                 $('#EgeCert').removeAttr('disabled');
             }
             $('#_EgeMark').show();
         }
     } 

     function loadFormValues() {
         var existingCerts = '';
         var exams_html = '';
         $.getJSON("Abiturient/GetAbitCertsAndExams", null, function (res) {
             existingCerts = res.Certs;
             for (var i = 0; i < res.Exams.length; i++) {
                 exams_html += '<option value="' + res.Exams[i].Key + '">' + res.Exams[i].Value + '</option>';
             }
             $("#EgeExam").html(exams_html);
             $("#EgeCert").autocomplete({
                 source: existingCerts
             });
         });
     }



     function ShowDialog()
     {
         var certificateNumber = $("#EgeCert"), 
        examName = $("#EgeExam"),
        examMark = $("#EgeMark"),
        IsSecondWave = $("#IsSecondWave"),
        IsInUniversity = $("#IsInUniversity"),

        allFields = $([]).add(certificateNumber).add(examName).add(examMark),
        tips = $(".validateTips");

         function updateTips(t) {
             tips.text(t).addClass("ui-state-highlight");
             setTimeout(function () {
                 tips.removeClass("ui-state-highlight", 1500);
             }, 500);
         }

         function checkVal() {
             var val = examMark.val();
             if ((val < 1 || val > 100) && (!$("#IsSecondWave").is(':checked')) && (!$("#IsInUniversity").is(':checked'))) {
                 updateTips("Экзаменационный балл должен быть от 1 до 100");
                 return false;
             }
             else {
                 return true;
             }
         }

         function checkLength() {
             if ((certificateNumber.val().length > 15 || certificateNumber.val().length < 15) && ($("#EgeYear").val() < 2014)) {
                 certificateNumber.addClass("ui-state-error");
                 updateTips("Номер сертификата должен быть 15-значным в формате РР-ХХХХХХХХ-ГГ");
                 return false;
             } else {
                 return true;
             }
         }   
            
         function checkRegexp(o, regexp, n) {
             if (!(regexp.test(o.val()))) {
                 o.addClass("ui-state-error");
                 updateTips(n);
                 return false;
             } else {
                 return true;
             }
         }

         $("#dialog-form").dialog({
             autoOpen: false,
             height: 430,
             width: 350,
             modal: true,
             buttons: {
                 "Добавить": function () {
                     var bValid = true;
                     allFields.removeClass("ui-state-error");

                     bValid = bValid && checkLength() && checkVal();

                     if (bValid) {
                         //add to DB
                         var parm = new Object();
                         parm["certNumber"] = certificateNumber.val();
                         parm["egeYear"] = $("#EgeYear").val();
                         parm["examName"] = examName.val();
                         parm["examValue"] = examMark.val(); 
                         if ($('#EgeYear').val() != <%=DateTime.Now.Year%> ) 
                         {
                                parm["IsInUniversity"] = "false";
                         parm["IsSecondWave"] = "false";
                     }
                     else
                     {
                         parm["IsInUniversity"] = $("#IsInUniversity").is(':checked');
                         parm["IsSecondWave"] = $("#IsSecondWave").is(':checked');
                     }

                     $.post("Abiturient/AddMark", parm, function (res) {
                         //add to table if ok
                         if (res.IsOk) {
                             $("#tblEGEData tbody").append('<tr id="' + res.Data.Id + '">' +
                                 '<td>' + res.Data.CertificateNumber + '</td>' +
                                 '<td>' + res.Data.EgeYear + '</td>' +
                                 '<td>' + res.Data.ExamName + '</td>' +
                                 '<td>' + res.Data.ExamMark + '</td>' +
                                 '<td><span class="link" onclick="DeleteMrk(\'' + res.Data.Id + '\')"><img src="../../Content/themes/base/images/delete-icon.png" alt="Удалить оценку" /></span></td>' +
                             '</tr>');
                             $("#noMarks").html("").hide();
                             $("#dialog-form").dialog("close");
                             $("#tblEGEData").show();
                         }
                         else {
                             updateTips(res.ErrorMessage);
                         }
                     }, "json");
                 }
             },
             "Отменить": function () {
                 $(this).dialog("close");
             }
         },
             close: function () {
                 allFields.val("").removeClass("ui-state-error");
                 updateTips('Все поля обязательны для заполнения');
             }
         });

         $("#dialog:ui-dialog").dialog("destroy");
         $("#create-ege").button().click(function () {
             loadFormValues();
             $("#dialog-form").dialog("open");
         });
     }
 </script>

                    <br />
                    <div id="EGEData" class="clearfix">
                        <h6><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEmarks").ToString()%></h6>
                        <% if (Model.EducationInfo.EgeMarks.Count == 0)
                            { 
                        %>
                            <h6 id="noMarks"><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEnomarks").ToString()%></h6>
                        <%
                            }
                        %>
                        <table id="tblEGEData" class="paginate" style="width:450px; vertical-align:middle; text-align:center; <% if (Model.EducationInfo.EgeMarks.Count == 0) {%> display:none;<%}%>">
                            <thead>
                            <tr>
                                <th style="width:30%;"><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEsert").ToString()%></th>
                                <th style="width:10%;"><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEyear").ToString()%></th>
                                <th style="width:30%;"><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEsubject").ToString()%></th>
                                <th style="width:30%;"><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEball").ToString()%></th>
                                <th ></th>
                            </tr>
                            </thead>
                            <tbody>
                        <%
                                foreach (var mark in Model.EducationInfo.EgeMarks)
                                {
                        %>
                            <tr id="<%= mark.Id.ToString() %>">
                                <td><span><%= mark.CertificateNum%></span></td>
                                <td><span><%= mark.Year %></span></td>
                                <td><span><%= mark.ExamName %></span></td>
                                <td><span><%= mark.Value %></span></td>
                                <td><%= Html.Raw("<span class=\"link\" onclick=\"DeleteMrk('" + mark.Id.ToString() + "')\"><img src=\"../../Content/themes/base/images/delete-icon.png\" alt=\"Удалить оценку\" /></span>")%></td>
                            </tr>
                        <%
                                }
                        %>
                            </tbody>
                        </table>
                        <br />
                        <div class="clearfix">
                        <input type="button" id="create-ege" class="button button-blue" value='<%=GetGlobalResourceObject("PersonalOffice_Step4", "AddMark").ToString()%>'/>
                        </div>
                        <div id="dialog-form">
                            <p id="validation_info" class="validateTips">Все поля обязательны для заполнения</p>
	                        <hr />
                            <fieldset>
                                <div id="_CertNum" class="clearfix">
                                    <label for="EgeCert"><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEsert").ToString()%></label><br />
		                            <input type="text" id="EgeCert" disabled="disabled"/><br /><br />
                                </div>
                               <div class="clearfix">
                                    <label for="EgeExam"><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEyear").ToString()%></label><br />
		                            <%= Html.DropDownList("EgeYear", Model.EducationInfo.EgeYearList, new SortedList<string, object>(){{"onchange","updateIs2014()"}})%>
                                </div>
                                <div class="clearfix">
                                    <label for="EgeExam"><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEsubject").ToString()%></label><br />
		                            <%= Html.DropDownList("EgeExam", Model.EducationInfo.EgeSubjectList) %>
                                </div>
                                <div id="_EgeMark" class="clearfix" >
                                    <label for="EgeMark"><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEball").ToString()%></label><br />
		                            <input type="text" id="EgeMark" value="" /><br />
                                </div>
                                <div id="_IsSecondWave" class="clearfix" >
                                    <label for="IsSecondWave"><%=GetGlobalResourceObject("PersonalOffice_Step4", "SecondWave").ToString()%></label><br />
		                            <input type="checkbox" id="IsSecondWave" onchange="updateIsInUniversity()" /><br />
                                </div>
                                <br />
                                <div id="_IsInUniversity" class="clearfix">
                                    <label for="IsInUniversity"><%=GetGlobalResourceObject("PersonalOffice_Step4", "PassInSPbSU").ToString()%></label><br />
		                            <input type="checkbox" id="IsInUniversity" onchange="updateIsInUniversity()" /><br />
                                </div>
	                        </fieldset>
                        </div>
                    </div>