<%@ Import Namespace="OnlineAbit2013" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Abiturient/PersonalOffice.Master" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.PersonalOffice>" %>

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
    <script type="text/javascript" src="../../Scripts/jquery-ui-1.8.11.js"></script>
    <script type="text/javascript" src="../../Scripts/jquery.ui.datepicker-ru.js"></script>
    <script type="text/javascript"> 
        $(function () {
            $('#CurrentEducation_StudyLevelId').change(function () { setTimeout(GetProfessions) });
            $('#CurrentEducation_StudyFormId').change(function () { setTimeout(GetProfessions) });
            $('#CurrentEducation_StudyBasisId').change(function () { setTimeout(GetProfessions) });
            $('#CurrentEducation_SemesterId').change(function () { setTimeout(GetProfessions) });
            $('#CurrentEducation_LicenseProgramId').change(function () { setTimeout(Myfun) });
            $('#CurrentEducation_ObrazProgramId').change(function () { setTimeout(Myfun) });
            $('#DisorderInfo_YearOfDisorder').change(function () { setTimeout(CheckDisorderInfoYear) });
            $('#ChangeStudyFormReason_Reason').keyup(function () { setTimeout(ChangeReason); });
            $('#ChangeStudyFormReason_Reason').blur(function () { setTimeout(ChangeReason); });
            fStart();

            GetProfessions();
             <% if (Model.CurrentEducation != null && !String.IsNullOrEmpty(Model.CurrentEducation.HiddenLicenseProgramId)) { %>
            $('#CurrentEducation_LicenseProgramId').val(<%= Model.CurrentEducation.HiddenLicenseProgramId %>);
            $('#CurrentEducation_HiddenLicenseProgramId').val(<%= Model.CurrentEducation.HiddenLicenseProgramId %>);
            <% } %>
            $("form").submit(function () {
                return CheckForm();
            });

            <% if (Model.CurrentEducation != null && !String.IsNullOrEmpty(Model.CurrentEducation.HiddenObrazProgramId)) { %>
            $(function () { setTimeout(GetObrazPrograms) });
            $('#CurrentEducation_ObrazProgramId').val(<%= Model.CurrentEducation.HiddenObrazProgramId %>);
            $('#CurrentEducation_HiddenObrazProgramId').val(<%= Model.CurrentEducation.HiddenObrazProgramId %>);
            <% } %>
        });

        function CheckForm() {
            <% if (Model.AddEducationInfo.HasTransfer)
               {
                   if (Model.AddEducationInfo.HasReason)
                   {  %>
                        return CheckLicenseProgramId() && CheckObrazProgramId();
                <% }
                   else
                   {%> 
                        return CheckLicenseProgramId();
                 <% }
               }
               else
               { %>
                    return true;
            <% } %>
        }
        function CheckLicenseProgramId() {
            var ret = true;
            var val = $('#CurrentEducation_HiddenLicenseProgramId').val();
             
            if (val == '0' || val == '') {
                ret = false;
                $('#CurrentEducation_LicenseProgramId').addClass('input-validation-error');
                $('#CurrentEducation_LicenseProgramId_Message').show();
            }
            else {
                $('#CurrentEducation_LicenseProgramId').removeClass('input-validation-error');
                $('#CurrentEducation_LicenseProgramId_Message').hide();
            }
            return ret;
        }
        function CheckObrazProgramId() {
            var ret = true;
            var val = $('#CurrentEducation_HiddenObrazProgramId').val();

            if (val == '0' || val == '') {
                ret = false;
                $('#CurrentEducation_ObrazProgramId').addClass('input-validation-error');
                $('#CurrentEducation_ObrazProgramId_Message').show();
            }
            else {
                $('#CurrentEducation_ObrazProgramId').removeClass('input-validation-error');
                $('#CurrentEducation_ObrazProgramId_Message').hide();
            }
            return ret;
        }
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
            var CurLevelId = $('#CurrentEducation_StudyLevelId').val();
            var CurrlObrazProgram = '#CurrentEducation_ObrazProgramId';
            var CurrlProfession = '#CurrentEducation_LicenseProgramId';
            var sfId = $('#CurrentEducation_StudyFormId').val();

            var curSemester = $('#CurrentEducation_SemesterId').val();
           
            $('#CurrentEducation_HiddenLicenseProgramId').val('0'); 
            $('#CurrentEducation_LicenseProgramId').removeClass('input-validation-error');
            $('#CurrentEducation_LicenseProgramId_Message').hide();

            $('#CurrentEducation_HiddenObrazProgramId').val('0');
            $('#CurrentEducation_ObrazProgramId').removeClass('input-validation-error');
            $('#CurrentEducation_ObrazProgramId_Message').hide();

            $.post('/Abiturient/GetProfsAll', {
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
        function GetObrazPrograms() {
            var CurrlObrazProgram = '#CurrentEducation_ObrazProgramId';
            var profId = $('#CurrentEducation_LicenseProgramId').val();
            var sfId = $('#CurrentEducation_StudyFormId').val();
            if (profId == null) {
                profId = $('#CurrentEducation_HiddenLicenseProgramId').val();
            }
            $('#CurrentEducation_HiddenObrazProgramId').val('0');

            $('#CurrentEducation_ObrazProgramId').removeClass('input-validation-error');
            $('#CurrentEducation_ObrazProgramId_Message').hide();
            $('#_ObrazProg').show();
            var CurLevelId = $('#CurrentEducation_StudyLevelId').val();
            var curSemester = $('#CurrentEducation_SemesterId').val();
            $.post('/Transfer/GetObrazProgramsAll', {
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
    <!-- ниже кусок JS для ЕГЭ -->
    <script type="text/javascript">
        function updateIsSecondWave() {
            if (($("#IsInUniversity").is(':checked')) || ($("#IsSecondWave").is(':checked'))) {
                $('#_EgeMark').hide();
            }
            else {
                $('#_EgeMark').show();
            }
        }
        function updateIsInUniversity() {
            if (($("#IsInUniversity").is(':checked')) || ($("#IsSecondWave").is(':checked'))) {
                $('#_EgeMark').hide();
            }
            else {
                $('#_EgeMark').show();
            }
        }

        function fStart() {
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
                yearRange: '1990:2017',
                defaultDate: '-1y',
            });
            $.datepicker.setDefaults($.datepicker.regional['<%= GetGlobalResourceObject("Common", "DatetimePicker").ToString()%>']);
                <% } %>

            var examName = $("#EgeExam"),
			examMark = $("#EgeMark"),
            IsSecondWave = $("#IsSecondWave"),
            IsInUniversity = $("#IsInUniversity"),

			allFields = $([]).add(examName).add(examMark),
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

                        bValid = bValid && checkVal();

                        if (bValid) {
                            //add to DB
                            var parm = new Object();
                            parm["egeYear"] = $("#EgeYear").val();
                            parm["examName"] = examName.val();
                            parm["examValue"] = examMark.val();
                            parm["IsInUniversity"] = $("#IsInUniversity").is(':checked');
                            parm["IsSecondWave"] = $("#IsSecondWave").is(':checked');

                            $.post("Abiturient/AddMark", parm, function (res) {
                                //add to table if ok
                                if (res.IsOk) {
                                    $("#tblEGEData tbody").append('<tr id="' + res.Data.Id + '">' +
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
                $("#dialog-form").dialog("open");
            });
        }
        function DeleteMrk(id) {
            var data = new Object();
            data['mId'] = id;
            $.post("Abiturient/DeleteEgeMark", data, function r(res) {
                if (res.IsOk) {
                    $("#" + id.toString()).html('').hide();
                }
                else {
                    alert("Ошибка при удалении оценки:\n" + res.ErrorMsg);
                }
            }, 'json');
        }
        function ChangeReason() {
            var tx = $('#ChangeStudyFormReason_Reason').val();
            if (tx.indexOf("поступление") != -1) {
                $('#ChangeStudyFormReasonMessage').text('Для поступления на программы подготовки бакалавриата, магистратуры, аспирантуры - измените тип поступления на 4й странице анкеты на "Основной приём"');
                $('#ChangeStudyFormReasonMessage').show();
            }
            else {
                $('#ChangeStudyFormReasonMessage').text('');
                $('#ChangeStudyFormReasonMessage').hide();
            }
        }
        function UpdatAfterCertificatesType()
        {
            $.post('Abiturient/GetTypeCertificate', { typeid: $("#CertTypeId").val() }, function (res) {
                if (res.IsOk) {
                    if (res.IsBool)
                    {
                        $("#divCertValue").hide(); $('#IsBoolType').val(1);
                    }
                    else
                    {
                        $("#divCertValue").show(); $('#IsBoolType').val(0);
                    }
                }
                else {
                    alert(res.ErrorMessage);
                }
            }, 'json');
        }
        function CheckValueCertificate()
        {
            var val = $('#CertValue').val();
            var regex = /^([0-9,.])+$/i;
            if (val != '') {
                if (!regex.test(val)) {
                    $('#CertValue').addClass('input-validation-error');
                    $('#validationMsgCertValue').show();
                    return false;
                }
                else {
                    $('#CertValue').removeClass('input-validation-error');
                    $('#validationMsgCertValue').hide();
                }
            }
            if ($('#IsBoolType').val() != "0" && val != '') {
                $('#CertValue').addClass('input-validation-error');
                return false;
            }
            return true;
        }
        function AddCertificates() {
            var ret = CheckValueCertificate();
            if (!ret)
                return false;
            if ($('#CertTypeId').val() == null)
                return false;

            var param = new Object();
            param['TypeId'] = $('#CertTypeId').val();
            param['Number'] = $('#CertNumber').val();
            param['BoolType'] = $('#IsBoolType').val();
            param['Value'] = $('#CertValue').val();

            $.post('Abiturient/AddCertificates', param, function (res) {
                if (res.IsOk) {
                    $('#CertBlock').show();
                    var output = '';
                    output += '<tr id=\'' + res.Id + '\'>';
                    output += '<td style="vertical-align: middle;">' + res.Name + '</td>';
                    output += '<td style="vertical-align: middle;">' + res.Number + '</td>';
                    if (!res.IsBool) 
                        output += '<td style="vertical-align: middle;">' + res.Value + '</td>';
                    else
                        output += '<td style="vertical-align: middle;">' + <%= '"'+GetGlobalResourceObject("PersonalOffice_Step5", "CertificatePassed").ToString()+'"'%> + '</td>';

                    output += '<td style="width:10%; vertical-align: middle;"><span class="link" onclick="DeleteCertificate(\'' + res.Id + '\')" ><img src="../../Content/themes/base/images/delete-icon.png" alt="Удалить оценку" /><span></td>';
                    output += '</tr>';
                    $('#tblOlympiads tbody').append(output);
                }
                else {
                    alert(res.ErrorMessage);
                }
            }, 'json');
        }
        function DeleteCertificate(id) {
            var param = new Object();
            param['id'] = id;
            $.post('Abiturient/DeleteCertificate', param, function (res) {
                if (res.IsOk) {
                    $('#' + id).hide();
                    if (res.Count == 0) {
                        $('#CertBlock').hide();
                    }
                }
                else {
                    alert(res.ErrorMessage);
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
                <% foreach (var msg in Model.Messages)
                    { %>
                    <div id="<%= msg.Id %>" class="message info" style="padding:5px">
                        <span class="ui-icon ui-icon-alert"></span><%= msg.Text %>
                        <div style="float:right;"><span class="link" onclick="DeleteMsg('<%= msg.Id %>')"><img src="../../Content/themes/base/images/delete-icon.png" alt="Удалить" /></span></div>
                    </div>
                <% } %>
                <form class="panel form" action="Abiturient/NextStep" method="post" onsubmit="return CheckForm();">
                    <h3>5. <%= GetGlobalResourceObject("PersonalOffice_Step5", "PageHeader").ToString()%></h3>
                    <hr /><hr />
                    <%= Html.ValidationSummary() %>
                    <%= Html.HiddenFor(x => x.Stage) %>
                    <div class="clearfix">
                        <%= Html.LabelFor(x => x.AddEducationInfo.LanguageId, GetGlobalResourceObject("PersonalOffice_Step4", "LanguageId").ToString())%>
                        <%= Html.DropDownListFor(x => x.AddEducationInfo.LanguageId, Model.AddEducationInfo.LanguageList) %>
                    </div>
                    <div id="EnglishMark" class="clearfix">
                        <%= Html.LabelFor(x => x.AddEducationInfo.EnglishMark, GetGlobalResourceObject("PersonalOffice_Step4", "EnglishMark").ToString())%>
                        <%= Html.TextBoxFor(x => x.AddEducationInfo.EnglishMark) %>
                    </div>
                    <div class="clearfix">
                        <%= Html.LabelFor(x => x.AddEducationInfo.StartEnglish, GetGlobalResourceObject("PersonalOffice_Step4", "EnglishNull").ToString())%>
                        <%= Html.CheckBoxFor(x => x.AddEducationInfo.StartEnglish)%>
                    </div>
                    <% if (Model.CertificatesVisible)
                       { %>
                    <hr />
                    <h4><%=GetGlobalResourceObject("PersonalOffice_Step5", "CertificateHeader").ToString()%></h4>
                    <div>
                        <div class="clearfix">
                        <%= GetGlobalResourceObject("PersonalOffice_Step5", "CertificateType").ToString()%>:<br />
                        <%= Html.DropDownList("CertTypeId", Model.Certificates.CertTypeList,  new SortedList<string, object>() { {"style", "width:460px;"} , {"size", "4"} , {"onchange", "UpdatAfterCertificatesType()"}}) %>
                        </div><br/><p></p>
                        <div class="clearfix">
                            <label for="CertNumber"><%= GetGlobalResourceObject("PersonalOffice_Step5", "CertificateNumber").ToString()%></label>
                            <input id="CertNumber" type="text" /><br/><p></p>
                        </div>
                        <div class="clearfix" id ="divCertValue">
                            <label for="CertValue"><%= GetGlobalResourceObject("PersonalOffice_Step5", "CertificateValue").ToString()%></label>
                            <input id="CertValue" type="text" />
                            <br/><p></p>
                            <span id="validationMsgCertValue" class="Red" style="display:none;"><%= GetGlobalResourceObject("PersonalOffice_Step5", "validationMsgCertValue").ToString()%></span>
                        </div>
                        <input type="hidden" id="IsBoolType" />
                        <div class="clearfix" id="btnAddCertificates" >
                            <input type = "button" onclick="AddCertificates()" class="button button-blue" value="<%= GetGlobalResourceObject("PersonalOffice_Step5", "btnAdd").ToString()%>"/>
                        </div>
                        <% if (Model.Certificates.Certs.Count==0) { %> 
                            <div id = "CertBlock" style="width: 464px; overflow-x: scroll; display: none;"><% } else { %> 
                            <div id = "CertBlock" style="width: 464px; overflow-x: scroll; "><% } %>
                            <h4 > <%= GetGlobalResourceObject("PersonalOffice_Step5", "CertificateAdded").ToString()%></h4>
                            <table id="tblOlympiads" class="paginate" style="width:100%; text-align: center; vertical-align:middle; ">
                                <thead>
                                    <tr>
                                        <th style="text-align:center;"> <%= GetGlobalResourceObject("PersonalOffice_Step5", "CertificateType").ToString()%></th> 
                                        <th style="text-align:center;"> <%= GetGlobalResourceObject("PersonalOffice_Step5", "CertificateNumber").ToString()%></th>
                                        <th style="width:35%;text-align:center;"> <%= GetGlobalResourceObject("PersonalOffice_Step5", "CertificateValue").ToString()%></th>
                                        <th style="width:10%;text-align:center;"> <%= GetGlobalResourceObject("PersonalOffice_Step5", "btnDelete").ToString()%></th>
                                    </tr>
                                </thead>
                                <tbody>
                                <% foreach (var cert in Model.Certificates.Certs)
                                    {
                                %>
                                    <tr id='<%= cert.Id.ToString() %>'>
                                        <td style="vertical-align: middle;"><%= Html.Encode(cert.Name) %></td>
                                        <td style="vertical-align: middle;"><%= Html.Encode(cert.Number) %></td>
                                        <% if (cert.BoolType) { %><td style="vertical-align: middle;"> <%= GetGlobalResourceObject("PersonalOffice_Step5", "CertificatePassed").ToString()%> <%} %></td> 
                                        <% if (cert.ValueType){ %><td style="vertical-align: middle;"><%= cert.ValueResult.ToString()%><% }%></td> 
                                        <td style="width:10%; vertical-align: middle;"><%= Html.Raw("<span class=\"link\" onclick=\"DeleteCertificate('" + cert.Id.ToString() + "')\" ><img src=\"../../Content/themes/base/images/delete-icon.png\" alt=\"Удалить\" /></span>")%></td>
                                    </tr>
                                <% } %>
                                </tbody>
                            </table>
                        </div>
                    </div>
                    <hr /> 
                    <% } %>
                    
                    <% if (Model.AddEducationInfo.HasTransfer) { %>
                    <h3><%=GetGlobalResourceObject("PersonalOffice_Step4", "CurrentEducationHeader").ToString()%></h3>
                    <div id="_TransferData" class="clearfix">
                        <% if (!Model.AddEducationInfo.HasReason) { %>
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
                        <hr />
                        <%} %>
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
                                <span id="CurrentEducation_LicenseProgramId_Message" class="Red" style="display:none; border-collapse:collapse;"><%=GetGlobalResourceObject("PersonalOffice_Step4", "CurrentEducation_LicenseProgramId_Message").ToString()%></span>
                             
                            </div>
                            <% if (Model.AddEducationInfo.HasReason) { %>
                            <div class="clearfix" id="_ObrazProg" <% if (String.IsNullOrEmpty(Model.CurrentEducation.HiddenObrazProgramId)) { %>style="display:none;"<% } %>>
                                <%= Html.LabelFor(x => x.CurrentEducation.ObrazProgramId, GetGlobalResourceObject("PersonalOffice_Step4", "CurrentObrazProgram").ToString())%>
                                <%= Html.HiddenFor(x => x.CurrentEducation.HiddenObrazProgramId)%>
                                <select id="CurrentEducation_ObrazProgramId"  <%if (!Model.Enabled){ %> disabled="disabled" <% } %> size="5" name="CurObrazProgram" style="width:459px;"></select>
                                <span id="CurrentEducation_ObrazProgramId_Message" class="Red" style="display:none; border-collapse:collapse;"><%=GetGlobalResourceObject("PersonalOffice_Step4", "CurrentEducation_ObrazProgramId_Message").ToString()%></span>
                            </div>
                            <div class="clearfix">
                                <%= Html.LabelFor(x => x.CurrentEducation.ProfileName,GetGlobalResourceObject("PersonalOffice_Step4", "CurrentProfile").ToString())%>
                                <%= Html.TextBoxFor(x => x.CurrentEducation.ProfileName)%>
                            </div>
                            <%} %>
                        </div>
                        <% if (Model.AddEducationInfo.HasReason) { %>
                        <div id="_Reason"> 
                            <%= Html.LabelFor(x => x.ChangeStudyFormReason.Reason, GetGlobalResourceObject("PersonalOffice_Step4", "ChangeStudyFormReason").ToString())%>
                            <%= Html.TextAreaFor(x => x.ChangeStudyFormReason.Reason, 5, 85, new SortedList<string, object>() { { "class", "noresize" },{ "onchange", "ChangeReason()" } })%>
                            <span id="ChangeStudyFormReasonMessage" class="Red" style="display:none; border-collapse:collapse;"> </span>
                        </div>
                        <% } %>
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
                    <% } %>
                    <% if (Model.AddEducationInfo.HasEGE) { %>
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
                                <th style="width:10%"><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEyear").ToString()%></th>
                                <th style="width:60%"><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEsubject").ToString()%></th>
                                <th style="width:20%"><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEball").ToString()%></th>
                                <th style="width:10%"></th>
                            </tr>
                            </thead>
                            <tbody>
                        <%
                                foreach (var mark in Model.EducationInfo.EgeMarks)
                                {
                        %>
                            <tr id="<%= mark.Id.ToString() %>">
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
                               <div class="clearfix">
                                    <label for="EgeExam"><%=GetGlobalResourceObject("PersonalOffice_Step4", "EGEyear").ToString()%></label><br />
		                            <%= Html.DropDownList("EgeYear", Model.EducationInfo.EgeYearList) %>
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
                    <li><a href="../../Abiturient?step=1"><%= GetGlobalResourceObject("PersonInfo", "Step1")%></a></li>
                    <li><a href="../../Abiturient?step=2"><%= GetGlobalResourceObject("PersonInfo", "Step2")%></a></li>
                    <li><a href="../../Abiturient?step=3"><%= GetGlobalResourceObject("PersonInfo", "Step3")%></a></li>
                    <li><a href="../../Abiturient?step=4"><%= GetGlobalResourceObject("PersonInfo", "Step4")%></a></li>
                    <li><a href="../../Abiturient?step=5"><b><%= GetGlobalResourceObject("PersonInfo", "Step5")%></b></a></li>
                    <li><a href="../../Abiturient?step=6"><%= GetGlobalResourceObject("PersonInfo", "Step6")%></a></li>
                    <li><a href="../../Abiturient?step=7"><%= GetGlobalResourceObject("PersonInfo", "Step7")%></a></li>
                </ol>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderScriptsContent" runat="server">
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="Subheader" runat="server">
        <h2><%= GetGlobalResourceObject("PersonInfo", "QuestionnaireData")%></h2>
</asp:Content>
