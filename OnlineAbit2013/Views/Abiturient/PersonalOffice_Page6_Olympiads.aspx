<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.PersonalOffice>" %>

<script type="text/javascript">
    $(function () {
        $('#OlDate').datepicker({
            changeMonth: true,
            changeYear: true,
            showOn: "focus"
        });
        $('#OlympYear').change(function () { setTimeout(UpdateAfterOlympYear); });
        $('#OlympType').change(function () { setTimeout(UpdateAfterOlympType); });
        $('#OlympName').change(function () { setTimeout(UpdateAfterOlympName); });
        $('#OlympSubject').change(function () { setTimeout(UpdateAfterOlympSubject); });
        $('#OlympValue').change(function () { setTimeout(UpdateAfterOlympValue) });
    });

    $.datepicker.regional["ru"];
    function UpdateAfterOlympYear() {
        $('#btnAddOlympiad').hide();
        $('#_OlympType').hide();
        $('#OlympType').html('');
        $('#_OlympName').hide();
        $('#_OlympSubject').hide();
        $('#_OlympValue').hide();
        var param = new Object();
        param['OlympYear'] = $('#OlympYear').val();
        $.post('Abiturient/GetOlympTypeList', param, function (json_data) {
            if (json_data.IsOk) {
                var output = '';
                for (var i = 0; i < json_data.List.length; i++) {
                    output += '<option value="' + json_data.List[i].Id + '">' + json_data.List[i].Name + '</option>';
                }
                $('#OlympType').html(output);
                $('#_OlympType').show();
            }
        }, 'json');
    }
    function UpdateAfterOlympType() {
        $('#btnAddOlympiad').hide();
        $('#_OlympSubject').hide();
        $('#_OlympValue').hide();
        $('#OlympName').html('');
        var param = new Object();
        param['OlympTypeId'] = $('#OlympType').val();
        param['OlympYear'] = $('#OlympYear').val();
        $.post('Abiturient/GetOlympNameList', param, function (json_data) {
            if (json_data.IsOk) {
                var output = '';
                for (var i = 0; i < json_data.List.length; i++) {
                    output += '<option value="' + json_data.List[i].Id + '">' + json_data.List[i].Name + '</option>';
                }
                $('#OlympName').html(output);
                $('#_OlympName').show();
            }
        }, 'json');
    }
    function UpdateAfterOlympName() {
        $('#btnAddOlympiad').hide();
        $('#_OlympValue').hide();
        $('#OlympSubject').html('');
        var param = new Object();
        param['OlympTypeId'] = $('#OlympType').val();
        param['OlympNameId'] = $('#OlympName').val();
        param['OlympYear'] = $('#OlympYear').val();
        $.post('Abiturient/GetOlympSubjectList', param, function (json_data) {
            if (json_data.IsOk) {
                var output = '';
                for (var i = 0; i < json_data.List.length; i++) {
                    output += '<option value="' + json_data.List[i].Id + '">' + json_data.List[i].Name + '</option>';
                }
                $('#OlympSubject').html(output);
                $('#_OlympSubject').show();
            }
        }, 'json');
    }
    function UpdateAfterOlympSubject() {
        $('#btnAddOlympiad').hide();
        $('#OlympValue').val('0');
        $('#_OlympValue').show();
    }
    function UpdateAfterOlympValue() {
        $('#btnAddOlympiad').show();
    }

    function CheckOlSeries() {
        if ($('#OlSeries').val() == '') {
            $('#OlSeries_Message').show();
            $('#OlSeries').addClass('input-validation-error');
            return false;
        }
        else {
            $('#OlSeries_Message').hide();
            $('#OlSeries').removeClass('input-validation-error');
            return true;
        }
    }
    function CheckOlNumber() {
        if ($('#OlNumber').val() == '') {
            $('#OlNumber_Message').show();
            $('#OlNumber').addClass('input-validation-error');
            return false;
        }
        else {
            $('#OlNumber_Message').hide();
            $('#OlNumber').removeClass('input-validation-error');
            return true;
        }
    }
    function CheckOlDate() {
        if ($('#OlDate').val() == '') {
            $('#OlDate_Message').show();
            $('#OlDate').addClass('input-validation-error');
            return false;
        }
        else {
            $('#OlDate_Message').hide();
            $('#OlDate').removeClass('input-validation-error');
            return true;
        }
    }
    function AddOlympiad() {
        var ret = true;
        if (!CheckOlSeries()) { ret = false; }
        if (!CheckOlNumber()) { ret = false; }
        //if (!CheckOlDate()) { ret = false; }
        if (!ret)
            return false;
        var param = new Object();
        param['OlympYear'] = $('#OlympYear').val();
        param['OlympTypeId'] = $('#OlympType').val();
        param['OlympNameId'] = $('#OlympName').val();
        param['OlympSubjectId'] = $('#OlympSubject').val();
        param['OlympValueId'] = $('#OlympValue').val();
        param['Series'] = $('#OlSeries').val();
        param['Number'] = $('#OlNumber').val();
        param['Date'] = $('#OlDate').val();
        $.post('Abiturient/AddOlympiad', param, function (res) {
            if (res.IsOk) {
                $('#OlympBlock').show();
                var output = '';
                output += '<tr id=\'' + res.Id + '\'>';
                output += '<td style="vertical-align: middle;">' + res.Year + '</td>';
                output += '<td style="vertical-align: middle;">' + res.Type + '</td>';
                output += '<td style="vertical-align: middle;">' + res.Name + '</td>';
                output += '<td style="vertical-align: middle;">' + res.Subject + '</td>';
                output += '<td style="vertical-align: middle;">' + res.Value + '</td>';
                output += '<td style="width:35%; vertical-align: middle;">' + res.Doc + '</td>';
                output += '<td style="width:10%; vertical-align: middle;"><span class="link" onclick="DeleteOlympiad(\'' + res.Id + '\')" ><img src="../../Content/themes/base/images/delete-icon.png" alt="Удалить оценку" /><span></td>';
                output += '</tr>';
                $('#tblOlympiads tbody').append(output);
                $('#_OlympType').hide();
                $('#OlympType').html('');
                $('#_OlympName').hide();
                $('#_OlympSubject').hide();
                $('#_OlympValue').hide();
            }
            else {
                alert(res.ErrorMessage);
            }
        }, 'json');
    }
    function DeleteOlympiad(id) {
        var param = new Object();
        param['id'] = id;
        $.post('Abiturient/DeleteOlympiad', param, function (res) {
            if (res.IsOk) {
                $('#' + id).hide();
                if (res.Count == 0) {
                    $('#OlympBlock').hide();
                }
            }
            else {
                alert(res.ErrorMessage);
            }
        }, 'json');
    }
    </script>


<div class="form panel">
                <h3><%= GetGlobalResourceObject("PersonalOffice_Step5", "OlympiadsHeader").ToString()%></h3>
                    <div class="clearfix">
                        <%= Html.DropDownList("OlympYear", Model.PrivelegeInfo.OlympYearList,  new SortedList<string, object>() { {"style", "width:460px;"} , {"size", "4"} }) %>
                    </div>
                    <div class="clearfix" id="_OlympType" style="display:none">
                        <select id ="OlympType" style="width:460px;" size="6"></select>
                    </div>
                    <div class="clearfix" id="_OlympName" style="display:none">
                        <select id ="OlympName" style="width:460px;" size="6"></select>
                    </div>
                    <div class="clearfix" id="_OlympSubject" style="display:none">
                        <select id ="OlympSubject" style="width:460px;" size="6"></select>
                    </div>
                    <div class="clearfix" id="_OlympValue" style="display:none">
                        <%= Html.DropDownList("OlympValue", Model.PrivelegeInfo.OlympValueList, new SortedList<string, object>() { {"style", "width:460px;"} , {"size", "4"} }) %>
                    </div>
                    <div>
                        <h4><%= GetGlobalResourceObject("PersonalOffice_Step5", "DocumentHeader").ToString()%></h4>
                        <hr />
                        <div class="clearfix">
                            <label for="OlSeries"><%= GetGlobalResourceObject("PersonalOffice_Step5", "DocumentSeries").ToString()%></label>
                            <input id="OlSeries" type="text" /><br/><p></p>
                            <span id="OlSeries_Message" style="display:none; color:Red;"><%= GetGlobalResourceObject("PersonalOffice_Step5", "DocumentSeriesMessage").ToString()%></span>
                        </div>
                        <div class="clearfix">
                            <label for="OlNumber"><%= GetGlobalResourceObject("PersonalOffice_Step5", "DocumentNumber").ToString()%></label>
                            <input id="OlNumber" type="text" /><br/><p></p>
                            <span id="OlNumber_Message" style="display:none; color:Red;"><%= GetGlobalResourceObject("PersonalOffice_Step5", "DocumentNumberMessage").ToString()%></span>
                        </div>
                        <div class="clearfix">
                            <label for="OlDate"><%= GetGlobalResourceObject("PersonalOffice_Step5", "DocumentDate").ToString()%></label>
                            <input id="OlDate" type="text" /><br/><p></p>
                            <span id="OlDate_Message" style="display:none; color:Red;"><%= GetGlobalResourceObject("PersonalOffice_Step5", "DocumentDateMessage").ToString()%></span>
                        </div>
                        <hr />
                        <div class="message info">
                            <%= GetGlobalResourceObject("PersonalOffice_Step5", "OlympiadsMessage").ToString()%>
                        </div>
                        <div class="clearfix" id="btnAddOlympiad" >
                            <button onclick="AddOlympiad()" class="button button-blue"> <%= GetGlobalResourceObject("PersonalOffice_Step5", "btnAdd").ToString()%></button>
                        </div>
                        <br />
                       
                            <% if (Model.PrivelegeInfo.pOlympiads.Count==0) { %> 
                            <div id = "OlympBlock" style="width: 464px; overflow-x: scroll; display: none;"><% } else { %> 
                            <div id = "OlympBlock" style="width: 464px; overflow-x: scroll; "><% } %>
                            <h4 > <%= GetGlobalResourceObject("PersonalOffice_Step5", "OlympiadsAdded").ToString()%></h4>
                            <table id="tblOlympiads" class="paginate" style="width:100%; text-align: center; vertical-align:middle; ">
                                <thead>
                                    <tr>
                                        <th style="text-align:center;"> <%= GetGlobalResourceObject("PersonalOffice_Step5", "OlympiadsYear").ToString()%></th>
                                        <th style="text-align:center;"> <%= GetGlobalResourceObject("PersonalOffice_Step5", "OlympiadsType").ToString()%></th>
                                        <th style="text-align:center;"> <%= GetGlobalResourceObject("PersonalOffice_Step5", "OlympiadsName").ToString()%></th>
                                        <th style="text-align:center;"> <%= GetGlobalResourceObject("PersonalOffice_Step5", "OlympiadsSubject").ToString()%></th>
                                        <th style="text-align:center;"> <%= GetGlobalResourceObject("PersonalOffice_Step5", "OlympiadsStatus").ToString()%></th>
                                        <th style="width:35%;text-align:center;"> <%= GetGlobalResourceObject("PersonalOffice_Step5", "DocumentHeader").ToString()%></th>
                                        <th style="width:10%;text-align:center;"> <%= GetGlobalResourceObject("PersonalOffice_Step5", "btnDelete").ToString()%></th>
                                    </tr>
                                </thead>
                                <tbody>
                                <% foreach (var olympiad in Model.PrivelegeInfo.pOlympiads)
                                    {
                                %>
                                    <tr id='<%= olympiad.Id.ToString("N") %>'>
                                        <td style="vertical-align: middle;"><%= Html.Encode(olympiad.OlympYear) %></td>
                                        <td style="vertical-align: middle;"><%= Html.Encode(olympiad.OlympType) %></td>
                                        <td style="vertical-align: middle;"><%= Html.Encode(olympiad.OlympName) %></td>
                                        <td style="vertical-align: middle;"><%= Html.Encode(olympiad.OlympSubject) %></td>
                                        <td style="vertical-align: middle;"><%= Html.Encode(olympiad.OlympValue) %></td>
                                        <td style="width:35%; vertical-align: middle;">
                                            <%= Html.Encode(olympiad.DocumentSeries + " " + olympiad.DocumentNumber + " от " + olympiad.DocumentDate.ToShortDateString())%>
                                        </td>
                                        <td style="width:10%; vertical-align: middle;"><%= Html.Raw("<span class=\"link\" onclick=\"DeleteOlympiad('" + olympiad.Id.ToString("N") + "')\" ><img src=\"../../Content/themes/base/images/delete-icon.png\" alt=\"Удалить\" /></span>")%></td>
                                    </tr>
                                <% } %>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div> 