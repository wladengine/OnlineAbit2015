<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Communication/PersonalOffice.Master" Inherits="System.Web.Mvc.ViewPage<OnlineAbit2013.Models.GlobalCommunicationApplicant>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <%= GetGlobalResourceObject("ApplicationInfo", "Title")%>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Subheader" runat="server">
    <h2><%= GetGlobalResourceObject("ApplicationInfo", "Title")%></h2>
</asp:Content>

<asp:Content ID="HeaderScripts" ContentPlaceHolderID="HeaderScriptsContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <style> 
       .wrapper
       {
           width: 1100px;
       }
       .grid_6
       {
           width: 1100px;
       }
       .first 
       {
           width: 1100px;
       }
       table
       {
           width: 100%;
           font: normal 12px Verdana;
       }
       table, tr, td
       {
        border-collapse: collapse;
        padding: 5px;
       }
       .table-div
       {
           padding:7px;
       }
       .YesNo
       {
           float:left; 
           width: 50px; 
           text-align:center;
       }
       .mini
       {
           padding: 2px;
       }
       .Complete
       {
           width: 65px;
       }
       .Checked
       {
           opacity: 1;
       }
       .NotChecked
       {
           opacity: 0.5;
       }
       .td_pts{
           text-align: right; 
           vertical-align: middle;
           height: 25px;
           min-height: 25px;
       }
       .pts
       {
            min-width: 15px; 
            width: 20px; 
       }
       .filesbody 
       {
           display:none;
       }
       #down1, #down2, #down3, #down4, #down5
       {
           display:none;
       }
    </style>
    <script>
        function YesNoClick(id1, id2) {
            var result;
            if ($(id1).hasClass('Checked')) {
                $(id1).removeClass("Checked");
                $(id1).addClass("NotChecked");
                result = 0;
                $(id2).removeClass("NotChecked");
                $(id2).addClass("Checked");
            }
            else {
                $(id1).removeClass("NotChecked");
                $(id1).addClass("Checked");
                result = 1;
                $(id2).removeClass("Checked");
                $(id2).addClass("NotChecked");
            }
            $.post("../../Communication/ChangeBoolValue", { Barcode: '<%=Model.Number %>', result: result, type: id1.id }, function (data) { }, 'json');
        }
        function UpdateRuPts()
        {
            $.post("../../Communication/UpdateRuPts", {
                Barcode: '<%=Model.Number %>',
                RuPort: $('#RuPortfolioPts').val(),
                RuInt: $('#RuInterviewPts').val(),
            }, function (data) { location.reload();}, 'json');
            
        }
        function UpdateDePts() {
            $.post("../../Communication/UpdateDePts", {
                Barcode: '<%=Model.Number %>',
                DePort: $('#DePortfolioPts').val(),
                DeInt: $('#DeInterviewPts').val(),
            }, function (data) { location.reload(); }, 'json');

        }

        function ShowHide(obj, down, up)
        {
            $(obj).toggle();
            $(down).toggle();
            $(up).toggle();
        }

        function UpdateStatus()
        {
            $.post("../../Communication/UpdateStatus", {
                Barcode: '<%=Model.Number %>',
                StatusId: $('#StatusId').val(),
            }, function (data) { }, 'json');

        }
    </script>
    <div>
       <%=GetGlobalResourceObject("Communication", "BackToOverview")%>: <a href = "../../Communication/Index?sort=<%=Model.SortOrder%>#<%=Model.Number.ToString() %>>"><%="#"+Model.Number.ToString() %></a>
    </div>
    <hr />
    <div>
        <table>
            <tr>
                <td style="vertical-align:top;">
                    <div>
                        <div style="display: table-cell; vertical-align: top; height:50px; padding-right: 15px;" > <%if (Model.Sex) 
                            {%><img src="../../Content/themes/base/images/male.png" alt="male" /><%}else
                            {%><img src="../../Content/themes/base/images/female.png" alt="female" /><%}%> 
                        </div> 
                        <div  style="display: table-cell; vertical-align: top; height:50px; " >
                             <span style="color: #0095CD; font: bold 19px Verdana;"><%=Model.Surname + " " + Model.Name + " " + Model.SecondName%></span>
                        <br />
                            <span style="color: #0095CD; font: bold 17px Verdana; vertical-align: top; height:25px; ""><%=Model.FioEng%></span>
                        </div> 
                    </div>
                    
                    <div class="table-div">
                         <%=GetGlobalResourceObject("Communication", "Birthday")%>: <%=Model.DateOfBirth%>
                    </div> 
                    <div class="table-div">
                        <%=GetGlobalResourceObject("Communication", "PlaceOfBirth")%>: <%=Model.PlaceOfBirth%>
                    </div>
                    <div class="table-div">
                        <%=GetGlobalResourceObject("Communication", "Nationality")%>: <%=Model.Nationality%>
                    </div>
                    <hr />
                    <div class="table-div"><%=GetGlobalResourceObject("Communication", "Email")%>: <%=Model.Email%> </div>
                    <div class="table-div"><%=GetGlobalResourceObject("Communication", "PosstalAddress")%>: <%=Model.PosstalAddress%></div>
                    <hr />
                    <div class="table-div"><%=GetGlobalResourceObject("Communication", "PassportValid")%>: <%=Model.PassportValid %></div>
                    <div class="table-div"><%=GetGlobalResourceObject("Communication", "VisaApplicationPlace")%>: <%=Model.VisaApplicationPlace %></div>
                </td>
                <td style= "width:250px;">
                    <div style = " max-height:250px; ">
                        <%if (!String.IsNullOrEmpty(Model.Photo)) { %><img src="<% = String.Format("data:image/png;base64,{0}", Model.Photo) %>"  style ="max-width:250px; max-height: 250px;"/> <% } else
                        {%><img src="../../Content/themes/base/images/user_no_photo.png" alt="<%=GetGlobalResourceObject("Communication", "NoPhoto")%>" /><%} %>
                    </div>
                    <div>
                        <div  class="table-div"><%=GetGlobalResourceObject("Communication", "HasFee")%>: <%if (Model.HasFee) { %><%=GetGlobalResourceObject("Communication", "Yes")%><% } else { %><%=GetGlobalResourceObject("Communication", "No")%><%} %></div>
                        <div  class="table-div"><%=GetGlobalResourceObject("Communication", "HasNoFee")%>: <%if (Model.HasNoFee) { %><%=GetGlobalResourceObject("Communication", "Yes")%><% } else { %><%=GetGlobalResourceObject("Communication", "No")%><%} %></div>
                    </div>
                </td>
            </tr>
        </table>
        <hr /> 
        <% if (Model.Certificates.Count>0) { %>
        <table style="width: 50%; text-align:center;">
            <tr>
                <th><%=GetGlobalResourceObject("PersonalOffice_Step5", "CertificateType")%></th>
                <th><%=GetGlobalResourceObject("PersonalOffice_Step5", "CertificateNumber")%></th>
                <th><%=GetGlobalResourceObject("PersonalOffice_Step5", "CertificateValue")%></th>
            </tr>
            <% foreach (var x in Model.Certificates) {%>
            <tr> 
                <td> <%=x.TypeName %></td>
                <td > <%=x.Number %></td> 
                <td><%if (x.BoolType) { %>
                    <%=GetGlobalResourceObject("PersonalOffice_Step5", "CertificatePassed") %>
                    <%} else {%>
                    <%=x.Result.ToString() %>
                    <% } %>
                </td>
            </tr>
            <% } %>
        </table>
        <%} %>
        <hr />
        <table>
            <tr>
                <td style="width:33%; text-align:center;"><%=GetGlobalResourceObject("Communication", "Complete")%></td>
                <td style="width:33%; text-align:center;"><%=GetGlobalResourceObject("Communication", "PrintResults")%></td>
                <td style="width:33%; text-align:center;"><%=GetGlobalResourceObject("Communication", "Status")%></td>
            </tr>
            <tr>
                <td style=" vertical-align: top;">
                    <div style =" width: 216px; margin:auto; transform: translateX(6px);">
                        <div id="IsComplete" class="YesNo button button-green Complete <%if (Model.IsComplete) { %>Checked <%} else {%>NotChecked<%}%>"  style="margin: auto;" onclick="YesNoClick(IsComplete, Compl)"><%=GetGlobalResourceObject("Communication", "Yes")%></div>
                        <div id="Compl" class="YesNo button button-green Complete <%if (!Model.IsComplete) { %>Checked <%} else {%>NotChecked<%}%>" onclick="YesNoClick(IsComplete, Compl)"><%=GetGlobalResourceObject("Communication", "No")%></div>
                   </div>
                </td>
                <td style=" text-align:center; vertical-align: top;"><div><input type="button" value ="Print PDF" class="button button-green" onclick="<%= string.Format("window.open('/Communication/GetPrint?Barcode={0}','')", Model.Number.ToString()) %>"/></div></td>
                <td style=" vertical-align: top;"><div style = "margin: auto; width: 190px;"> <%=Html.DropDownListFor(x => x.StatusId, Model.StatusList, new { onchange="UpdateStatus()"})%></div></td>
            </tr>
        </table>
        <hr /> <hr />
        <table>
            <tr>
                <td style="width:50%;">
                    <div id ="Files1Header" onclick="ShowHide(Files1Body, down1, up1)" class ="button-blue"> 
                        <%=GetGlobalResourceObject("Communication", "PassportScan")%>  <span id ="down1">▾</span><span id="up1">▴</span>
                    </div>
                    <div id ="Files1Body" class ="filesbody">
                        <%foreach (var x in Model.lstFiles.Where(x=>x.type == OnlineAbit2013.Models.CommunicationFileType.PassportScan)) { %>
                        <a href="<%= "../../Application/GetFile?id=" + x.Id.ToString("N") %>" target="_blank"><img src="../../Content/themes/base/images/downl1.png" alt="Скачать файл" /></a>
                        <%=x.FileName %><br />
                        <%} %>
                    <hr />
                    </div>
                    <br />
                    <div id ="Files2Header" onclick="ShowHide(Files2Body, down2, up2)" class ="button-blue"> 
                       <%=GetGlobalResourceObject("Communication", "Diploma")%> <span id ="down2">▾</span><span id="up2">▴</span>
                    </div>
                    <div id ="Files2Body" class ="filesbody">
                        <%foreach (var x in Model.lstFiles.Where(x=>x.type == OnlineAbit2013.Models.CommunicationFileType.Diploma)) { %>
                        <a href="<%= "../../Application/GetFile?id=" + x.Id.ToString("N") %>" target="_blank"><img src="../../Content/themes/base/images/downl1.png" alt="Скачать файл" /></a>
                        <%=x.FileName %><br />
                        <%} %>
                    <hr /></div>
                    <br />
                    <div id ="Files3Header" onclick="ShowHide(Files3Body, down3, up3)" class ="button-blue"> 
                       <%=GetGlobalResourceObject("Communication", "EnglishCertificates")%>  <span id ="down3">▾</span><span id="up3">▴</span>
                    </div>
                    <div id ="Files3Body" class ="filesbody">
                        <%foreach (var x in Model.lstFiles.Where(x=>x.type == OnlineAbit2013.Models.CommunicationFileType.EnglishCertificates)) { %>
                        <a href="<%= "../../Application/GetFile?id=" + x.Id.ToString("N") %>" target="_blank"><img src="../../Content/themes/base/images/downl1.png" alt="Скачать файл" /></a>
                        <%=x.FileName %><br />
                        <%} %>
                    <hr /></div>
                    <br />
                    <div id ="Files4Header" onclick="ShowHide(Files4Body, down4, up4)" class ="button-blue"> 
                       <%=GetGlobalResourceObject("Communication", "MotivationLetter")%>  <span id ="down4">▾</span><span id="up4">▴</span>
                    </div>
                    <div id ="Files4Body" class ="filesbody">
                        <%foreach (var x in Model.lstFiles.Where(x=>x.type == OnlineAbit2013.Models.CommunicationFileType.MotivationLetter)) { %>
                        <a href="<%= "../../Application/GetFile?id=" + x.Id.ToString("N") %>" target="_blank"><img src="../../Content/themes/base/images/downl1.png" alt="Скачать файл" /></a>
                        <%=x.FileName %><br />
                        <%} %>
                    <hr /></div>
                    <br />
                    <div id ="Files5Header" onclick="ShowHide(Files5Body, down5, up5)" class ="button-blue"> 
                       <%=GetGlobalResourceObject("Communication", "CV")%> <span id ="down5">▾</span><span id="up5">▴</span>
                    </div>
                    <div id ="Files5Body" class ="filesbody">
                        <%foreach (var x in Model.lstFiles.Where(x=>x.type == OnlineAbit2013.Models.CommunicationFileType.CV)) { %>
                        <a href="<%= "../../Application/GetFile?id=" + x.Id.ToString("N") %>" target="_blank"><img src="../../Content/themes/base/images/downl1.png" alt="Скачать файл" /></a>
                        <%=x.FileName %><br />
                        <%} %>
                    <hr /></div>
                    <br />
                </td>
                <td style="width:50%;">
                    <table> 
                        <tr> 
                            <td class ="td_pts"><%=GetGlobalResourceObject("Communication", "RuPortfolioPts")%></td> 
                            <td><% if (Model.isRussian) { %>
                                <%=Html.TextBoxFor(x => x.RuPortfolioPts, new {style="min-width : 98px; width: 98px;"})%>
                                <% } else { %>
                                <%=Html.TextBoxFor(x => x.RuPortfolioPts, new {style="min-width : 98px; width: 98px;", disabled="disabled"})%>
                                <%} %>  
                            </td>
                            
                            <td class ="td_pts"><%=GetGlobalResourceObject("Communication", "DePortfolioPts")%></td> 
                            <td><% if (Model.isGermany) { %> <%=Html.TextBoxFor(x=>x.DePortfolioPts, new {style="min-width : 98px; width: 98px;"}) %>
                            <% } else { %><%=Html.TextBoxFor(x=>x.DePortfolioPts, new {style="min-width : 98px; width: 98px;",  disabled="disabled"}) %>
                            <%} %> </td>
                            <td> <%=Html.TextBoxFor(x=>x.CommonPortfolioPts, new {style="min-width : 98px; width: 98px;", disabled="disabled"}) %> </td>
                        </tr>
                          <tr> 
                            <td class ="td_pts" ><%=GetGlobalResourceObject("Communication", "Interview")%></td> 
                            <td>
                                <div id="Interview" class="YesNo mini button button-blue  <%if (Model.Interview) { %>Checked <%} else{%>NotChecked<%}%>" onclick="YesNoClick(Interview, Int)"><%=GetGlobalResourceObject("Communication", "Yes")%></div>
                                <div id="Int" class="YesNo mini button button-blue  <%if (!Model.Interview) { %>Checked <%} else{%>NotChecked<%}%>" onclick="YesNoClick(Interview, Int)"><%=GetGlobalResourceObject("Communication", "No")%></div>
                            </td>
                            <td colspan="3"></td>
                        </tr>
                        <tr> 
                            <td class ="td_pts"><%=GetGlobalResourceObject("Communication", "RuInterviewPts")%></td>
                            <td>
                                <% if (Model.isRussian) { %> <%=Html.TextBoxFor(x=>x.RuInterviewPts, new {style="min-width : 98px; width: 98px;"}) %>
                                <% } else { %> <%=Html.TextBoxFor(x=>x.RuInterviewPts, new {style="min-width : 98px; width: 98px;", disabled="disabled"}) %>
                                <%} %>  
                            </td>
                            <td class ="td_pts"> 
                                <%=GetGlobalResourceObject("Communication", "DeInterviewPts")%></td>
                            <td>
                                <% if (Model.isGermany) { %>   <%=Html.TextBoxFor(x=>x.DeInterviewPts, new {style="min-width : 98px; width: 98px;"}) %>
                            <% } else { %><%=Html.TextBoxFor(x=>x.DeInterviewPts, new {style="min-width : 98px; width: 98px;" , disabled="disabled"}) %>
                            <%} %>  </td>
                            <td> <%=Html.TextBoxFor(x=>x.CommonInterviewPts, new {style="min-width : 98px; width: 98px;", disabled="disabled"}) %> </td>
                        </tr>
                        <tr> 
                            <td class ="td_pts" ><%=GetGlobalResourceObject("Communication", "OverallPts")%></td>
                            <td><%=Html.TextBoxFor(x=>x.OverallPts, new {style="min-width : 98px; width: 98px;", disabled="disabled"}) %> </td>
                            <td colspan="3"></td>
                        </tr>
                        <tr>
                            <% if (Model.isRussian) {  %>
                            <td colspan="5" style="text-align:right;"> <input type="button" value ="<%=GetGlobalResourceObject("Communication", "Submit")%>" onclick="UpdateRuPts()" class="button button-green"/></td>
                            <% } else { %>
                            <td colspan="5" style="text-align:right;"> <input type="button" value ="<%=GetGlobalResourceObject("Communication", "Submit")%>" onclick="UpdateDePts()" class="button button-green"/></td>
                            <%} %>
                        </tr>
                    </table>
                   
                </td>
            </tr>
        </table>

    </div>
</asp:Content>
