<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.PersonalOffice>" %>

<script type="text/javascript">
        $(function () {
            $('#WorkPlace').keyup(function () {
                var str = $('#WorkPlace').val();
                if (str != "") {
                    $('#WorkPlace').removeClass('input-validation-error');
                    $('#validationMsgPersonWorksPlace').hide(); ;
                }
                else {
                    $('#WorkPlace').addClass('input-validation-error');
                    $('#validationMsgPersonWorksPlace').show();
                }
            });
            $('#WorkProf').keyup(function () {
                var str = $('#WorkProf').val();
                if (str != "") {
                    $('#WorkProf').removeClass('input-validation-error');
                    $('#validationMsgPersonWorksLevel').hide(); ;
                }
                else {
                    $('#WorkProf').addClass('input-validation-error');
                    $('#validationMsgPersonWorksLevel').show() ;
                }
            });
            $('#WorkSpec').keyup(function () {
                var str = $('#WorkSpec').val();
                if (str != "") {
                    $('#WorkSpec').removeClass('input-validation-error');
                    $('#validationMsgPersonWorksDuties').hide();
                }
                else {
                    $('#WorkSpec').addClass('input-validation-error');
                    $('#validationMsgPersonWorksDuties').show();
                }
            });
        });
        function AddWorkPlace() {
            var params = new Object();
            params['WorkStag'] = $('#WorkStag').val();
            params['WorkPlace'] = $('#WorkPlace').val();
            params['WorkProf'] = $('#WorkProf').val();
            params['WorkSpec'] = $('#WorkSpec').val();
            var Ok = CheckRegExp();
             
            if (params['WorkPlace'] == "") {
                $('#WorkPlace').addClass('input-validation-error');
                $('#validationMsgPersonWorksPlace').show();
                Ok = false;
            }
            if (params['WorkProf'] == "") {
                $('#WorkProf').addClass('input-validation-error');
                $('#validationMsgPersonWorksLevel').show() ;
                Ok = false;
            }
            if (params['WorkSpec'] == "") {
                $('#WorkSpec').addClass('input-validation-error');
                $('#validationMsgPersonWorksDuties').show();
                Ok = false;
            }
            if (Ok) {
                $.post('Abiturient/AddWorkPlace', params, function (res) {
                    if (res.IsOk) {
                        $('#BlockPersonWorks').show();
                        var info = '<tr id="' + res.Data.Id + '">';
                        info += '<td>' + res.Data.Place + '</td>';
                        info += '<td>' + res.Data.Stag + '</td>';
                        info += '<td>' + res.Data.Level + '</td>';
                        info += '<td>' + res.Data.Duties + '</td>';
                        info += '<td><span class="link" onclick="DeleteWorkPlace(\'' + res.Data.Id + '\')"><img src="../../Content/themes/base/images/delete-icon.png" alt="Удалить" /></span></td>';
                        $('#PersonWorks tbody').append(info);
                    }
                    else {
                        alert(res.ErrorMessage);
                    }
                }, 'json');
            }
        }
        function DeleteWorkPlace(id) {
            var parm = new Object();
            parm["wrkId"] = id;
            $.post('Abiturient/DeleteWorkPlace', parm, function (res) {
                if (res.IsOk) {
                    $('#' + id).hide(250).html('');
                    if (res.Count == 0) {
                        $('#BlockPersonWorks').hide();
                    }
                }
                else {
                    alert(res.ErrorMessage);
                }
            }, 'json');
        }
        function CheckRegExp() {
            var val = $('#WorkStag').val();
            var regex = /^([0-9])+$/i;
            if (val != '') {
                if (!regex.test(val)) {
                    $('#WorkStag').addClass('input-validation-error');
                    $('#validationMsgPersonWorksExperience').show();
                    return false;
                }
                else {
                    $('#WorkStag').removeClass('input-validation-error');
                    $('#validationMsgPersonWorksExperience').hide();
                }
            }
            return true;
        }
    </script>
    
                <div class="form panel">
                <h3><%= GetGlobalResourceObject("PersonalOffice_Step5", "WorkExperienceHeader").ToString()%></h3>
                <hr /> 
                    <div class="clearfix">
                        <label for="WorkStag"><%= GetGlobalResourceObject("PersonalOffice_Step5", "JobExperience").ToString()%></label>
                        <input id="WorkStag" onkeyup="CheckRegExp()" type="text"/>
                        <br /><p></p><span id="validationMsgPersonWorksExperience" style="display:none; color:Red;"><%= GetGlobalResourceObject("PersonalOffice_Step5", "validationMsgPersonWorksExperience").ToString()%></span>
                    </div>
                    <div class="clearfix">
                        <label for="WorkPlace"><%= GetGlobalResourceObject("PersonalOffice_Step5", "JobLocation").ToString()%></label>
                        <input id="WorkPlace" type="text" /><br /><p></p><span id="validationMsgPersonWorksPlace" class="Red"  style="display:none;"><%= GetGlobalResourceObject("PersonalOffice_Step5", "validationMsgPersonWorksPlace").ToString()%></span>
                    </div>
                    <div class="clearfix">
                        <label for="WorkProf"><%= GetGlobalResourceObject("PersonalOffice_Step5", "JobPosition").ToString()%></label>
                        <input id="WorkProf" type="text" /><br /><p></p><span id="validationMsgPersonWorksLevel" class="Red" style="display:none;"><%= GetGlobalResourceObject("PersonalOffice_Step5", "validationMsgPersonWorksLevel").ToString()%></span>
                    </div>
                    <div class="clearfix">
                        <label for="WorkSpec"><%= GetGlobalResourceObject("PersonalOffice_Step5", "JobFunctions").ToString()%></label>
                        <textarea id="WorkSpec" cols="80" rows="4" style="width: 437px;"></textarea>
                    </div>
                    <div>
                        <span id="validationMsgPersonWorksDuties" class="Red" style="display:none;"><%= GetGlobalResourceObject("PersonalOffice_Step5", "validationMsgPersonWorksDuties").ToString()%></span>
                    </div> 
                    <div class="clearfix">
                        <button id="btnAddProfs" onclick="AddWorkPlace()" class="button button-blue"><%= GetGlobalResourceObject("PersonalOffice_Step5", "btnAdd").ToString()%></button>
                    </div>
                <br /><br />
                <% if (Model.WorkInfo.pWorks.Count == 0)
                   { %>  <div id = "BlockPersonWorks" style="width: 464px; overflow-x: scroll; display: none;"> <% }
                   else
                   { %> <div id = "BlockPersonWorks" style="width: 464px; overflow-x: scroll; "> <%} %>
                        <table id="PersonWorks" class="paginate" style="width:100%; text-align: center; vertical-align:middle; " >
                            <thead>
                                <tr>
                                    <th><%= GetGlobalResourceObject("PersonalOffice_Step5", "JobLocation").ToString()%></th>
                                    <th><%= GetGlobalResourceObject("PersonalOffice_Step5", "JobExperience").ToString()%></th>
                                    <th><%= GetGlobalResourceObject("PersonalOffice_Step5", "JobPosition").ToString()%></th>
                                    <th><%= GetGlobalResourceObject("PersonalOffice_Step5", "JobFunctions").ToString()%></th>
                                    <th><%= GetGlobalResourceObject("PersonalOffice_Step5", "btnDelete").ToString()%></th>
                                </tr>
                            </thead>
                            <tbody>
                            <% foreach (var wrk in Model.WorkInfo.pWorks)
                                {
                            %>
                                <tr>
                                <%= Html.Raw(string.Format(@"<tr id=""{0}"">", wrk.Id.ToString())) %>
                                    <td><%= Html.Encode(wrk.Place) %></td>
                                    <td><%= Html.Encode(wrk.Stag) %></td>
                                    <td><%= Html.Encode(wrk.Level) %></td>
                                    <td><%= Html.Encode(wrk.Duties) %></td>
                                    <td><%= Html.Raw(string.Format(@"<span class=""link"" onclick=""DeleteWorkPlace('{0}')""><img src=""../../Content/themes/base/images/delete-icon.png"" alt=""Удалить"" /></span>", wrk.Id.ToString()))%></td>
                                </tr>
                            <% } %>
                            </tbody>
                        </table> 
                    </div>
                </div>
                <br /> 
                