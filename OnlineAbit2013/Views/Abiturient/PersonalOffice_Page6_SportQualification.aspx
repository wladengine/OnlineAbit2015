<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.PersonalOffice>" %>
    <script type="text/javascript">
        $(function () {
            $('#form').submit(function () {
                return CheckForm();
            })
            $('#PrivelegeInfo_SportQualificationId').change(function () { setTimeout(fChangeSportQualification); });
        });
        function CheckForm() {
            var res = true;
            if (!fCheckLengthSportQualification()) { res = false; }
            return res;
        }
        function fChangeSportQualification() {
            var val = $('#PrivelegeInfo_SportQualificationId').val();
            if (val == 44) {
                $('#PrivelegeInfo_SportQualificationLevel').val("");
                $('#dSportQualificationLevel').hide();
                $('#dOtherSport').show();
                $('#SportQualificationLevel_Message_MaxLength').hide();
            }
            else {
                $('#PrivelegeInfo_OtherSportQualification').val("");
                $('#dSportQualificationLevel').show();
                $('#dOtherSport').hide();
                $('#OtherSportQualification_Message_MaxLength').hide();
            }
        }
        function fCheckLengthSportQualification() {
            var val = $('#PrivelegeInfo_SportQualificationLevel').val();
            if (val.length > 50)
            {
                $('#PrivelegeInfo_SportQualificationLevel').addClass('input-validation-error');
                $('#SportQualificationLevel_Message_MaxLength').show();
                return false;
            }
            else
            {
                $('#SportQualificationLevel_Message_MaxLength').hide();
                $('#PrivelegeInfo_SportQualificationLevel').removeClass('input-validation-error');
            }
            val = $('#PrivelegeInfo_OtherSportQualification').val();
            if (val.length > 500) {
                $('#PrivelegeInfo_OtherSportQualification').addClass('input-validation-error');
                $('#OtherSportQualification_Message_MaxLength').show();
                return false;
            }
            else {
                $('#OtherSportQualification_Message_MaxLength').hide();
                $('#PrivelegeInfo_OtherSportQualification').removeClass('input-validation-error');
            }
            return true;
        }
    </script>
                <form id="form" action="Abiturient/NextStep" method="post">
                    <div class ="form panel">
                    <h3><%= GetGlobalResourceObject("PersonalOffice_Step5", "SportValue").ToString()%></h3>
                    <hr />
                    <div class="form">
                        <div class="clearfix">
                            <%= Html.LabelFor(x => x.PrivelegeInfo.SportQualificationId, GetGlobalResourceObject("PersonalOffice_Step5", "SportQualification").ToString())%>
                            <%= Html.DropDownListFor(x => x.PrivelegeInfo.SportQualificationId, Model.PrivelegeInfo.SportQualificationList) %>
                        </div>
                        <div id="dSportQualificationLevel" class="clearfix">
                            <%= Html.LabelFor(x => x.PrivelegeInfo.SportQualificationLevel, GetGlobalResourceObject("PersonalOffice_Step5", "SportCategory").ToString()) %>
                            <%= Html.TextBoxFor(x => x.PrivelegeInfo.SportQualificationLevel) %>
                        </div>
                        <div class = "clearfix">
                            <span id="SportQualificationLevel_Message_MaxLength" style="display:none; color:Red;"><asp:Literal runat="server" Text="<%$Resources:PersonInfo, MaxLengthLimit %>"></asp:Literal></span>
                        </div>
                        <div id="dOtherSport" class="clearfix" style=" display:none; border-collapse:collapse;">
                            <%= Html.LabelFor(x => x.PrivelegeInfo.OtherSportQualification, GetGlobalResourceObject("PersonalOffice_Step5", "SportQualificationCategory").ToString())%>
                            <%= Html.TextBoxFor(x => x.PrivelegeInfo.OtherSportQualification) %>
                        </div>
                        <div class = "clearfix">
                            <span id="OtherSportQualification_Message_MaxLength" style="display:none; color:Red;"><asp:Literal runat="server" Text="<%$Resources:PersonInfo, MaxLengthLimit %>"></asp:Literal></span>
                        </div>
                    </div>
                    <input name="Stage" type="hidden" value="<%= Model.Stage %>" />
                    </div>
                    <input id="Submit4" class="button button-green" type="submit" value="<%= GetGlobalResourceObject("PersonInfo", "ButtonSubmitText").ToString()%>" />
                </form>
         