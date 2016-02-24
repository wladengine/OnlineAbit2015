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
   .grid_2
   {
       width: 100px;
       display: none;
   }
   .wrapper
   {
       width: 1290px;
   }
   .grid_6
   {
       width: 1290px;
   }
   .first 
   {
       width: 1290px;
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
</style>
    <div>
       Возврат к списку: <a href = "../../Communication/Index"><%="#"+Model.Number.ToString() %></a>
    </div>
    <hr />

    <div>
        <table>
            <tr>
                <td style="vertical-align:top;">
                    <div style="display: table-cell; vertical-align: top; height:50px; " > <%if (Model.Sex) 
                        {%><img src="../../Content/themes/base/images/male.png" alt="male" /><%}else
                        {%><img src="../../Content/themes/base/images/female.png" alt="female" /><%}%> 
                    </div> 
                    <div  style="display: table-cell; vertical-align: top; height:50px; " >
                         <span style="color: #1755d7; font: bold 19px Verdana;"><%=Model.Surname + " " + Model.Name + " " + Model.SecondName%></span>
                    </div>
                </td>
                <td style="width:250px;" rowspan="4">
                    <div style=" max-height:250px; ">
                        <%if (!String.IsNullOrEmpty(Model.Photo)) { %><img src="<% = String.Format("data:image/png;base64,{0}", Model.Photo) %>"  style ="max-width:250px; max-height: 250px;"/> <% } else
                        {%><img src="../../Content/themes/base/images/user_no_photo.png" alt="Photo wasn't found" /><%} %>
                    </div>
                    <br />
                    <div  class="table-div"> Has fee: <%=Model.HasFee %></div>
                    <div  class="table-div"> Has no Fee: <%=Model.HasNoFee %></div>
                </td>
            </tr>
            <tr>
                <td>
                    <div class="table-div">
                        Birthday: <%=Model.DateOfBirth%>
                    </div> 
                    <div class="table-div">
                        Place of Birth: <%=Model.PlaceOfBirth%>
                    </div>
                    <div class="table-div">
                        Nationality: <%=Model.Nationality%>
                    </div>
                    <hr />
                </td>
            </tr>
            <tr>
                <td>
                    <div class="table-div">Email: <%=Model.Email%> </div>
                    <div class="table-div">Posstal Address: <%=Model.PosstalAddress%></div>
                    <hr />
                </td>
            </tr>
            <tr>
                <td>
                    <div class="table-div">Passport scan</div>
                    <div class="table-div">Passport Valid: <%=Model.PassportValid %></div>
                    <div class="table-div">Visa Application Place: <%=Model.VisaApplicationPlace %></div>
                </td>
            </tr> 
        </table>

        <table>
            <tr>
                <td><% %></td>
            </tr>
        </table>

    </div>
</asp:Content>
