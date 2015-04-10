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
    <script type="text/javascript" src="../../Scripts/jquery.ui.datepicker-ru.js"></script>
    <style>
	    .ui-autocomplete {
		    max-height: 200px;
		    max-width: 400px;
		    overflow-y: auto;
		    /* prevent horizontal scrollbar */
		    overflow-x: hidden;
		    /* add padding to account for vertical scrollbar */
		    padding-right: 20px;
	    }
	    /* IE 6 doesn't support max-height
	        * we use height instead, but this forces the menu to always be this tall
	        */
	    * html .ui-autocomplete {
		    height: 200px;
	    }
    </style> 
    <script type="text/javascript" src="../../Scripts/jquery-ui-1.8.11.js"></script>
    <script type="text/javascript">
        function GetCities(i) {
            if ($('#CountryEducId_' + i).val() == '193') {
                $.post('../../AbiturientNew/GetCityNames', { regionId: $('#RegionEducId_' + i).val() }, function (data) {
                    if (data.IsOk) {
                        $('#SchoolCity_' + i).autocomplete({
                            source: data.List
                        });
                    }
                }, 'json');
            }
        }
        function CheckSchoolName(i) {
            var ret = true;
            if ($('#SchoolName_' + i).val() == '') {
                ret = false;
                $('#SchoolName_' + i).addClass('input-validation-error');
                $('#SchoolName_Message_' + i).show();
            }
            else {
                $('#SchoolName_' + i).removeClass('input-validation-error');
                $('#SchoolName_Message_' + i).hide();
            }
            if ($('#SchoolName_' + i).val() == "Санкт-Петербургский государственный  университет (СПбГУ)") {
                $('#_AccreditationInfo_' + i).hide();
                $('#SchoolCity_' + i).val("Санкт-Петербург");
            }
            else {
                $('#_AccreditationInfo_' + i).show();
            }
            var SchoolTypeId = $('#SchoolTypeId_' + i).val();
            if (SchoolTypeId == '4') {
                if (($('#VuzAdditionalTypeId_' + i).val() == 2) || ($('#VuzAdditionalTypeId_' + i).val() == 3)){
                    $('#SchoolName_' + i).val("Санкт-Петербургский государственный  университет (СПбГУ)");
                    $('#SchoolCity_' + i).val("Санкт-Петербург"); 
                    }
            }
            return ret;
        }
        function CheckSchoolExitYear(i) {
            var ret = true; 
            if ($('#SchoolExitYear_' + i).val() == '') {
                ret = false;
                $('#SchoolExitYear_' + i).addClass('input-validation-error');
                $('#SchoolExitYear_Message_' + i).show();
                $('#SchoolExitYear_MessageFormat_' + i).hide();
            }
            else {
                $('#SchoolExitYear_' + i).removeClass('input-validation-error');
                $('#SchoolExitYear_Message_' + i).hide(); 
                var regex = /^\d{4}$/i;
                var val = $('#SchoolExitYear_' + i).val();
                if (!regex.test(val)) {
                    $('#SchoolExitYear_' + i).addClass('input-validation-error');
                    $('#SchoolExitYear_MessageFormat_' + i).show();
                    ret = false;
                }
                else {
                    $('#SchoolExitYear_' + i).removeClass('input-validation-error');
                    $('#SchoolExitYear_MessageFormat_' + i).hide();
                }
            }
            return ret;
        }
        function UpdateAfterSchooltype(i) {
            $('#LabelEducationInfo_VuzExitYear').hide();
            $('#LabelEducationInfo_SchoolExitYear').show();
            $('#LabelEducationInfo_SchoolCurName').hide();
            $('#LabelEducationInfo_SchoolName').show();
            $('#LabelEducationInfo_CountryCurEducId').hide();
            $('#LabelEducationInfo_CountryEducId').show();
            var SchoolTypeId = $('#SchoolTypeId_' + i).val();
            if (SchoolTypeId == '1') {
                $('#_vuzAddType_' + i).hide();
                $('#_schoolExitClass_' + i).show();
                $('#_SchoolNumber_' + i).show();
                $('#HEData_' + i).hide();
                LoadAutoCompleteValues(i);
            }
            else if (SchoolTypeId == '4') {
                $('#_vuzAddType_' + i).show();
                $('#_schoolExitClass_' + i).hide();
                $('#_SchoolNumber_' + i).hide();
                UpdateVuzAddType(i);
                $('#HEData_' + i).show();
                LoadAutoCompleteValues(i);
            }
            else {
                $('#_vuzAddType_' + i).hide();
                $('#_schoolExitClass_' + i).hide();
                $('#_SchoolNumber_' + i).hide();
                $('#HEData_' + i).hide();
                LoadAutoCompleteValues(i);
            }
        }
        function UpdateVuzAddType(i) {
            if ($('#VuzAdditionalTypeId_' + i).val() == 2) {
                $('#_TransferHasScholarship_' + i).show();
                $('#SchoolCity_' + i).val("Санкт-Петербург");
                $('#SchoolName_' + i).val("Санкт-Петербургский государственный  университет (СПбГУ)");
                $('#_ForeignCountryEduc_' + i).hide();
                $('#LabelEducationInfo_VuzExitYear_' + i).show();
                $('#LabelEducationInfo_SchoolExitYear_' + i).hide();
                $('#LabelEducationInfo_SchoolCurName_' + i).show();
                $('#LabelEducationInfo_SchoolName_' + i).hide();
                $('#LabelEducationInfo_CountryCurEducId_' + i).show();
                $('#LabelEducationInfo_CountryEducId_' + i).hide();
            }
            else { 
                if ($('#VuzAdditionalTypeId_' + i).val() == 4) {
                    if ($('#CountryEducId_' + i).val() == '193') { $('#_TransferHasScholarship_' + i).show(); }
                    else { $('#_TransferHasScholarship_' + i).hide(); }
                    if ($('#SchoolName_' + i).val() == "Санкт-Петербургский государственный  университет (СПбГУ)") {
                        $('#_AccreditationInfo_' + i).hide(); 
                    }
                    else {
                        $('#_AccreditationInfo_' + i).show();
                    } 
                    $('#LabelEducationInfo_VuzExitYear_' + i).show();
                    $('#LabelEducationInfo_SchoolExitYear_' + i).hide();
                    $('#LabelEducationInfo_SchoolCurName_' + i).show();
                    $('#LabelEducationInfo_SchoolName_' + i).hide();
                    $('#LabelEducationInfo_CountryCurEducId_' + i).show();
                    $('#LabelEducationInfo_CountryEducId_' + i).hide();
                }
                else {
                    if ($('#VuzAdditionalTypeId_' + i).val() == 4) {
                        $('#SchoolName_' + i).val("Санкт-Петербургский государственный  университет (СПбГУ)");
                        $('#SchoolCity_' + i).val("Санкт-Петербург");
                        $('#LabelEducationInfo_VuzExitYear_' + i).show();
                        $('#LabelEducationInfo_SchoolExitYear_' + i).hide();
                        $('#LabelEducationInfo_SchoolCurName_' + i).show();
                        $('#LabelEducationInfo_SchoolName_' + i).hide();
                        $('#LabelEducationInfo_CountryCurEducId_' + i).show();
                        $('#LabelEducationInfo_CountryEducId_' + i).hide();
                    }
                    else
                        if ($('#VuzAdditionalTypeId_' + i).val() == 3) {
                            $('#SchoolName_' + i).val("Санкт-Петербургский государственный  университет (СПбГУ)");
                            $('#SchoolCity_' + i).val("Санкт-Петербург");
                            $('#LabelEducationInfo_VuzExitYear_' + i).show();
                            $('#LabelEducationInfo_SchoolExitYear_' + i).hide();
                            $('#LabelEducationInfo_SchoolCurName_' + i).show();
                            $('#LabelEducationInfo_SchoolName_' + i).hide();
                            $('#LabelEducationInfo_CountryCurEducId_' + i).show();
                            $('#LabelEducationInfo_CountryEducId_' + i).hide();
                        }
                        else { 
                            $('#LabelEducationInfo_SchoolCurName_' + i).hide();
                            $('#LabelEducationInfo_SchoolName_' + i).show();
                            $('#LabelEducationInfo_VuzExitYear_' + i).hide();
                            $('#LabelEducationInfo_SchoolExitYear_' + i).show();
                            $('#LabelEducationInfo_CountryCurEducId_' + i).hide();
                            $('#LabelEducationInfo_CountryEducId_' + i).show();
                        }
                }
            }
            $('#_CountryEduc_' + i).show();
            $('#_regionEduc_' + i).show();
            if ($('#CountryEducId_' + i).val() != "193") {
                $('#_regionEduc_' + i).hide();
            }
            if (($('#VuzAdditionalTypeId_' + i).val() == 2) || ($('#VuzAdditionalTypeId_' + i).val() == 3)) {
                $('#_CountryEduc_' + i).hide();
                $('#_regionEduc_' + i).hide();
            }
        }
        function updateRegionEduc(i) {
            if ($('#CountryEducId_' + i).val() == '193') {
                $('#_regionEduc_' + i).show();
            }
            else {
                $('#_regionEduc_' + i).hide();
                if ($('#SchoolName_' + i).val() == "Санкт-Петербургский государственный  университет (СПбГУ)") {
                    $('#SchoolName_' + i).val('');
                    $('#SchoolCity_' + i).val('');
                }
            }
            UpdateAfterSchooltype(i);
        }
        function updateForeignCountryEduc(i) {
            if ($('#CountryEducId_' + i).val() != '193') {
                $('#_ForeignCountryEduc_' + i).show();
            }
            else {
                $('#_ForeignCountryEduc_' + i).hide();
            }
        }

        var cachedVuzNames = false;
        var VuzNamesCache;
        var EmptySource = [];
        function LoadAutoCompleteValues(i) {
            var vals = new Object();
            vals["schoolType"] = 4//$('#EducationInfo_SchoolTypeId').val();
            if (!cachedVuzNames) {
                $.post('/AbiturientNew/LoadVuzNames', vals, function (res) {
                    if (res.IsOk) {
                        VuzNamesCache = res.Values;
                        cachedVuzNames = true;
                        if ($('#EducationInfo_SchoolTypeId').val() == 4) {
                            $('#EducationInfo_SchoolName').autocomplete({
                                source: res.Values
                            });
                        }
                        else {
                            $('#EducationInfo_SchoolName').autocomplete({
                                source: EmptySource
                            });
                        }
                    }
                }, 'json');
            }
            else {
                if ($('#SchoolTypeId_' + i).val() == 4) {
                    $('#SchoolName_' + i).autocomplete({
                        source: VuzNamesCache
                    });
                }
                else {
                    $('#SchoolName_' + i).autocomplete({
                        source: EmptySource
                    });
                }
            }
        }
        function UpdateAfterCountryEduc(i) {
            if ($('#CountryEducId_' + i).val() != 6) {
                $('#CountryMessage_' + i).hide();
            }
            else {
                $('#CountryMessage_' + i).show();
            }
        }

        function CheckForm() {
            var ret = true;
            for (var i = 0; i < <%= Model.EducationInfo.EducationDocumentsMaxCount %>; i++)
            {
                if ($('#_isEnabled_' + i).val() == '1' && !CheckSchoolName(i)) { ret = false; }
                if ($('#_isEnabled_' + i).val() == '1' && !CheckSchoolExitYear(i)) { ret = false; }
            }
            return ret;
        }

        $(function () {
            setTimeout(function() { GetCities(0); });
            setTimeout(function() { UpdateAfterSchooltype(0); });
            for (var i = 0; i < <%= Model.EducationInfo.EducationDocumentsMaxCount %>; i++)
            {
                updateForeignCountryEduc(i);
                if ($('#SchoolTypeId_' + i).val() != 4) {
                    $('#HEData_' + i).hide();
                }
                else {
                    $('#HEData_' + i).show();
                }
                if ($('#SchoolTypeId_' + i).val() == 1) {
                    $('#_SchoolNumber_' + i).show();
                }
                else {
                    $('#_SchoolNumber_' + i).hide();
                }
                //UpdateAfterSchooltype(i);
                UpdateVuzAddType(i);
            }
            <% for (int i = 0; i < Model.EducationInfo.EducationDocuments.Count; i++ )
               {
                   var Doc = Model.EducationInfo.EducationDocuments[i]; %>
            $('#SchoolTypeId_<%= i %>').val('<%= Doc.SchoolTypeId %>');
            UpdateAfterSchooltype(<%= i %>);
            $('#VuzAdditionalTypeId_<%= i %>').val('<%= Doc.VuzAdditionalTypeId %>');
            $('#PersonQualification_<%= i %>').val('<%= Doc.PersonQualification %>');
            $('#SchoolExitClassId_<%= i %>').val('<%= Doc.SchoolExitClassId %>');
            $('#CountryEducId_<%= i %>').val('<%= Doc.CountryEducId %>');
            $('#RegionEducId_<%= i %>').val('<%= Doc.RegionEducId %>');
            setTimeout(function() { GetCities(<%= i %>); });
            $('#PersonStudyForm_<%= i %>').val('<%= Doc.StudyFormId %>');
            $('#PersonQualification_<%= i %>').val('<%= Doc.PersonQualification %>');
            <% } %>
        });
    </script>
    <script type="text/javascript">
        function DeleteMsg(id) {
            var p = new Object();
            p["id"] = id;
            $.post('/AbiturientNew/DeleteMsg', p, function (res) {
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

        function AddNext(i) {
            var nxt = i + 1;
            $('#_linkAdd_' + i).hide();
            $('#div_' + nxt).show(250);
            $('#_isEnabled_' + nxt).val('1');
            setTimeout(function() { GetCities(nxt); });
        }
        function DeleteDoc(i) {
            var prev = i - 1;
            $('#_linkAdd_' + prev).show();
            $('#div_' + i).hide(250);
            $('#_isEnabled_' + i).val('0');
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
                <form id="form" action="AbiturientNew/NextStep" method="post" onsubmit="return CheckForm();">
                    <h3><%= GetGlobalResourceObject("PersonalOffice_Step4", "EducationHeader")%></h3>
                    <h6><%= GetGlobalResourceObject("PersonalOffice_Step4", "EducationDocumentsHeader")%></h6>
                    <hr />
                    <input name="Stage" type="hidden" value="<%= Model.Stage %>" />
                    <fieldset>
                    <% for (int i = 0; i < Model.EducationInfo.EducationDocuments.Count; i++ )
                        {
                            var Doc = Model.EducationInfo.EducationDocuments[i]; %>
                        <div id="div_<%= i %>" class="form panel">
                            <%= Html.Hidden("_sId_" + i, Doc.sId) %>
                            <%= Html.Hidden("_isEnabled_" + i, 1, new Dictionary<string, object> { { "id", "_isEnabled_" + i } }) %>
                            <div class="clearfix">
                                <label for="SchoolTypeId_<%= i %>" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                    <asp:Literal ID="Literal3" runat="server" Text="<%$Resources:PersonalOffice_Step4, SchoolTypeId %>"></asp:Literal>
                                    <asp:Literal ID="Literal4" runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                </label> 
                                <%= Html.DropDownList("SchoolTypeId_" + i, Doc.SchoolTypeList, new Dictionary<string, object> { { "id", "SchoolTypeId_" + i }, { "onchange", "UpdateAfterSchooltype(" + i + ")" } })%>
                            </div>
                            <div id="_vuzAddType_<%= i %>" class="clearfix" style="display:none">
                                <div class="clearfix">
                                    <label for="VuzAdditionalTypeId_<%= i %>" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                        <asp:Literal ID="Literal5" runat="server" Text="<%$Resources:PersonalOffice_Step4, VuzAdditionalTypeId %>"></asp:Literal>
                                        <asp:Literal ID="Literal6" runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                    </label> 
                                    <%= Html.DropDownList("VuzAdditionalTypeId_" + i, Doc.VuzAdditionalTypeList, new Dictionary<string, object> { { "id", "VuzAdditionalTypeId_" + i }, { "onchange", "UpdateVuzAddType(" + i + ")" } }) %>
                                </div>
                            </div>
                            <div id="_schoolExitClass_<%= i %>" class="clearfix" style="display:none">
                                <div class="clearfix">
                                    <label for="SchoolExitClassId_<%= i %>" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                        <asp:Literal ID="Literal7" runat="server" Text="<%$Resources:PersonalOffice_Step4, SchoolExitClass %>"></asp:Literal>
                                        <asp:Literal ID="Literal8"  runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                    </label>
                                    <%= Html.DropDownList("SchoolExitClassId_"+ i, Doc.SchoolExitClassList, new Dictionary<string, object> { { "id", "SchoolExitClassId_" + i } }) %>
                                </div>
                            </div>

                            <div id="_schoolName_<%= i %>" class="clearfix">
                                <label id="LabelEducationInfo_SchoolCurName_<%= i %>" style="display:none;" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                    <asp:Literal ID="Literal13" runat="server" Text="<%$Resources:PersonalOffice_Step4, SchoolCurName %>"></asp:Literal><asp:Literal ID="Literal14" runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                </label>  
                                <label id="LabelEducationInfo_SchoolName_<%= i %>" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                    <asp:Literal ID="Literal15" runat="server" Text="<%$Resources:PersonalOffice_Step4, SchoolName %>"></asp:Literal><asp:Literal ID="Literal16" runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                </label>  
                                <%= Html.TextBox("SchoolName_"+ i, Doc.SchoolName, new Dictionary<string, object> { { "id", "SchoolName_" + i }, { "onchange", "CheckSchoolName(" + i + ")" }, { "onkeyup", "CheckSchoolName(" + i + ")" }, { "onblur", "CheckSchoolName(" + i + ")" } })%>
                                <br /><p></p>
                                <span id="SchoolName_Message_<%= i %>" class="Red" style="display:none">  
                                    <%= GetGlobalResourceObject("PersonalOffice_Step4", "EducationInfo_SchoolName_Message").ToString()%> 
                                </span>
                            </div>
                            <div id="_SchoolNumber_<%= i %>" class="clearfix">
                                <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "SchoolNumber").ToString(), new Dictionary<string, object> { { "for", "SchoolNumber_" + i } })%>
                                <%= Html.TextBox("SchoolNumber_" + i, Doc.SchoolNumber, new Dictionary<string, object> { { "id", "SchoolNumber_" + i } }) %>
                            </div>

                            <div id="_CountryEduc_<%= i %>" class="clearfix">
                                <label id="LabelEducationInfo_CountryCurEducId_<%= i %>" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                    <asp:Literal ID="Literal1" runat="server" Text="<%$Resources:PersonalOffice_Step4, CountryCurEducId %>"></asp:Literal>
                                    <asp:Literal ID="Literal2" runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                </label> 
                                <label id="LabelEducationInfo_CountryEducId_<%= i %>" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                    <asp:Literal ID="Literal9" runat="server" Text="<%$Resources:PersonalOffice_Step4, CountryEducId %>"></asp:Literal>
                                    <asp:Literal ID="Literal10" runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                </label> 
                                <%= Html.DropDownList("CountryEducId_" + i, Doc.CountryList, new Dictionary<string, object> { { "id", "CountryEducId_" + i }, { "onchange", "updateRegionEduc(" + i + "); updateForeignCountryEduc(" + i + "); UpdateAfterCountryEduc(" + i + ")" } }) %>
                            </div>
                            <div id="_regionEduc_<%= i %>" class="clearfix">
                                <div class="clearfix">
                                    <label for="RegionEducId_<%= i %>" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                        <asp:Literal ID="Literal11" runat="server" Text="<%$Resources:PersonalOffice_Step4, RegionEducId %>"></asp:Literal>
                                        <asp:Literal ID="Literal12"  runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                    </label>  
                                    <%= Html.DropDownList("RegionEducId_" + i, Doc.RegionList, new Dictionary<string, object> { { "id", "RegionEducId_" + i }, { "onchange", "GetCities(" + i + ")" } }) %>
                                </div>
                            </div>
                            <div class="clearfix">
                                <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "ResidentialPlace").ToString(), new Dictionary<string, object> { { "for", "SchoolCity_" + i } } )%>
                                <%= Html.TextBox("SchoolCity_" + i, Doc.SchoolCity, new Dictionary<string, object> { { "id", "SchoolCity_" + i } }) %>
                            </div>
                            <div class="clearfix">
                                <label id="LabelEducationInfo_VuzExitYear_<%= i %>" style="display: none;" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                    <asp:Literal ID="Literal17" runat="server" Text="<%$Resources:PersonalOffice_Step4, VuzExitYear %>"></asp:Literal>
                                    <asp:Literal ID="Literal18" runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                </label>
                                <label id="LabelEducationInfo_SchoolExitYear_<%= i %>" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                    <asp:Literal ID="Literal19" runat="server" Text="<%$Resources:PersonalOffice_Step4, SchoolExitYear %>"></asp:Literal>
                                    <asp:Literal ID="Literal20" runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                </label>
                                <%= Html.TextBox("SchoolExitYear_" + i, Doc.SchoolExitYear, new Dictionary<string, object> { { "id", "SchoolExitYear_" + i }, { "onchange", "CheckSchoolExitYear(" + i + ")" }, { "onkeyup", "CheckSchoolExitYear(" + i + ")" }, { "onblur", "CheckSchoolExitYear(" + i + ")" } })%>
                                <br /><p></p>
                                <span id="SchoolExitYear_Message_<%= i %>" class="Red" style="display:none; border-collapse:collapse;">
                                    <%=GetGlobalResourceObject("PersonalOffice_Step4", "SchoolExitYear_Message").ToString()%>
                                </span>
                                <span id="SchoolExitYear_MessageFormat_<%= i %>" class="Red" style="display:none; border-collapse:collapse;">
                                    <%=GetGlobalResourceObject("PersonalOffice_Step4", "EducationInfo_SchoolExitYear_MessageFormat").ToString()%>
                                </span>
                            </div>
                            <hr />
                            <div class="clearfix">
                                <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "DiplomSeries").ToString(), new Dictionary<string, object> { { "for", "Series_" + i } })%>
                                <%= Html.TextBox("Series_" + i, Doc.Series, new Dictionary<string, object> { { "id", "Series_" + i } }) %>
                                <br /><p></p>
                                <span id="EducationInfo_DiplomSeries_Message_<%= i %>" class="Red" style="display:none">
                                    <%=GetGlobalResourceObject("PersonalOffice_Step4", "DiplomSeries_Message").ToString()%>
                                </span>
                            </div>
                            <div class="clearfix">
                                <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "DiplomNumber").ToString(), new Dictionary<string, object> { { "for", "Number_" + i } })%>
                                <%= Html.TextBox("Number_" + i, Doc.Number, new Dictionary<string, object> { { "id", "Number_" + i } })%>
                                <br /><p></p>
                                <span id="EducationInfo_DiplomNumber_Message_<%= i %>" class="Red" style="display:none">
                                    <%=GetGlobalResourceObject("PersonalOffice_Step4", "DiplomNumber_Message").ToString()%>
                                </span>
                            </div>
                            
                            <div id="AvgMark" class="clearfix">
                                <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "AvgMark").ToString(), new Dictionary<string, object> { { "for", "AvgMark_" + i } })%>
                                <%= Html.TextBox("AvgMark_" + i, Doc.AvgMark, new Dictionary<string, object> { { "id", "AvgMark_" + i } }) %>
                            </div>
                            <div id="_IsExcellent" class="clearfix">
                                <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "RedDiploma").ToString(), new Dictionary<string, object> { { "for", "IsExcellent_" + i } })%>
                                <%= Html.CheckBox("IsExcellent_"+ i, Doc.IsExcellent)%>
                            </div>

                            <div id="_ForeignCountryEduc_<%= i %>" class="clearfix" style="display:none">
                                <div class="clearfix">
                                    <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "IsEqual").ToString(), new Dictionary<string, object> { { "for", "IsEqual_" + i } })%>
                                    <%= Html.CheckBox("IsEqual_" + i, Doc.IsEqual, new Dictionary<string, object> { { "id", "IsEqual_" + i } })%>
                                </div>
                                <div class="clearfix">
                                    <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "EqualityDocumentNumber").ToString(), new Dictionary<string, object> { { "for", "EqualityDocumentNumber_" + i } })%>
                                    <%= Html.TextBox("EqualityDocumentNumber_" + i, Doc.EqualityDocumentNumber, new Dictionary<string, object> { { "id", "EqualityDocumentNumber_" + i } }) %>
                                </div>
                            </div>

                            <div id="HEData_<%= i %>">
                                <h4><% =GetGlobalResourceObject("PersonalOffice_Step4", "HEDataHeader").ToString()%></h4>
                                <hr />
                                <div class="clearfix">
                                    <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "PersonSpecialization").ToString(), new Dictionary<string, object> { { "for", "ProgramName_" + i } })%>
                                    <%= Html.TextBox("ProgramName_" + i, Doc.ProgramName, new Dictionary<string, object> { { "id", "ProgramName_" + i } })%>
                                </div>
                                <div class="clearfix">
                                    <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "PersonStudyForm").ToString(), new Dictionary<string, object> { { "for", "PersonStudyForm_" + i } })%>
                                    <%= Html.DropDownList("PersonStudyForm_" + i, Doc.StudyFormList, new Dictionary<string, object> { { "id", "PersonStudyForm_" + i } }) %>
                                </div>
                                <div class="clearfix">
                                    <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "PersonQualification").ToString(), new Dictionary<string, object> { { "for", "PersonQualification_" + i } })%>
                                    <%= Html.DropDownList("PersonQualification_" + i, Model.EducationInfo.QualificationList, new Dictionary<string, object> { { "id", "PersonQualification_" + i } }) %>
                                </div>
                                <div class="clearfix">
                                    <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "DiplomTheme").ToString(), new Dictionary<string, object> { { "for", "DiplomTheme_" + i } })%>
                                    <%= Html.TextArea("DiplomTheme_" + i, Doc.DiplomTheme, 3, 70, new Dictionary<string, object> { { "id", "DiplomTheme_" + i } }) %>
                                </div>
                                <div class="clearfix">
                                    <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "HEEntryYear").ToString(), new Dictionary<string, object> { { "for", "HEEntryYear_" + i } })%>
                                    <%= Html.TextBox("HEEntryYear_" + i, Doc.HEEntryYear, new Dictionary<string, object> { { "id", "HEEntryYear_" + i } }) %>
                                </div> 
                            </div>
                            <hr />
                            <div class="clearfix">
                                <% if (i < Model.EducationInfo.EducationDocumentsMaxCount - 1) { %>
                                <span id="_linkAdd_<%= i %>" class="alink" onclick="AddNext(<%= i %>)">
                                    <img src="../../Content/themes/base/images/add-icon_16px.png" />
                                    Добавить ещё
                                </span>
                                <% } %>
                                <% if (i > 0) { %>
                                <span id="_linkDelete_<%= i %>" class="alink" onclick="DeleteDoc(<%= i %>)">
                                    <img src="../../Content/themes/base/images/delete-icon.png" />
                                    Удалить
                                </span>
                                <% } %>
                            </div>
                        </div>
                    <% } %>
                    <% for (int i = Model.EducationInfo.EducationDocuments.Count; i < Model.EducationInfo.EducationDocumentsMaxCount; i++ ) { %>
                        <div id="div_<%= i %>" class="form panel" <% if (i > 0) { %> style="display:none" <% } %>>
                            <%= Html.Hidden("_sId_" + i, "") %>
                            <%= Html.Hidden("_isEnabled_" + i, i == 0 ? 1 : 0, new Dictionary<string, object> { { "id", "_isEnabled_" + i } }) %>
                            <div class="clearfix">
                                <label for="SchoolTypeId_<%= i %>" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                    <asp:Literal ID="Literal21" runat="server" Text="<%$Resources:PersonalOffice_Step4, SchoolTypeId %>"></asp:Literal>
                                    <asp:Literal ID="Literal22" runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                </label> 
                                <%= Html.DropDownList("SchoolTypeId_" + i, Model.EducationInfo.SchoolTypeList, new Dictionary<string, object> { { "id", "SchoolTypeId_" + i }, { "onchange", "UpdateAfterSchooltype(" + i + ")" } })%>
                            </div>
                            <div id="_vuzAddType_<%= i %>" class="clearfix" style="display:none">
                                <div class="clearfix">
                                    <label for="VuzAdditionalTypeId_<%= i %>" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                        <asp:Literal ID="Literal23" runat="server" Text="<%$Resources:PersonalOffice_Step4, VuzAdditionalTypeId %>"></asp:Literal>
                                        <asp:Literal ID="Literal24" runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                    </label> 
                                    <%= Html.DropDownList("VuzAdditionalTypeId_" + i, Model.EducationInfo.VuzAdditionalTypeList, new Dictionary<string, object> { { "id", "VuzAdditionalTypeId_" + i }, { "onchange", "UpdateVuzAddType(" + i + ")" } }) %>
                                </div>
                            </div>
                            <div id="_schoolExitClass_<%= i %>" class="clearfix" style="display:none">
                                <div class="clearfix">
                                    <label for="SchoolExitClassId_<%= i %>" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                        <asp:Literal ID="Literal25" runat="server" Text="<%$Resources:PersonalOffice_Step4, SchoolExitClass %>"></asp:Literal>
                                        <asp:Literal ID="Literal26"  runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                    </label>
                                    <%= Html.DropDownList("SchoolExitClassId_"+ i, Model.EducationInfo.SchoolExitClassList, new Dictionary<string, object> { { "id", "SchoolExitClassId_" + i } }) %>
                                </div>
                            </div>

                            <div id="_schoolName_<%= i %>" class="clearfix">
                                <label id="LabelEducationInfo_SchoolCurName_<%= i %>" style="display:none;" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                    <asp:Literal ID="Literal33" runat="server" Text="<%$Resources:PersonalOffice_Step4, SchoolCurName %>"></asp:Literal><asp:Literal ID="Literal34" runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                </label>  
                                <label id="LabelEducationInfo_SchoolName_<%= i %>" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                    <asp:Literal ID="Literal35" runat="server" Text="<%$Resources:PersonalOffice_Step4, SchoolName %>"></asp:Literal><asp:Literal ID="Literal36" runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                </label>  
                                <%= Html.TextBox("SchoolName_"+ i, "", new Dictionary<string, object> { { "id", "SchoolName_" + i }, { "onchange", "CheckSchoolName(" + i + ")" }, { "onkeyup", "CheckSchoolName(" + i + ")" }, { "onblur", "CheckSchoolName(" + i + ")" } })%>
                                <br /><p></p>
                                <span id="SchoolName_Message_<%= i %>" class="Red" style="display:none">  
                                    <%= GetGlobalResourceObject("PersonalOffice_Step4", "EducationInfo_SchoolName_Message").ToString()%> 
                                </span>
                            </div>
                            <div id="_SchoolNumber_<%= i %>" class="clearfix">
                                <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "SchoolNumber").ToString(), new Dictionary<string, object> { { "for", "SchoolNumber_" + i } })%>
                                <%= Html.TextBox("SchoolNumber_" + i, "", new Dictionary<string, object> { { "id", "SchoolNumber_" + i } }) %>
                            </div>

                            <div id="_CountryEduc_<%= i %>" class="clearfix">
                                <label id="LabelEducationInfo_CountryCurEducId_<%= i %>" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                    <asp:Literal ID="Literal27" runat="server" Text="<%$Resources:PersonalOffice_Step4, CountryCurEducId %>"></asp:Literal>
                                    <asp:Literal ID="Literal28" runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                </label> 
                                <label id="LabelEducationInfo_CountryEducId_<%= i %>" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                    <asp:Literal ID="Literal29" runat="server" Text="<%$Resources:PersonalOffice_Step4, CountryEducId %>"></asp:Literal>
                                    <asp:Literal ID="Literal30" runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                </label> 
                                <%= Html.DropDownList("CountryEducId_" + i, Model.EducationInfo.CountryList, new Dictionary<string, object> { { "id", "CountryEducId_" + i }, { "onchange", "updateRegionEduc(" + i + "); updateForeignCountryEduc(" + i + "); UpdateAfterCountryEduc(" + i + ")" } }) %>
                            </div>
                            <div id="_regionEduc_<%= i %>" class="clearfix">
                                <div class="clearfix">
                                    <label for="RegionEducId_<%= i %>" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                        <asp:Literal ID="Literal31" runat="server" Text="<%$Resources:PersonalOffice_Step4, RegionEducId %>"></asp:Literal>
                                        <asp:Literal ID="Literal32"  runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                    </label>  
                                    <%= Html.DropDownList("RegionEducId_" + i, Model.EducationInfo.RegionList, new Dictionary<string, object> { { "id", "RegionEducId_" + i }, { "onchange", "GetCities(" + i + ")" } }) %>
                                </div>
                            </div>

                            <div class="clearfix">
                                <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "ResidentialPlace").ToString(), new Dictionary<string, object> { { "for", "SchoolCity_" + i } } )%>
                                <%= Html.TextBox("SchoolCity_" + i, "", new Dictionary<string, object> { { "id", "SchoolCity_" + i } }) %>
                            </div>
                            <div class="clearfix">
                                <label id="LabelEducationInfo_VuzExitYear_<%= i %>" style="display: none;" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                    <asp:Literal ID="Literal37" runat="server" Text="<%$Resources:PersonalOffice_Step4, VuzExitYear %>"></asp:Literal>
                                    <asp:Literal ID="Literal38" runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                </label>
                                <label id="LabelEducationInfo_SchoolExitYear_<%= i %>" title='<asp:Literal runat="server" Text="<%$ Resources:PersonInfo, RequiredField%>"></asp:Literal>'> 
                                    <asp:Literal ID="Literal39" runat="server" Text="<%$Resources:PersonalOffice_Step4, SchoolExitYear %>"></asp:Literal>
                                    <asp:Literal ID="Literal40" runat="server" Text="<%$Resources:PersonInfo, Star %>"></asp:Literal>
                                </label>
                                <%= Html.TextBox("SchoolExitYear_" + i, "", new Dictionary<string, object> { { "id", "SchoolExitYear_" + i }, { "onchange", "CheckSchoolExitYear(" + i + ")" }, { "onkeyup", "CheckSchoolExitYear(" + i + ")" }, { "onblur", "CheckSchoolExitYear(" + i + ")" } })%>
                                <br /><p></p>
                                <span id="SchoolExitYear_Message_<%= i %>" class="Red" style="display:none; border-collapse:collapse;">
                                    <%=GetGlobalResourceObject("PersonalOffice_Step4", "SchoolExitYear_Message").ToString()%>
                                </span>
                                <span id="SchoolExitYear_MessageFormat_<%= i %>" class="Red" style="display:none; border-collapse:collapse;">
                                    <%=GetGlobalResourceObject("PersonalOffice_Step4", "EducationInfo_SchoolExitYear_MessageFormat").ToString()%>
                                </span>
                            </div>
                            <hr />
                            <div class="clearfix">
                                <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "DiplomSeries").ToString(), new Dictionary<string, object> { { "for", "Series_" + i } })%>
                                <%= Html.TextBox("Series_" + i, "", new Dictionary<string, object> { { "id", "Series_" + i } }) %>
                                <br /><p></p>
                                <span id="EducationInfo_DiplomSeries_Message_<%= i %>" class="Red" style="display:none">
                                    <%=GetGlobalResourceObject("PersonalOffice_Step4", "DiplomSeries_Message").ToString()%>
                                </span>
                            </div>
                            <div class="clearfix">
                                <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "DiplomNumber").ToString(), new Dictionary<string, object> { { "for", "Number_" + i } })%>
                                <%= Html.TextBox("Number_" + i, "", new Dictionary<string, object> { { "id", "Number_" + i } })%>
                                <br /><p></p>
                                <span id="EducationInfo_DiplomNumber_Message_<%= i %>" class="Red" style="display:none">
                                    <%=GetGlobalResourceObject("PersonalOffice_Step4", "DiplomNumber_Message").ToString()%>
                                </span>
                            </div>
                            
                            <div id="AvgMark_<%= i %>" class="clearfix">
                                <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "AvgMark").ToString(), new Dictionary<string, object> { { "for", "AvgMark_" + i } })%>
                                <%= Html.TextBox("AvgMark_" + i, "", new Dictionary<string, object> { { "id", "AvgMark_" + i } }) %>
                            </div>
                            <div id="_IsExcellent_<%= i %>" class="clearfix">
                                <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "RedDiploma").ToString(), new Dictionary<string, object> { { "for", "IsExcellent_" + i } })%>
                                <%= Html.CheckBox("IsExcellent_"+ i, false)%>
                            </div>

                            <div id="_ForeignCountryEduc_<%= i %>" class="clearfix" style="display:none">
                                <div class="clearfix">
                                    <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "IsEqual").ToString(), new Dictionary<string, object> { { "for", "IsEqual_" + i } })%>
                                    <%= Html.CheckBox("IsEqual_" + i, false, new Dictionary<string, object> { { "id", "IsEqual_" + i } })%>
                                </div>
                                <div class="clearfix">
                                    <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "EqualityDocumentNumber").ToString(), new Dictionary<string, object> { { "for", "EqualityDocumentNumber_" + i } })%>
                                    <%= Html.TextBox("EqualityDocumentNumber_" + i, "", new Dictionary<string, object> { { "id", "EqualityDocumentNumber_" + i } }) %>
                                </div>
                            </div>

                            <div id="HEData_<%= i %>">
                                <h4><% =GetGlobalResourceObject("PersonalOffice_Step4", "HEDataHeader").ToString()%></h4>
                                <hr />
                                <div class="clearfix">
                                    <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "PersonSpecialization").ToString(), new Dictionary<string, object> { { "for", "ProgramName_" + i } })%>
                                    <%= Html.TextBox("ProgramName_" + i, "", new Dictionary<string, object> { { "id", "ProgramName_" + i } })%>
                                </div>
                                <div class="clearfix">
                                    <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "PersonStudyForm").ToString(), new Dictionary<string, object> { { "for", "PersonStudyForm_" + i } })%>
                                    <%= Html.DropDownList("PersonStudyForm_" + i, Model.EducationInfo.StudyFormList, new Dictionary<string, object> { { "id", "PersonStudyForm_" + i } }) %>
                                </div>
                                <div class="clearfix">
                                    <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "PersonQualification").ToString(), new Dictionary<string, object> { { "for", "PersonQualification_" + i } })%>
                                    <%= Html.DropDownList("PersonQualification_" + i, Model.EducationInfo.QualificationList, new Dictionary<string, object> { { "id", "PersonQualification_" + i } }) %>
                                </div>
                                <div class="clearfix">
                                    <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "DiplomTheme").ToString(), new Dictionary<string, object> { { "for", "DiplomTheme_" + i } })%>
                                    <%= Html.TextArea("DiplomTheme_" + i, "", 3, 70, new Dictionary<string, object> { { "id", "DiplomTheme_" + i } }) %>
                                </div>
                                <div class="clearfix">
                                    <%= Html.Label("", GetGlobalResourceObject("PersonalOffice_Step4", "HEEntryYear").ToString(), new Dictionary<string, object> { { "for", "HEEntryYear_" + i } })%>
                                    <%= Html.TextBox("HEEntryYear_" + i, "", new Dictionary<string, object> { { "id", "HEEntryYear_" + i } }) %>
                                </div> 
                            </div>
                            <hr />
                            <div class="clearfix">
                                <% if (i < Model.EducationInfo.EducationDocumentsMaxCount - 1) { %>
                                <span id="_linkAdd_<%= i %>" class="alink" onclick="AddNext(<%= i %>)">
                                    <img src="../../Content/themes/base/images/add-icon_16px.png" />
                                    <%= GetGlobalResourceObject("PersonalOffice_Step4", "EducationBlockAdd")%>
                                </span>
                                <% } %>
                                <% if (i > 0) { %>
                                <span id="_linkDelete_<%= i %>" class="alink" onclick="DeleteDoc(<%= i %>)">
                                    <img src="../../Content/themes/base/images/delete-icon.png" />
                                    <%= GetGlobalResourceObject("PersonalOffice_Step4", "EducationBlockDelete")%>
                                </span>
                                <% } %>
                            </div>
                        </div>
                    <% } %>
                    </fieldset>
                    <hr />
                    <div class="clearfix">
                        <input id="Submit3" type="submit" class="button button-green" value="<%= GetGlobalResourceObject("PersonInfo", "ButtonSubmitText").ToString()%>" />
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
