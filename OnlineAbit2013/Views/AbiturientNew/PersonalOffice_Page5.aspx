<%@ Import Namespace="OnlineAbit2013" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Page Title="" Language="C#" MasterPageFile="~/Views/AbiturientNew/PersonalOffice.Master" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.PersonalOffice>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <%= GetGlobalResourceObject("Main", "PersonalOfficeHeader").ToString()%>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<% if (1 == 0)/* типа затычка, чтобы VS видела скрипты */
   {
%>
    <script type="text/javascript" src="../../Scripts/jquery-1.5.1-vsdoc.js"></script>
    <script type="text/javascript" src="../../Scripts/jquery.validate-vsdoc.js"></script>
<% } %>
    <script type="text/javascript"> 

        <% if (!String.IsNullOrEmpty(Model.CurrentEducation.HiddenObrazProgramId)) { %>
        $(function () { setTimeout(GetObrazPrograms) });
        <% } %>

        $(function () { setTimeout(GetProfessions) });
        $(function () {
            $('#CurrentEducation_StudyLevelId').change(function () { setTimeout(GetProfessions) });
            $('#CurrentEducation_StudyFormId').change(function () { setTimeout(GetProfessions) });
            $('#CurrentEducation_StudyBasisId').change(function () { setTimeout(GetProfessions) });
            $('#CurrentEducation_SemesterId').change(function () { setTimeout(GetProfessions) });
            $('#CurrentEducation_LicenseProgramId').change(function () { setTimeout(Myfun) });
            $('#CurrentEducation_ObrazProgramId').change(function () { setTimeout(Myfun) });

            $('#DisorderInfo_YearOfDisorder').change(function () { setTimeout(CheckDisorderInfoYear) });
        });

        function Myfun() {
            var profId = $('#CurrentEducation_LicenseProgramId').val();
            $('#CurrentEducation_HiddenLicenseProgramId').val(profId);
            var obrzId = $('#CurrentEducation_ObrazProgramId').val();
            $('#CurrentEducation_HiddenObrazProgramId').val(obrzId);
        }

        function CheckDisorderInfoYear() {
            var ret = true;
            if ($('#DisorderInfo_YearOfDisorder').val() == '') {
                ret = false;
                $('#DisorderInfo_YearOfDisorder').addClass('input-validation-error');
                $('#DisorderInfo_Year_Message').show();
                $('#DisorderInfo_Year_MessageFormat').hide();
            }
            else {
                $('#DisorderInfo_YearOfDisorder').removeClass('input-validation-error');
                $('#DisorderInfo_Year_Message').hide();
                var regex = /^\d{4}$/i;
                var val = $('#DisorderInfo_YearOfDisorder').val();
                if (!regex.test(val)) {
                    $('#DisorderInfo_YearOfDisorder').addClass('input-validation-error');
                    $('#DisorderInfo_Year_MessageFormat').show();
                    ret = false;
                }
                else {
                    $('#EducationInfo_SchoolExitYear').removeClass('input-validation-error');
                    $('#DisorderInfo_Year_MessageFormat').hide();
                }
            }
            return ret;
        }

        function GetProfessions() {
            if ($('#EducationInfo_VuzAdditionalTypeId').val() == 2) {
                var CurLevelId = $('#CurrentEducation_StudyLevelId').val();
                var CurrlObrazProgram = '#CurrentEducation_ObrazProgramId';
                var CurrlProfession = '#CurrentEducation_LicenseProgramId';
                var sfId = $('#CurrentEducation_StudyFormId').val();

                var curSemester = $('#CurrentEducation_SemesterId').val();

                $.post('/AbiturientNew/GetProfs', {
                    studyform: sfId, studybasis: $('#CurrentEducation_StudyBasisId').val(),
                    entry: CurLevelId,
                    semesterId: curSemester
                }, function (json_data) {
                    var options = '';
                    if (json_data.NoFree) {
                        $('#CurrentEducation_LicenseProgramId').html('');
                        $('#CurrentEducation_ObrazProgramId').html('');
                    }
                    else {
                        for (var i = 0; i < json_data.length; i++) {
                            options += '<option value="' + json_data[i].Id + '"';
                            if (json_data[i].Id == $('#CurrentEducation_HiddenLicenseProgramId').val())
                                options += ' selected="true" ';
                            options += ' >' + json_data[i].Name + '</option>';
                        }
                        $('#CurrentEducation_LicenseProgramId').html(options);
                        $('#CurrentEducation_ObrazProgramId').html('');
                    }
                }, 'json');
            }
        }
        function GetObrazPrograms() {
            var CurrlObrazProgram = '#CurrentEducation_ObrazProgramId';
            var profId = $('#CurrentEducation_LicenseProgramId').val();
            var sfId = $('#CurrentEducation_StudyFormId').val();
            if (profId == null) {
                profId = $('#CurrentEducation_HiddenLicenseProgramId').val();
            }
            $('#_ObrazProg').show();
            var CurLevelId = $('#CurrentEducation_StudyLevelId').val();
            var curSemester = $('#CurrentEducation_SemesterId').val();
            $.post('/Transfer/GetObrazPrograms', {
                prof: profId, studyform: sfId, studybasis: $('#CurrentEducation_StudyBasisId').val(),
                entry: CurLevelId, semesterId: curSemester
            }, function (json_data) {
                var options = '';
                if (json_data.NoFree) {
                    $('#CurrentEducation_ObrazProgramId').html('');
                }
                else {
                    for (var i = 0; i < json_data.List.length; i++) {
                        options += '<option value="' + json_data.List[i].Id + '"';
                        if (json_data.List[i].Id == $('#CurrentEducation_HiddenObrazProgramId').val())
                            options += ' selected="true" ';
                        options += ' >' + json_data.List[i].Name + '</option>';
                    }
                    $('#CurrentEducation_ObrazProgramId').html(options).show();
                }
            }, 'json');
        }
    </script>
    <script type="text/javascript">
        function updateIs2014() {
            if ($("#Is2014").is(':checked')) {
                $('#EgeCert').attr('disabled', 'disabled');
            }
            else {
                $('#EgeCert').removeAttr('disabled');
            }
        }
        function updateIsSecondWave() {
            if (($("#IsInUniversity").is(':checked')) || ($("#IsSecondWave").is(':checked'))) {
                $('#EgeCert').attr('disabled', 'disabled');
                $('#_EgeMark').hide();
            }
            else {
                if ($("#Is2014").is(':checked')) {
                    $('#EgeCert').attr('disabled', 'disabled');
                }
                else {
                    $('#EgeCert').removeAttr('disabled');
                }
                $('#_EgeMark').show();
            }
        }
        function updateIsInUniversity() {
            if (($("#IsInUniversity").is(':checked')) || ($("#IsSecondWave").is(':checked'))) {
                $('#EgeCert').attr('disabled', 'disabled');
                $('#_EgeMark').hide();
            }
            else {
                if ($("#Is2014").is(':checked')) {
                    $('#EgeCert').attr('disabled', 'disabled');
                }
                else {
                    $('#EgeCert').removeAttr('disabled');
                }
                $('#_EgeMark').show();
            }
        }

        function fStartTwo() {
            $("#dialog:ui-dialog").dialog("destroy");
            $('form').submit(function () {
                return CheckForm();
            });

                <% if (!Model.Enabled)
                   { %>
                $('input').attr('readonly', 'readonly');
                $('select').attr('disabled', 'disabled');
                <% } %>

                <% if (Model.Enabled)
                 { %>
                $("#CurrentEducation_AccreditationDate").datepicker({
                    changeMonth: true,
                    changeYear: true,
                    showOn: "focus",
                    yearRange: '1920:2014',
                    defaultDate: '-1y',
                });
                $.datepicker.regional["ru"];
                <% } %>

                function loadFormValues() {
                    var existingCerts = '';
                    var exams_html = '';
                    $.getJSON("AbiturientNew/GetAbitCertsAndExams", null, function (res) {
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

                var certificateNumber = $("#EgeCert"),
			    examName = $("#EgeExam"),
			    examMark = $("#EgeMark"),
                Is2014 = $("#Is2014"),
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
                function checkLength() {
                    if ((certificateNumber.val().length > 15 || certificateNumber.val().length < 15) && (!$("#Is2014").is(':checked'))) {
                        certificateNumber.addClass("ui-state-error");
                        updateTips("Номер сертификата должен быть 15-значным в формате РР-ХХХХХХХХ-ГГ");
                        return false;
                    } else {
                        return true;
                    }
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

                            bValid = bValid && checkLength();
                            bValid = bValid && checkVal();

                            if (bValid) {
                                //add to DB
                                var parm = new Object();
                                parm["certNumber"] = certificateNumber.val();
                                parm["examName"] = examName.val();
                                parm["examValue"] = examMark.val();
                                parm["Is2014"] = $("#Is2014").is(':checked');
                                parm["IsInUniversity"] = $("#IsInUniversity").is(':checked');
                                parm["IsSecondWave"] = $("#IsSecondWave").is(':checked');

                                $.post("AbiturientNew/AddMark", parm, function (res) {
                                    //add to table if ok
                                    if (res.IsOk) {
                                        $("#tblEGEData tbody").append('<tr id="' + res.Data.Id + '">' +
							            '<td>' + res.Data.CertificateNumber + '</td>' +
							            '<td>' + res.Data.ExamName + '</td>' +
							            '<td>' + res.Data.ExamMark + '</td>' +
                                        '<td><span class="link" onclick="DeleteMrk(\'' + res.Data.Id + '\')"><img src="../../Content/themes/base/images/delete-icon.png" alt="Удалить оценку" /></span></td>' +
						                '</tr>');
                                        $("#noMarks").html("").hide();
                                        $("#dialog-form").dialog("close");
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
                    }
                });

                $("#create-ege").button().click(function () {
                    loadFormValues();
                    $("#dialog-form").dialog("open");
                });
            }
            function DeleteMrk(id) {
                var data = new Object();
                data['mId'] = id;
                $.post("AbiturientNew/DeleteEgeMark", data, function r(res) {
                    if (res.IsOk) {
                        $("#" + id.toString()).html('').hide();
                    }
                    else {
                        alert("Ошибка при удалении оценки:\n" + res.ErrorMsg);
                    }
                }, 'json');
            }
	</script>
    <div class="grid">
        <div class="wrapper">
            <div class="grid_4 first">
            <% if (!Model.Enabled)
                { %>
                <div id="Message" class="message warning">
                    <span class="ui-icon ui-icon-alert"></span><%= GetGlobalResourceObject("PersonInfo", "WarningMessagePersonLocked").ToString()%>
                </div>
            <% } %>
                <form class="panel form" action="AbiturientNew/NextStep" method="post">
                    <%= Html.ValidationSummary() %>
                    <%= Html.HiddenFor(x => x.Stage) %>
                    <div id="EnglishMark" class="clearfix">
                        <%= Html.LabelFor(x => x.AddEducationInfo.EnglishMark, GetGlobalResourceObject("PersonalOffice_Step4", "EnglishMark").ToString())%>
                        <%= Html.TextBoxFor(x => x.AddEducationInfo.EnglishMark) %>
                    </div>
                    <div class="clearfix">
                        <%= Html.LabelFor(x => x.AddEducationInfo.StartEnglish, GetGlobalResourceObject("PersonalOffice_Step4", "EnglishNull").ToString())%>
                        <%= Html.CheckBoxFor(x => x.AddEducationInfo.StartEnglish)%>
                    </div>
                    <div id="_ForeignCountryEduc" class="clearfix" style="display:none">
                        <div class="clearfix">
                            <%= Html.LabelFor(x => x.AddEducationInfo.HasTRKI, GetGlobalResourceObject("PersonalOffice_Step4", "HasTRKI").ToString())%>
                            <%= Html.CheckBoxFor(x => x.AddEducationInfo.HasTRKI)%>
                        </div>
                        <div id="TRKI" class="clearfix" >
                            <%= Html.LabelFor(x => x.AddEducationInfo.TRKICertificateNumber, GetGlobalResourceObject("PersonalOffice_Step4", "TRKICertificateNumber").ToString())%>
                            <%= Html.TextBoxFor(x => x.AddEducationInfo.TRKICertificateNumber) %>
                        </div>
                    </div>
                    <div class="clearfix">
                        <%= Html.LabelFor(x => x.AddEducationInfo.LanguageId, GetGlobalResourceObject("PersonalOffice_Step4", "LanguageId").ToString())%>
                        <%= Html.DropDownListFor(x => x.AddEducationInfo.LanguageId, Model.AddEducationInfo.LanguageList) %>
                    </div>
                    <% if (Model.AddEducationInfo.HasTransfer) { %>
                    <div id="_TransferData" class="clearfix">
                        <div id = "_AccreditationInfo">
                            <div class="clearfix">
                                <%= Html.LabelFor(x => x.CurrentEducation.HasAccreditation, GetGlobalResourceObject("PersonalOffice_Step4", "HasAccreditation").ToString())%>
                                <%= Html.CheckBoxFor(x => x.CurrentEducation.HasAccreditation)%>
                            </div>
                            <div class="clearfix">
                                <%= Html.LabelFor(x => x.CurrentEducation.AccreditationNumber,  GetGlobalResourceObject("PersonalOffice_Step4", "AccreditationNumber").ToString())%>
                                <%= Html.TextBoxFor(x => x.CurrentEducation.AccreditationNumber)%>
                            </div>
                            <div class="clearfix">
                                <%= Html.LabelFor(x => x.CurrentEducation.AccreditationDate,  GetGlobalResourceObject("PersonalOffice_Step4", "AccreditationDate").ToString()) %>
                                <%= Html.TextBoxFor(x => x.CurrentEducation.AccreditationDate)%> 
                            </div>
                        </div>
                        <div class="clearfix">
                            <%= Html.LabelFor(x => x.CurrentEducation.StudyLevelId,  GetGlobalResourceObject("PersonalOffice_Step4", "PersonStudyLevel").ToString())%>
                            <%= Html.DropDownListFor(x => x.CurrentEducation.StudyLevelId, Model.CurrentEducation.StudyLevelList, new SortedList<string, object>() { })%>
                        </div>  
                        <div class="clearfix">
                            <%= Html.LabelFor(x => x.CurrentEducation.SemesterId,  GetGlobalResourceObject("PersonalOffice_Step4", "PersonStudySemester").ToString())%>
                            <%= Html.DropDownListFor(x => x.CurrentEducation.SemesterId, Model.CurrentEducation.SemesterList, new SortedList<string, object>() {  })%>
                        </div>
                        <div id="_TransferSPBUAddInfo" class="clearfix" >
                            <div class="clearfix">
                                <%= Html.LabelFor(x => x.CurrentEducation.StudyFormId,GetGlobalResourceObject("PersonalOffice_Step4", "PersonStudyForm").ToString())%>
                                <%= Html.DropDownListFor(x => x.CurrentEducation.StudyFormId, Model.EducationInfo.StudyFormList, new SortedList<string, object>() {  })%>
                            </div>
                            <div class="clearfix">
                                <%= Html.LabelFor(x => x.CurrentEducation.StudyBasisId,GetGlobalResourceObject("PersonalOffice_Step4", "PersonStudyBasis").ToString() ) %>
                                <%= Html.DropDownListFor(x => x.CurrentEducation.StudyBasisId, Model.EducationInfo.StudyBasisList, new SortedList<string, object>() { })%>
                            </div>
                            <div class="clearfix">
                                <%= Html.LabelFor(x => x.CurrentEducation.LicenseProgramId, GetGlobalResourceObject("PersonalOffice_Step4", "CurrentLicenceProgram").ToString()) %>
                                <%= Html.HiddenFor(x=> x.CurrentEducation.HiddenLicenseProgramId) %>
                                <select id="CurrentEducation_LicenseProgramId" size="12" name="lProfession" style="width:459px;" onchange="GetObrazPrograms()"></select> 
                            </div>
                            <div class="clearfix" id="_ObrazProg" <% if (String.IsNullOrEmpty(Model.CurrentEducation.HiddenObrazProgramId)) { %>style="display:none;"<% } %>>
                                <%= Html.LabelFor(x => x.CurrentEducation.ObrazProgramId, GetGlobalResourceObject("PersonalOffice_Step4", "CurrentObrazProgram").ToString())%>
                                <%= Html.HiddenFor(x => x.CurrentEducation.HiddenObrazProgramId)%>
                                <select id="CurrentEducation_ObrazProgramId"  <%if (!Model.Enabled){ %> disabled="disabled" <% } %> size="5" name="CurObrazProgram" style="width:459px;"></select>
                            </div>
                            <div class="clearfix">
                                <%= Html.LabelFor(x => x.CurrentEducation.ProfileName,GetGlobalResourceObject("PersonalOffice_Step4", "CurrentProfile").ToString())%>
                                <%= Html.TextBoxFor(x => x.CurrentEducation.ProfileName)%>
                            </div>
                        </div>
                        <div id="_Reason"> 
                            <%= Html.LabelFor(x => x.ChangeStudyFormReason.Reason, GetGlobalResourceObject("PersonalOffice_Step4", "ChangeStudyFormReason").ToString())%>
                            <%= Html.TextAreaFor(x => x.ChangeStudyFormReason.Reason, 5, 85, new SortedList<string, object>() { { "class", "noresize" } })%>
                        </div>
                        <div id="_TransferHasScholarship">
                            <div class="clearfix">
                                <%= Html.LabelFor(x => x.CurrentEducation.HasScholarship, GetGlobalResourceObject("PersonalOffice_Step4", "HasScholarship").ToString()) %>
                                <%= Html.CheckBoxFor(x => x.CurrentEducation.HasScholarship)%>
                            </div>
                        </div>
                    </div>
                    <% } %>
                    <% if (Model.AddEducationInfo.HasRecover) { %>
                    <div id = "_DisorderInfo">
                        <h3><%=GetGlobalResourceObject("PersonalOffice_Step4", "DisorderInfo").ToString()%></h3>
                        <hr /> 
                        <fieldset><br />
                        <div class="clearfix">
                            <%= Html.LabelFor(x => x.DisorderInfo.YearOfDisorder, GetGlobalResourceObject("PersonalOffice_Step4", "DisorderYear").ToString() )%>
                            <%= Html.TextBoxFor(x => x.DisorderInfo.YearOfDisorder)%>
                            <br /><p></p>
                            <span id="DisorderInfo_Year_Message" class="Red" style="display:none; border-collapse:collapse;"><%=GetGlobalResourceObject("PersonalOffice_Step4", "SchoolExitYear_Message").ToString()%></span>
                            <span id="DisorderInfo_Year_MessageFormat" class="Red" style="display:none; border-collapse:collapse;"><%=GetGlobalResourceObject("PersonalOffice_Step4", "EducationInfo_SchoolExitYear_MessageFormat").ToString()%></span>
                        </div>
                        <div class="clearfix">
                            <%= Html.LabelFor(x => x.DisorderInfo.EducationProgramName, GetGlobalResourceObject("PersonalOffice_Step4", "ObrazProgramName").ToString())%>
                            <%= Html.TextBoxFor(x => x.DisorderInfo.EducationProgramName)%>
                            <br /> 
                        </div>
                        <div class="clearfix">
                            <%= Html.LabelFor(x => x.DisorderInfo.IsForIGA, GetGlobalResourceObject("PersonalOffice_Step4", "RecoverForIGA").ToString())%>
                            <%= Html.CheckBoxFor(x => x.DisorderInfo.IsForIGA)%>
                            <br /> 
                        </div>
                        </fieldset>
                    </div>

                    <div id="EGEData" class="clearfix">
                        <h6><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEmarks").ToString()%></h6>
                        <% if (Model.EducationInfo.EgeMarks.Count == 0)
                            { 
                        %>
                            <h6 id="noMarks"><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEnomarks").ToString()%></h6>
                        <%
                            }
                            else
                            {
                        %>
                        <table id="tblEGEData" class="paginate" style="width:400px">
                            <thead>
                            <tr>
                                <th><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEsert").ToString()%></th>
                                <th><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEsubject").ToString()%></th>
                                <th><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEball").ToString()%></th>
                                <th></th>
                            </tr>
                            </thead>
                            <tbody>
                        <%
                                foreach (var mark in Model.EducationInfo.EgeMarks)
                                {
                        %>
                            <tr id="<%= mark.Id.ToString() %>">
                                <td><span><%= mark.CertificateNum%></span></td>
                                <td><span><%= mark.ExamName%></span></td>
                                <td><span><%= mark.Value%></span></td>
                                <td><%= Html.Raw("<span class=\"link\" onclick=\"DeleteMrk('" + mark.Id.ToString() + "')\"><img src=\"../../Content/themes/base/images/delete-icon.png\" alt=\"Удалить оценку\" /></span>")%></td>
                            </tr>
                        <%
                                }
                        %>
                            </tbody>
                        </table>
                        <% } %>
                        <br />
                        <button type="button" id="create-ege" class="button button-blue"><%=GetGlobalResourceObject("PersonalOffice_Step4", "AddMark").ToString()%></button>
                        <div id="dialog-form">
                            <p id="validation_info">Все поля обязательны для заполнения</p>
	                        <hr />
                            <fieldset>
                                <div id="_CertNum" class="clearfix">
                                    <label for="EgeCert"><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEsert").ToString()%></label><br />
		                            <input type="text" id="EgeCert" disabled="disabled"/><br />
                                </div>
                                <div class="clearfix">
                                    <label for="Is2014"><%=GetGlobalResourceObject("PersonalOffice_Step4", "CurrentYear").ToString()%></label><br />
		                            <input type="checkbox" id="Is2014" checked="checked" onchange="updateIs2014()" /><br />
                                </div>
                                <div class="clearfix">
                                    <label for="EgeExam"><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEsubject").ToString()%></label><br />
		                            <select id="EgeExam" ></select><br />
                                </div>
                                <div id="_EgeMark" class="clearfix" >
                                    <label for="EgeMark"><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEball").ToString()%></label><br />
		                            <input type="text" id="EgeMark" value="" /><br />
                                </div>
                                <div id="_IsSecondWave" class="clearfix" >
                                    <label for="IsSecondWave"><%=GetGlobalResourceObject("PersonalOffice_Step4", "SecondWave").ToString()%></label><br />
		                            <input type="checkbox" id="IsSecondWave" onchange="updateIsSecondWave()" /><br />
                                </div>
                                <br />
                                <div id="_IsInUniversity" class="clearfix">
                                    <label for="IsInUniversity"><%=GetGlobalResourceObject("PersonalOffice_Step4", "PassInSPbSU").ToString()%></label><br />
		                            <input type="checkbox" id="IsInUniversity" onchange="updateIsInUniversity()" /><br />
                                </div>
	                        </fieldset>
                        </div>
                    </div>
                        

                    <% } %>
                    <hr />
                    <div class="clearfix">
                        <input id="Submit5" class="button button-green" type="submit" value="<%= GetGlobalResourceObject("PersonInfo", "ButtonSubmitText").ToString()%>" />
                    </div>
                </form>
            </div>
            <div class="grid_2">
                <ol>
                    <li><a href="../../AbiturientNew?step=1"><%= GetGlobalResourceObject("PersonInfo", "Step1")%></a></li>
                    <li><a href="../../AbiturientNew?step=2"><%= GetGlobalResourceObject("PersonInfo", "Step2")%></a></li>
                    <li><a href="../../AbiturientNew?step=3"><%= GetGlobalResourceObject("PersonInfo", "Step3")%></a></li>
                    <li><a href="../../AbiturientNew?step=4"><%= GetGlobalResourceObject("PersonInfo", "Step4")%></a></li>
                    <li><a href="../../AbiturientNew?step=5"><%= GetGlobalResourceObject("PersonInfo", "Step5")%></a></li>
                    <li><a href="../../AbiturientNew?step=6"><%= GetGlobalResourceObject("PersonInfo", "Step6")%></a></li>
                    <li><a href="../../AbiturientNew?step=7"><%= GetGlobalResourceObject("PersonInfo", "Step7")%></a></li>
                </ol>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderScriptsContent" runat="server">
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="Subheader" runat="server">
    <h2>Анкета</h2>
</asp:Content>
