<%@ Page Title="" Language="C#"  Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.PersonalOffice>" %>

    <script>
        function UpdScWorks() {
            if ($('#ScWorkYear').val() == '') {
                $('#ScWorkYear_Message_MaxLength').hide();
                $('#ScWorkYear').addClass('input-validation-error');
                return false;
            }
            else {
                if ($('#ScWorkYear').val().length > 50) {
                    $('#ScWorkYear_Message_MaxLength').show();
                    $('#ScWorkYear').addClass('input-validation-error');
                    return false;
                }
                else {
                    $('#ScWorkYear_Message_MaxLength').hide();
                    $('#ScWorkYear').removeClass('input-validation-error');
                }
            }
            if ($('#ScWorkInfo').val() == '') {
                $('#ScWorkInfo').addClass('input-validation-error');
                $('#ScWorkInfo_Message').show();
                return false;
            }
            else {
                $('#ScWorkInfo').removeClass('input-validation-error');
                $('#ScWorkInfo_Message').hide();
            }
            var params = new Object();
            params['ScWorkInfo'] = $('#ScWorkInfo').val();
            params['ScWorkType'] = $('#WorkInfo_ScWorkId').val();
            params['ScWorkYear'] = $('#ScWorkYear').val();
            $.post('Abiturient/UpdateScienceWorks', params, function (res) {
                if (res.IsOk) {
                    $('#ScWorks').show();
                    var output = '';
                    output += '<tr id=\'' + res.Data.Id + '\'><td>';
                    output += res.Data.Type + '</td>';
                    output += '<td>'+res.Data.Year + '</td>';
                    output += '<td>' + res.Data.Info + '</td>';
                    output += '<td><span class="link" onclick="DeleteScWork(\'' + res.Data.Id + '\')" ><img src="../../Content/themes/base/images/delete-icon.png" alt="Удалить оценку" /><span></td>';
                    output += '</tr>';
                    $('#ScWorks tbody').append(output);
                }
                else {
                    alert(res.ErrorMsg);
                }
            }, 'json');
        }
        function DeleteScWork(id) {
            var param = new Object();
            param['id'] = id;
            $.post('Abiturient/DeleteScienceWorks', param, function (res) {
                if (res.IsOk) {
                    $("#" + id).hide(250).html(""); 
                    if (res.Count == 0) {
                        $('#ScWorks').hide();
                    }
                }
                else {
                    alert(res.ErrorMessage);
                }
            }, 'json');
        }
    </script>
                <h4><%= GetGlobalResourceObject("PersonalOffice_Step5", "ResearchWorkHeader").ToString()%></h4>
                <hr />
                <asp:Literal runat="server" Text="<%$Resources:PersonalOffice_Step5, ResearchWorkMessage %>"></asp:Literal>
                    <div class="clearfix">
                        <label><%= GetGlobalResourceObject("PersonalOffice_Step5", "WorkInfo").ToString()%></label>
                        <%= Html.DropDownListFor(x => x.WorkInfo.ScWorkId, Model.WorkInfo.ScWorks)%>
                    </div>
                    <div class="clearfix">
                        <label><%= GetGlobalResourceObject("PersonalOffice_Step5", "WorkYear").ToString()%> </label>
                        <input id="ScWorkYear" />
                    </div>
                    <div class = "clearfix">
                        <span id="ScWorkYear_Message_MaxLength" style="display:none; color:Red;"><asp:Literal runat="server" Text="<%$Resources:PersonInfo, MaxLengthLimit %>"></asp:Literal></span>
                    </div>
                    <div class="clearfix">
                        <textarea class="noresize" id="ScWorkInfo" rows="5" cols="80" style="width: 437px;"></textarea>
                    </div>
                    <div class = "clearfix">
                        <span id="ScWorkInfo_Message" style="display:none; color:Red;"><asp:Literal runat="server" Text="<%$Resources:PersonalOffice_Step5, ScWork_Message %>"></asp:Literal></span>
                    </div>
                    <br />
                    <div class="clearfix">
                        <button id="btnAddScWork" onclick="UpdScWorks()" class="button button-blue"><%= GetGlobalResourceObject("PersonalOffice_Step5", "btnAdd").ToString()%></button>
                    </div>
                    <br /><br />
                    <% if (Model.WorkInfo.pScWorks.Count == 0)
                       { %> <table id="ScWorks" class="paginate" style="display: none; width:100%; text-align: center;"> <%   }
                       else
                       {%>  <table id="ScWorks" class="paginate" style="width:100%; text-align: center;">  <%} %>
                        <thead>
                            <tr>
                                <th><%= GetGlobalResourceObject("PersonalOffice_Step5", "ScWorksType").ToString()%></th>
                                <th><%= GetGlobalResourceObject("PersonalOffice_Step5", "ScWorksYear").ToString()%></th>
                                <th><%= GetGlobalResourceObject("PersonalOffice_Step5", "ScWorksText").ToString()%></th>
                                <th><%= GetGlobalResourceObject("PersonalOffice_Step5", "btnDelete").ToString()%></th>
                            </tr>
                        </thead>
                        <tbody>
                        <% foreach (var scWork in Model.WorkInfo.pScWorks)
                            {
                        %>
                            <tr>
                            <%= Html.Raw(string.Format(@"<tr id=""{0}"">", scWork.Id)) %>
                                <td ><%= Html.Encode(scWork.ScienceWorkType) %></td>
                                <td ><%= Html.Encode(scWork.ScienceWorkYear) %></td>
                                <td><%= Html.Encode(scWork.ScienceWorkInfo) %></td>
                                <td><%= Html.Raw("<span class=\"link\" onclick=\"DeleteScWork('" + scWork.Id.ToString() + "')\" ><img src=\"../../Content/themes/base/images/delete-icon.png\" alt=\"Удалить\" /></span>") %></td>
                            </tr>
                        <% } %>
                        </tbody>
                    </table>
