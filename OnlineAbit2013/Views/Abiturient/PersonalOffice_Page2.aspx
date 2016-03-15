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
            $(function () { setTimeout(SetRequiredFields, 50) });
            <% if (!Model.Enabled)
                   { %>
                $('input').attr('readonly', 'readonly');
                $('select').attr('disabled', 'disabled');
                <% } %>
            <% if (Model.Enabled)
            { %>
                SetRequiredFields();
                $("#PassportInfo_PassportDate").datepicker({
                    changeMonth: true,
                    changeYear: true,
                    showOn: "focus",
                    yearRange: '1968:2015',
                    maxDate: "+1D",
                    defaultDate: '-3y'
                });
                $("#PassportInfo_PassportValid").datepicker({
                    changeMonth: true,
                    changeYear: true,
                    showOn: "focus",
                    yearRange: '2014:2034', 
                    defaultDate: '-3y'
                });
                $.datepicker.setDefaults($.datepicker.regional['<%= GetGlobalResourceObject("Common", "DatetimePicker").ToString()%>']);
                <% } %>

                $("form").submit(function () {
                    return CheckForm();
                });
                $('#PassportInfo_PassportType').change(SetRequiredFields);
                //$('#PassportInfo_PassportSeries').change(function () { setTimeout(CheckSeries); });
                $('#PassportInfo_PassportSeries').keyup(function () { setTimeout(CheckSeries); });

                //$('#PassportInfo_PassportNumber').change(function () { setTimeout(CheckNumber); });
                //$('#PassportInfo_PassportAuthor').change(function () { setTimeout(CheckAuthor); });
                //$('#PassportInfo_PassportDate').change(function () { setTimeout(CheckDate); });
                
                $('#PassportInfo_PassportNumber').keyup(function () { setTimeout(CheckNumber); });
                $('#PassportInfo_PassportAuthor').keyup(function () { setTimeout(CheckAuthor); });
                $('#PassportInfo_PassportDate').keyup(function () { setTimeout(CheckDate); });

                $('#PassportInfo_PassportSeries').blur(function () { setTimeout(CheckSeries); });
                $('#PassportInfo_PassportNumber').blur(function () { setTimeout(CheckNumber); });
                $('#PassportInfo_PassportAuthor').blur(function () { setTimeout(CheckAuthor); });
                $('#PassportInfo_PassportDate').blur(function () { setTimeout(CheckDate); });
                setTimeout(UpdatePassportTable());
            });
            function CheckForm() { 
                return CheckSeries() && CheckNumber() && CheckAuthor() && CheckDate() && CheckSnils();
            }
        </script>
    <script type="text/javascript">
        function SetRequiredFields() {
            var undo = $('#label_PassportInfo_PassportNumber').attr('title');
            if ($('#PassportInfo_PassportType').val() != 1) {
                $('#label_PassportInfo_PassportSeries').attr("title", "");
                $('#label_PassportInfo_PassportAuthor').attr("title", "");
                $('#unrequiredfiled_1').hide(); $('#unrequiredfiled_2').hide();
            }
            else {
                $('#label_PassportInfo_PassportSeries').attr("title", undo);
                $('#label_PassportInfo_PassportAuthor').attr("title", undo);
                $('#unrequiredfiled_1').show(); $('#unrequiredfiled_2').show();
            }
            var val = $('#PassportInfo_PassportType').val();
            if ((val == 1) || (val == 2) || (val == 6)) {
                $('#snils').show();
            }
            else {
                $('#snils').hide();
            } 
            CheckSeries();
            CheckNumber();
            CheckAuthor();
            CheckDate();
        }
        function CheckSeries() {
            var ret = true;
            var val = $('#PassportInfo_PassportSeries').val();
            var ruPassportRegex = /^\d{4}$/i;
                
            $('#PassportInfo_PassportSeries').removeClass('input-validation-error');
            $('#PassportInfo_PassportSeries_Message').hide();
            $('#PassportInfo_PassportSeries_Message_2').hide();
            $('#PassportInfo_PassportSeries_Message_3').hide();
            if ($('#PassportInfo_PassportType').val() == '1' && val == '') {
                ret = false;
                $('#PassportInfo_PassportSeries').addClass('input-validation-error');
                $('#PassportInfo_PassportSeries_Message').show(); 
            }
            else { 
                if ($('#PassportInfo_PassportType').val() == '1' && !ruPassportRegex.test(val)) 
                {
                    ret = false;
                    $('#PassportInfo_PassportSeries').addClass('input-validation-error'); 
                    $('#PassportInfo_PassportSeries_Message_2').show();
                }
                else 
                { 
                    if (val.length > 10) {
                        $('#PassportInfo_PassportSeries').addClass('input-validation-error'); 
                        $('#PassportInfo_PassportSeries_Message_3').show();
                    } 
                }
            }
            return ret;
        }
        function CheckNumber() {
            var ret = true;
            var val = $('#PassportInfo_PassportNumber').val();
            var ruPassportRegex = /^\d{6}$/i; 
            $('#PassportInfo_PassportNumber').removeClass('input-validation-error');
            $('#PassportInfo_PassportNumber_Message').hide();
            $('#PassportInfo_PassportNumber_Message_2').hide();
            $('#PassportInfo_PassportNumber_Message_3').hide();
            if ($('#PassportInfo_PassportNumber').val() == '') {
                ret = false;
                $('#PassportInfo_PassportNumber').addClass('input-validation-error');
                $('#PassportInfo_PassportNumber_Message').show();
            }
            else {  
                if ($('#PassportInfo_PassportType').val() == '1' && !ruPassportRegex.test(val)) {
                    $('#PassportInfo_PassportNumber').addClass('input-validation-error'); 
                    $('#PassportInfo_PassportNumber_Message_2').show();
                }
                else { 
                    if (val.length > 20) {
                        $('#PassportInfo_PassportNumber').addClass('input-validation-error'); 
                        $('#PassportInfo_PassportNumber_Message_3').show();
                    } 
                }
            }
            return ret;
        }
        function CheckDate() {
            var ret = true;
            if ($('#PassportInfo_PassportDate').val() == '') {
                ret = false;
                $('#PassportInfo_PassportDate').addClass('input-validation-error');
                $('#PassportInfo_PassportDate_Message').show();
            }
            else {
                $('#PassportInfo_PassportDate').removeClass('input-validation-error');
                $('#PassportInfo_PassportDate_Message').hide();
            }
            return ret;
        }
        function CheckAuthor() {
            var ret = true;
            var text = $('#PassportInfo_PassportAuthor_Message_Empty').text();

            if ($('#PassportInfo_PassportType').val() == '1' && $('#PassportInfo_PassportAuthor').val() == '') {
                ret = false;
                $('#PassportInfo_PassportAuthor').addClass('input-validation-error');
                $('#PassportInfo_PassportAuthor_Message').text(text);
                $('#PassportInfo_PassportAuthor_Message').show();
            }
            else {
                $('#PassportInfo_PassportAuthor').removeClass('input-validation-error');
                $('#PassportInfo_PassportAuthor_Message').hide();
            }
            if ($('#PassportInfo_PassportAuthor').val().length >  <%=Model.ConstInfo.PassportAuthor %>) {
                ret = false;
                $('#PassportInfo_PassportAuthor').addClass('input-validation-error');
                var text = $('#MessageMaxLength').text();
                $('#PassportInfo_PassportAuthor_Message').text(text);
                $('#PassportInfo_PassportAuthor_Message').show();
            }
            return ret;
        }
        function CheckSnils() {
            var ret = true;
            return ret;
        }
    </script>
    <script type="text/javascript">
        $(function () {
            $('#fileAttachment').change(ValidateInput);
        });
        function ValidateInput() {
            if ($.browser.msie) {
                var myFSO = new ActiveXObject("Scripting.FileSystemObject");
                var filepath = document.getElementById('fileAttachment').value;
                var thefile = myFSO.getFile(filepath);
                var size = thefile.size;
            } else {
                var fileInput = $("#fileAttachment")[0];
                var size = fileInput.files[0].size; // Size returned in bytes.
            }
            if (size > 4*1024*1024) {// 41943040 = 4Mb
                alert('Too big file for uploading (4Mb - max)');
                //Очищаем поле ввода файла
                document.getElementById('fileAttachment').parentNode.innerHTML = document.getElementById('fileAttachment').parentNode.innerHTML;
            }
        }
        function DeleteFile(id) {
            var p = new Object();
            p["id"] = id;
            $.post('/Abiturient/DeleteSharedFile', p, function (res) {
                if (res.IsOk) {
                    $('#' + id).hide(250).html("");
                }
                else {
                    if (res != undefined) {
                        alert(res.ErrorMessage);
                    }
                }
            }, 'json');
        }
        function GetList() {
            $.post('/Abiturient/GetFileList', null, function (res) {
                if (res.IsOk) {
                    var tbody = '';
                    for (var i = 0; i < res.Data.length; i++) {
                        tbody += '<tr id="' + res.Data[i].Id + '">';
                        tbody += '<td align="center" valign="middle"><a href="' + res.Data[i].Path + '"><img src="../../Content/themes/base/images/downl1.png" alt="Скачать файл" /></a></td>';
                        tbody += '<td>' + res.Data[i].FileName + '</td>';
                        tbody += '<td>' + res.Data[i].FileSize + '</td>';
                        tbody += '<td align="center" valign="middle"><span class="link" onclick="DeleteFile(\'' + res.Data[i].Id + '\')"><img src="../../Content/themes/base/images/delete-icon.png" alt="<%= GetGlobalResourceObject("AddSharedFiles", "Delete") %>" /></span></td>';
                        tbody += '</tr>';
                    }
                    $('#tblFiles tbody').html(tbody);
                }
            }, 'json');
        }
</script>
    <script type="text/javascript">
        function SaveOtherPassport() {
            var sSeries = $('#_frmPassportSeries').val();
            var sNumber = $('#_frmPassportNumber').val();
            var sSurname = $('#_frmSurname').val();
            if (sNumber == '')
                return;
            
            var iType = $('#_frmPassportType').val();
            var params = new Object();
            params['PassportType'] = iType;
            params['PassportSeries'] = sSeries;
            params['PassportNumber'] = sNumber;
            params['PassportSurname'] = sSurname;
            $.post('/Abiturient/AddOtherPassport', params, function(data) {
                if (data.IsOk) {
                    var str = $('#tblPassportData').html();
                    str += '<tr id="' + data.Id + '">';
                    str += '<td>' + data.PassportType + '</td>';
                    str += '<td>' + data.PassportSeries + '</td>';
                    str += '<td>' + data.PassportNumber + '</td>';
                    str += '<td>' + data.PassportSurname + '</td>';
                    str += '<td><span class="link" onclick="DeleteOtherPassport(\'' + data.Id + '\')" ><img src="../../Content/themes/base/images/delete-icon.png" alt="Удалить" /><span></td>';
                    str += '</tr>';
                    $('#tblPassportData').html(str);
                    $('#tblOtherPassports').show();
                }
            }, 'json');
        }

        function UpdatePassportTable() {
            $.post('/Abiturient/GetOtherPassportList', null, function(data) {
                if (data.IsOk) {
                    var str = '';
                    for (var i = 0; i < data.List.length; i++) {
                        str += '<tr id="' + data.List[i].Id + '">';
                        str += '<td>' + data.List[i].PassportType + '</td>';
                        str += '<td>' + data.List[i].PassportSeries + '</td>';
                        str += '<td>' + data.List[i].PassportNumber + '</td>';
                        str += '<td>' + data.List[i].PassportSurname + '</td>';
                        str += '<td><span class="link" onclick="DeleteOtherPassport(\'' + data.List[i].Id + '\')" ><img src="../../Content/themes/base/images/delete-icon.png" alt="Удалить" /><span></td>';
                        str += '</tr>';
                    }
                    $('#tblPassportData').html(str);
                    if (data.List.length == 0) {
                        $('#tblOtherPassports').hide();
                    }
                    else {
                        $('#tblOtherPassports').show();
                    }
                }
            }, 'json');
        }

        function DeleteOtherPassport(id) {
            var params = new Object();
            params['id'] = id;
            $.post('/Abiturient/DeleteOtherPassport', params, function(data) {
                if (data.IsOk) {
                    $('#' + id).html('');
                    if (data.Count == 0) {
                        $('#tblOtherPassports').hide();
                    }
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
                    <span id = "MessageLatinSymbols" style="display: none;"><%= GetGlobalResourceObject("PersonInfo", "MessageLatinSymbols").ToString()%></span>
                    <span id = "MessageMaxLength" style="display: none;"><%= GetGlobalResourceObject("PersonInfo", "MessageMaxLength").ToString()%></span>
                    <form id="form" class="form panel" action="Abiturient/NextStep" method="post" onsubmit="return CheckForm();">
                        <h3><%= GetGlobalResourceObject("PersonalOffice_Step2", "HeaderPassport").ToString()%></h3>
                        <hr />
                        <%= Html.ValidationSummary(GetGlobalResourceObject("PersonInfo", "ValidationSummaryHeader").ToString())%>
                        <input name="Stage" type="hidden" value="<%= Model.Stage %>" />
                        <input name="Enabled" type="hidden" value="<%= Model.Enabled %>" />
                        <div class="clearfix">
                            <label for="PassportInfo_PassportType" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                            <asp:Literal runat="server" Text="<%$Resources:PersonalOffice_Step2, PassportType %>"></asp:Literal><asp:Literal runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                            </label>
                             <%= Html.DropDownListFor(x => x.PassportInfo.PassportType, Model.PassportInfo.PassportTypeList) %>
                        </div>
                        <div class="clearfix">
                            <label id="label_PassportInfo_PassportSeries"  for="PassportInfo_PassportSeries" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                            <asp:Literal runat="server" Text="<%$Resources:PersonalOffice_Step2, PassportSeries %>"></asp:Literal><span id="unrequiredfiled_1"><asp:Literal runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal></span>
                            </label> 
                            <%= Html.TextBoxFor(x => x.PassportInfo.PassportSeries)%>
                            <br /><p></p> 
                            <span id="PassportInfo_PassportSeries_Message" class="Red" style="display:none">
                                <%= GetGlobalResourceObject("PersonalOffice_Step2", "PassportInfo_PassportSeries_Message").ToString()%>
                            </span>
                            <span id="PassportInfo_PassportSeries_Message_2" class="Red" style="display:none">
                                <%= GetGlobalResourceObject("PersonalOffice_Step2", "RF_Series_Message").ToString()%>
                            </span>
                            <span id="PassportInfo_PassportSeries_Message_3" class="Red" style="display:none">
                               <%= GetGlobalResourceObject("PersonalOffice_Step2", "LongValue_Message").ToString()%>
                            </span> 
                        </div>
                        <div class="clearfix">
                            <label id="label_PassportInfo_PassportNumber" for="PassportInfo_PassportNumber" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                            <asp:Literal runat="server" Text="<%$Resources:PersonalOffice_Step2, PassportNumber %>"></asp:Literal><asp:Literal runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                            </label> 
                            <%= Html.TextBoxFor(x => x.PassportInfo.PassportNumber)%>
                            <br /><p></p>
                            <span id="PassportInfo_PassportNumber_Message" class="Red" style="display:none">
                                <%= GetGlobalResourceObject("PersonalOffice_Step2", "PassportInfo_PassportNumber_Message").ToString()%>
                            </span>
                            <span id="PassportInfo_PassportNumber_Message_2" class="Red" style="display:none">
                                <%= GetGlobalResourceObject("PersonalOffice_Step2", "RF_Number_Message").ToString()%> 
                            </span>
                            <span id="PassportInfo_PassportNumber_Message_3" class="Red" style="display:none">
                                <%= GetGlobalResourceObject("PersonalOffice_Step2", "LongValue_Message").ToString()%> 
                            </span> 
                        </div>
                        <div class="clearfix">
                            <label id="label_PassportInfo_PassportAuthor"   for="PassportInfo_PassportAuthor" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                            <asp:Literal runat="server" Text="<%$Resources:PersonalOffice_Step2, PassportAuthor %>"></asp:Literal><span id="unrequiredfiled_2"><asp:Literal runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal></span>
                            </label> 
                            <%= Html.TextBoxFor(x => x.PassportInfo.PassportAuthor)%>
                            <br /><p></p>
                            <span id = "PassportInfo_PassportAuthor_Message_Long" style="display:none">Слишком длинное значение</span>
                            <span id = "PassportInfo_PassportAuthor_Message_Empty"  style="display:none"><%= GetGlobalResourceObject("PersonalOffice_Step2", "PassportInfo_PassportAuthor_Message").ToString()%></span>
                            <span id="PassportInfo_PassportAuthor_Message" class="Red" style="display:none"> 
                            <%= GetGlobalResourceObject("PersonalOffice_Step2", "PassportInfo_PassportAuthor_Message").ToString()%>
                            </span>
                        </div>
                        <div class="clearfix">
                            <label for="PassportInfo_PassportDate" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                            <asp:Literal runat="server" Text="<%$Resources:PersonalOffice_Step2, PassportDate %>"></asp:Literal><asp:Literal runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                            </label> 
                             <%= Html.TextBoxFor(x => x.PassportInfo.PassportDate)%>
                            <br /><p></p>
                            <span id="PassportInfo_PassportDate_Message" class="Red" style="display:none">
                                <%= GetGlobalResourceObject("PersonalOffice_Step2", "PassportInfo_PassportDate_Message").ToString()%>
                            </span>
                        </div>
                        <div class="clearfix">
                            <%= Html.LabelFor(x => x.PassportInfo.PassportCode, GetGlobalResourceObject("PersonalOffice_Step2", "PassportCode").ToString())%>
                            <%= Html.TextBoxFor(x => x.PassportInfo.PassportCode)%>
                        </div> 
                        <div id="snils" class="clearfix" style="display: none;">
                            <%= Html.LabelFor(x => x.PersonInfo.SNILS, GetGlobalResourceObject("PersonalOffice_Step2", "SNILS").ToString(), new { title = GetGlobalResourceObject("PersonalOffice_Step2", "SNILS_title").ToString() })%>
                            <%= Html.TextBoxFor(x => x.PersonInfo.SNILS, new { title = GetGlobalResourceObject("PersonalOffice_Step2", "SNILS_title").ToString() })%>
                        </div>
                        <%  if (Model.res== 4) { %>
                         <div class="clearfix">
                            <%= Html.LabelFor(x => x.PassportInfo.PassportValid, GetGlobalResourceObject("PersonalOffice_Step2", "PassportExpire").ToString())%>
                            <%= Html.TextBoxFor(x => x.PassportInfo.PassportValid)%>
                            <br /><p></p> 
                            <span id="PassportExpireMessage" class="Red" style="display:none;"><%= GetGlobalResourceObject("PersonalOffice_Step2", "PassportExpireMessage").ToString()%></span>
                        </div> 
                        <h4><%= GetGlobalResourceObject("PersonalOffice_Step2", "HeaderVisa").ToString()%></h4>
                        <hr />
                        <div class="clearfix">
                                <%= Html.LabelFor(x => x.VisaInfo.CountryId, GetGlobalResourceObject("PersonalOffice_Step2", "VisaCountryName").ToString())%>
                                <%= Html.DropDownListFor(x => x.VisaInfo.CountryId, Model.VisaInfo.CountryList)%>
                            <br /><p></p>
                            <span id="VisaCountryNameMessage" class="Red" style="display:none;">
                            <%= GetGlobalResourceObject("PersonalOffice_Step2", "VisaCountryNameMessage").ToString()%>
                            </span>
                        </div><br />
                        <div class="clearfix">
                            <%= Html.LabelFor(x => x.VisaInfo.Town, GetGlobalResourceObject("PersonalOffice_Step2", "VisaTownName").ToString())%>
                            <%= Html.TextBoxFor(x => x.VisaInfo.Town) %>
                            <br /><p></p>
                            <span id="VisaTownNameMessage" class="Red" style="display:none;">
                            <%= GetGlobalResourceObject("PersonalOffice_Step2", "VisaTownNameMessage").ToString()%>
                            </span>
                        </div><br />
                        <div class="clearfix">
                            <%= Html.LabelFor(x => x.VisaInfo.PostAddress, GetGlobalResourceObject("PersonalOffice_Step2", "VisaPostAddress").ToString())%>
                            <%= Html.TextBoxFor(x => x.VisaInfo.PostAddress)%>
                            <br /><p></p>
                            <span id="VisaPostAddressMessage" class="Red" style="display:none;">
                            <%= GetGlobalResourceObject("PersonalOffice_Step2", "VisaPostAddressMessage").ToString()%>
                            </span>
                        </div><br /> 
                        <% } %> 
                        <hr />
                        <div> 
                            <asp:Literal ID="Literal15" runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal> - <asp:Literal ID="Literal16" runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>  
                        </div>
                        <div class="clearfix">
                            <input id="Submit2" name = "SubmitSave" class="button button-green" type="submit" style="margin-right: 20px;" value="<%= GetGlobalResourceObject("PersonInfo", "ButtonSaveText").ToString()%>" />
                            <span> </span>
                            <input id="Submit1" class="button button-green" type="submit" value="<%= GetGlobalResourceObject("PersonInfo", "ButtonSubmitText").ToString()%>" />
                        </div>
                        <hr />
                    </form>
                    <div class="form panel">
                        <h5><%= GetGlobalResourceObject("PersonalOffice_Step2", "AnotherPassport").ToString()%></h5>
                        <div>
                            <label><%= GetGlobalResourceObject("PersonalOffice_Step2", "PassportType").ToString()%></label>
                            <%= Html.DropDownList("_frmPassportType", Model.PassportInfo.PassportTypeList, new Dictionary<string, object>() { { "id", "_frmPassportType" } })%>
                            <div class="clearfix">
                                <label><%= GetGlobalResourceObject("PersonalOffice_Step2", "PassportSeries").ToString()%></label>
                                <input id="_frmPassportSeries" type="text" />
                            </div><br />
                            <div class="clearfix">
                                <label><%= GetGlobalResourceObject("PersonalOffice_Step2", "PassportNumber").ToString()%></label>
                                <input id="_frmPassportNumber" type="text" />
                            </div><br />
                            <div class="clearfix">
                                <label><%= GetGlobalResourceObject("PersonalOffice_Step2", "PassportAnotherSurname").ToString()%></label>
                                <input id="_frmSurname" type="text" />
                            </div>
                            <br />
                            <button onclick="SaveOtherPassport()" class="button button-blue"><%= GetGlobalResourceObject("PersonInfo", "ButtonSaveText").ToString()%></button>
                        </div>
                        <br />
                        <table id="tblOtherPassports" class="paginate" style="width:100%">
                            <thead>
                                <tr>
                                    <th><%= GetGlobalResourceObject("PersonalOffice_Step2", "PassportType").ToString()%></th>
                                    <th><%= GetGlobalResourceObject("PersonalOffice_Step2", "PassportSeries").ToString()%></th>
                                    <th><%= GetGlobalResourceObject("PersonalOffice_Step2", "PassportNumber").ToString()%></th>
                                    <th style="width:50%"><%= GetGlobalResourceObject("PersonalOffice_Step2", "PassportSurname").ToString()%></th>
                                </tr>
                            </thead>
                            <tbody id="tblPassportData">
                            </tbody>
                        </table>
                    </div>
 <!-- /////////////////////////////////////////////////////////////////// -->
                    <p class="message info">
                     <asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:AddSharedFiles, PersonalOffice_Passport %>"></asp:Literal>
                    </p> 
                    <h4><%= GetGlobalResourceObject("AddSharedFiles", "PassportLoad") %></h4>
                    <form action="/Abiturient/AddSharedFile" method="post" class="form panel" enctype="multipart/form-data">
                        <fieldset> 
                            <input name="Stage" type="hidden" value="<%=Model.Stage %>" />
                            <div class="clearfix" >
                                <label for="fileAttachment"><%= GetGlobalResourceObject("AddSharedFiles", "File") %></label>
                                <input id="fileAttachment" type="file" name="File" />
                            </div>
                            <div class="clearfix" style="width: 100%; vertical-align : top; ">
                                <input name="FileTypeId" type="hidden" value="1"/>
                                <%= Html.Label(GetGlobalResourceObject("AddSharedFiles", "FileType").ToString())%> 
                                <div style="width:200px; height:30px; overflow: hidden;">
                                     <%= Html.DropDownList("FileTypeId", Model.FileTypes, new { disabled = "disabled"})%>
                                </div> 
                            </div>
                            <div class="clearfix">
                                <label for="fileComment"><%= GetGlobalResourceObject("AddSharedFiles", "Comment") %></label>
                                <textarea id="fileComment" cols="80" rows="5" class="noresize" name="Comment" style="width: 437px;"></textarea>
                            </div>
                            <hr />
                            <div class="clearfix">
                                <input id="btnSubmit" type="submit" class="button button-green" value="<%= GetGlobalResourceObject("AddSharedFiles", "Submit") %>" />
                            </div>
                        </fieldset>
                    </form>
                    <h4><%= GetGlobalResourceObject("AddSharedFiles", "LoadedFiles")%></h4>
                    <% if (Model.Files.Count > 0)
                       { %>
                    <table id="tblFiles" class="paginate" style="width:100%;">
                        <thead>
                            <tr>
                                <th style="width:10%;"><%= GetGlobalResourceObject("AddSharedFiles", "View")%></th>
                                <th><%= GetGlobalResourceObject("AddSharedFiles", "FileName")%></th>
                                <th><%= GetGlobalResourceObject("AddSharedFiles", "LoadDate")%></th>
                                <th><%= GetGlobalResourceObject("AddSharedFiles", "Comment")%></th>
                                <th><%= GetGlobalResourceObject("AddSharedFiles", "Size")%></th>
                                <th style="width:10%;"><%= GetGlobalResourceObject("AddSharedFiles", "Delete")%></th>
                            </tr>
                        </thead>
                        <tbody>
                    <% foreach (var file in Model.Files)
                       { %>
                            <tr id="<%= file.Id.ToString() %>">
                                <td style="vertical-align:middle; text-align:center;"><a href="<%= "../../Abiturient/GetFile?id=" + file.Id.ToString("N") %>" target="_blank"><img src="../../Content/themes/base/images/downl1.png" alt="Скачать файл" /></a></td>
                                <td style="vertical-align:middle; text-align:center;"><%= Html.Encode(file.FileName)%></td>
                                <td style="vertical-align:middle; text-align:center;"><%= Html.Encode(file.LoadDate)%></td>
                                <td style="vertical-align:middle; text-align:center;"><%= Html.Encode(file.Comment)%></td>
                                <td style="vertical-align:middle; text-align:center;"><%= file.FileSize > (2 * 1024 * 1024) ?
                                    Math.Round(((double)file.FileSize / (1024.0 * 1024.0)), 2).ToString() + " Mb"
                                    :
                                    file.FileSize > 1024 ?
                                    Math.Round(((double)file.FileSize / 1024.0), 2).ToString() + " Kb"
                                    : file.FileSize.ToString()%></td>
                                <td style="vertical-align:middle; text-align:center;">
                                    <span class="link" onclick="DeleteFile('<%= file.Id.ToString() %>')">
                                        <img src="../../Content/themes/base/images/delete-icon.png" alt="<%= GetGlobalResourceObject("AddSharedFiles", "Delete") %>" />
                                    </span>
                                </td>
                            </tr>
                    <% } %>
                        </tbody>
                    </table>
                    <% }
                       else
                       { %>
                       <table id="Table1" class="paginate" style="width:100%;"><tr><td>
                    <h5><%= GetGlobalResourceObject("AddSharedFiles", "NoFiles") %></h5>
                    </td></tr></table>
                    <% } %>
                    <br /> 
                    
<!-- /////////////////////////////////////////////////////////////////// -->              
                </div>
                
                <div class="grid_2">
                    <ol>
                        <li><a href="../../Abiturient?step=1"><%= GetGlobalResourceObject("PersonInfo", "Step1")%></a></li>
                        <li><a href="../../Abiturient?step=2"><%= GetGlobalResourceObject("PersonInfo", "Step2")%></a></li>
                        <li><a href="../../Abiturient?step=3"><%= GetGlobalResourceObject("PersonInfo", "Step3")%></a></li>
                        <li><a href="../../Abiturient?step=4"><%= GetGlobalResourceObject("PersonInfo", "Step4")%></a></li>
                        <li><a href="../../Abiturient?step=5"><%= GetGlobalResourceObject("PersonInfo", "Step5")%></a></li>
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
