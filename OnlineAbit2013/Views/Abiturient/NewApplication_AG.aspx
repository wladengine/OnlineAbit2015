﻿<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="OnlineAbit2013.Models" %>
<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Abiturient/PersonalOffice.Master" Inherits="System.Web.Mvc.ViewPage<Mag_ApplicationModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <%= GetGlobalResourceObject("NewApplication", "PageTitle")%>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="Subheader" runat="server">
   <h2> <%= GetGlobalResourceObject("NewApplication", "PageSubheader")%></h2>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<% if (!Model.Enabled)
   {
       if (Model.HasError)
       {  %>   
    <div class="error message"><%= Model.ErrorMessage%></div>  
       <% } %>
<%}
   else
   { %>
<script type="text/javascript">
    var entry;
    $(function () {
        $('#Block<%= Model.Applications.Count + 1 %>').show();
    });  
    var BlockIds = new Object();
    <% for (int i = 1; i <= Model.Applications.Count; i++ ) { %>
    BlockIds['<%= i.ToString() %>'] = '<%= Model.Applications[i - 1].Id.ToString("N") %>';
    <% } %> 

    $(function () {  
        entry = $('#Entry').val();
        GetStudyLevels(<%= Model.Applications.Count + 1 %>);
        //GetProfessions(<%= Model.Applications.Count + 1 %>);
        $('#FinishBtn<%= Model.Applications.Count + 1 %>').hide(); 
    });

    function GetStudyLevels(i) {
        var CurrProfs = '#Profs'+i;
        var CurrlProfession = '#lProfession'+i;
        var CurrlSpecialization = '#lSpecialization'+i;
        var CurrObrazPrograms = '#ObrazPrograms'+i; 
        var CurrObrazProgramsErrors='#ObrazProgramsErrors'+i;
        var CurrlObrazProgram = '#lObrazProgram'+i; 
        var CurrSpecs = '#Specs'+i;
        var CurrFinishBtn = '#FinishBtn'+i; 
        var CurrStudyLevel = '#StudyLevel'+i; 

        $(CurrStudyLevel).hide();
        $(CurrProfs).hide();
        $(CurrObrazPrograms).hide();
        $(CurrSpecs).hide();
        $(CurrFinishBtn).hide();
        $.post('/Abiturient/GetStudyLevels_AG', { studyform: $('#StudyFormId'+i).val(), studybasis: $('#StudyBasisId'+i).val(),
            entry: $('#EntryType').val(), isSecond: $('#IsSecondHidden'+i).val(), isParallel: $('#IsParallelHidden'+i).val(), isReduced : $('#IsReducedHidden'+i).val() }, function (json_data) 
            {
                var options = '';
                if (!json_data.IsOk) {
                    $(CurrObrazProgramsErrors).text(json_data.ErrorMessage).show();
                    $(CurrStudyLevel).attr('disabled', 'disabled').hide();
                    $(CurrlProfession).html('');
                    $(CurrStudyLevel).hide();
                }
                else {
                    $(CurrStudyLevel).show();
                    $(CurrObrazProgramsErrors).text('').hide();
                    for (var i = 0; i < json_data.List.length; i++) {
                        options += '<option value="' + json_data.List[i].Id + '">' + json_data.List[i].Name + '</option>';
                    }
                    $(CurrStudyLevel).html(options).removeAttr('disabled').show();
                    $(CurrlProfession).html('');
                    $(CurrlObrazProgram).html('');
                    $(CurrlSpecialization).html('');
                }  
            }, 'json');
    }

    function GetProfessions(i) {
        var CurrProfs = '#Profs'+i;
        var CurrlProfession = '#lProfession'+i;
        var CurrlSpecialization = '#lSpecialization'+i;
        var CurrObrazPrograms = '#ObrazPrograms'+i; 
        var CurrObrazProgramsErrors='#ObrazProgramsErrors'+i;
        var CurrlObrazProgram = '#lObrazProgram'+i; 
        var CurrSpecs = '#Specs'+i;
        var CurrFinishBtn = '#FinishBtn'+i; 

        $(CurrProfs).show();
        $(CurrObrazPrograms).hide();
        $(CurrSpecs).hide();
        $(CurrFinishBtn).hide();
        $.post('/Abiturient/GetProfs', { studyform: $('#StudyFormId'+i).val(), studybasis: $('#StudyBasisId'+i).val(),
            entry: $('#StudyLevel' + i).val(), isSecond: $('#IsSecondHidden'+i).val(), isParallel: $('#IsParallelHidden'+i).val(), isReduced : $('#IsReducedHidden'+i).val() }, function (json_data) 
            {
                var options = '';
                if (json_data.NoFree) {
                    var text = $('#NewApp_NoFreeEntries').text();
                    $(CurrObrazProgramsErrors).text(text).show();
                    $(CurrlProfession).attr('disabled', 'disabled').hide();
                    $(CurrlObrazProgram).html('');
                    $(CurrProfs).hide();
                }
                else {
                    $(CurrProfs).show();
                    $(CurrObrazProgramsErrors).text('').hide();
                    for (var i = 0; i < json_data.length; i++) {
                        options += '<option value="' + json_data[i].Id + '">' + json_data[i].Name + '</option>';
                    }
                    $(CurrlProfession).html(options).removeAttr('disabled').show();
                    $(CurrlObrazProgram).html('');
                    $(CurrlSpecialization).html('');
                }  
            }, 'json');
    }

    function GetObrazPrograms(i) {
        var CurrProfs = '#Profs'+i;
        var CurrlProfession = '#lProfession'+i;
        var CurrlSpecialization = '#lSpecialization'+i;
        var CurrObrazPrograms = '#ObrazPrograms'+i; 
        var CurrObrazProgramsErrors='#ObrazProgramsErrors'+i;
        var CurrlObrazProgram = '#lObrazProgram'+i; 
        var CurrSpecs = '#Specs'+i;
        var CurrFinishBtn = '#FinishBtn'+i; 

        var profId = $(CurrlProfession).val();
        var sfId = $('#StudyFormId'+i).val();
        flag = false;
        if (profId == null){
            return;
        } 
        $(CurrProfs).show();
        $(CurrObrazPrograms).show();
        $(CurrSpecs).hide();
        $(CurrFinishBtn).hide();
        var I = i;
        $.post('/Abiturient/GetObrazPrograms', { prof: profId, studyform: sfId, studybasis: $('#StudyBasisId'+i).val(), 
            entry: $('#StudyLevel' + i).val(), isParallel: $('#IsParallelHidden'+i).val(), isReduced : $('#IsReducedHidden'+i).val(), 
            semesterId : $('#SemesterId'+i).val() }, function (json_data) {
                var options = '';
                if (json_data.NoFree) {
                    var text = $('#ErrorHasApplication').text();
                    $(CurrObrazProgramsErrors).text(text).show();
                    $(CurrlObrazProgram).attr('disabled', 'disabled').hide();
                    $(CurrlSpecialization).html('');
                }
                else {
                    $(CurrObrazProgramsErrors).text('').hide();
                    for (var i = 0; i < json_data.List.length; i++) {
                        options += '<option value="' + json_data.List[i].Id + '"';
                        if (json_data.List.length == 1)
                        {
                            options += ' selected '; flag = true; 
                        } 
                        options += '>' + json_data.List[i].Name + '</option>';
                    }
                    $(CurrlObrazProgram).html(options).removeAttr('disabled').show();
                    $(CurrlSpecialization).html('');
                    if (flag)
                    {GetSpecializations(I);}
                }
            }, 'json');
    }

    function GetSpecializations(i) { 
        var profId = $('#lProfession'+i).val();
        var opId = $('#lObrazProgram'+i).val();
        var sfId = $('#StudyFormId'+i).val();
        var sbId = $('#StudyBasisId'+i).val()
        if (profId == null || opId == null){
            return;
        } 

        var CurrProfs = '#Profs'+i; 
        var CurrObrazPrograms = '#ObrazPrograms'+i;  
        var CurrObrazProgramsErrors='#ObrazProgramsErrors'+i;        
        var CurrSpecs = '#Specs'+i;
        var CurrlSpecialization = '#lSpecialization'+i;
        var CurrFinishBtn = '#FinishBtn'+i; 

        $(CurrProfs).show();
        $(CurrObrazPrograms).show();
        $(CurrSpecs).hide();
        $(CurrFinishBtn).hide();
        $.post('/Abiturient/GetSpecializations', { prof: profId, obrazprogram: opId, studyform: $('#StudyFormId'+i).val(), 
            studybasis: $('#StudyBasisId'+i).val(), entry: $('#StudyLevel' + i).val(), CommitId: $('#CommitId').val(), isParallel: $('#IsParallelHidden'+i).val(), 
            isReduced : $('#IsReducedHidden'+i).val(), semesterId : $('#SemesterId'+i).val() }, function (json_data) {
                var options = '';
            
                if (json_data.ret.List.length == 1) {
                    $(CurrFinishBtn).show();
                    $(CurrObrazProgramsErrors).text('').hide(); 
                }
                else {
                    if (json_data.ret.NoFree) {
                        var text = $('#ErrorHasApplication').text();
                        $(CurrObrazProgramsErrors).text(text).show();
                        $(CurrlSpecialization).attr('disabled', 'disabled').hide();
                    }
                    else {    
                        for (var i = 0; i< json_data.ret.List.length; i++) {
                            options += '<option value="' + json_data.ret.List[i].Id.toString() + '">' + json_data.ret.List[i].Name.toString() + '</option>';
                        } 
                        $(CurrObrazProgramsErrors).text('').hide();
                        $(CurrlSpecialization).html(options).removeAttr('disabled').show();
                        $(CurrSpecs).show(); 
                    }
                }
            }, 'json');
    }

    function MkBtn(i) { 
        $('#FinishBtn' + i).hide();
        currFinishButton = '#FinishBtn' + i;
        currSpecs = '#Specs' + i;  
        currObrazProgramErrors = '#ObrazProgramsErrors' + i;
        currProfile = '#Profile' + i;
        currProfessions = '#Professions' + i;
        currBlock = '#Block' + i;
        currNeedHostel = '#NeedHostel' + i;
        currBlockData = '#BlockData' + i;
        var nxt = i + 1;
        nextBlock = '#Block' + nxt; 
        currBlockData_Profession = '#BlockData_Profession' + i;
        currBlockData_ObrazProgram = '#BlockData_Profession' + i;
        currBlockData_Specialization = '#BlockData_Specialization' + i;  
      
        $.post('/Abiturient/CheckApplication_Mag', 
            {
                studyform: $('#StudyFormId'+i).val(), 
                studybasis: $('#StudyBasisId'+i).val(), 
                entry: $('#StudyLevel' + i).val(),
                isSecond:  $('#IsSecondHidden'+i).val(), 
                isReduced: $('#IsReducedHidden'+i).val(), 
                isParallel: $('#IsParallelHidden'+i).val(), 
                profession: $('#lProfession'+i).val(), 
                obrazprogram:  $('#lObrazProgram'+i).val(), 
                specialization: $('#lSpecialization'+i).val(), 
                NeedHostel: $('#NeedHostel' + i).is(':checked'), 
                CommitId: $('#CommitId').val() 
            },
            function(json_data) {
                if (json_data.IsOk) {
                    $('#FinishBtn' + i).show();
                }
                else {
                    $(currObrazProgramErrors).text(json_data.ErrorMessage).show();
                }
            }, 'json');
    }

    var nxt = 1;
    function SaveData(i) {
        $('#ObrazProgramsErrors').text('').hide();
        currFinishButton = '#FinishBtn' + i;
        currSpecs = '#Specs' + i;    
        currObrazProgramErrors = '#ObrazProgramsErrors' + i;  
        currNeedHostel = '#NeedHostel' + i;
        
        currBlock = '#Block' + i; 
        currBlockData = '#BlockData' + i;
        nxt = i + 1;
        nextBlock = '#Block' + nxt;

        currBlockData_StudyFormId = '#BlockData_StudyFormId' + i;
        currBlockData_StudyBasisId = '#BlockData_StudyBasisId' + i;
        currBlockData_Profession = '#BlockData_Profession' + i;
        currBlockData_ObrazProgram = '#BlockData_ObrazProgram' + i;
        currBlockData_Specialization = '#BlockData_Specialization' + i; 
        currBlockData_Faculty = '#BlockData_Faculty'+i;

        $.post('/Abiturient/AddApplication_Mag', { 
            priority: i,
            studyform: $('#StudyFormId'+i).val(), 
            studybasis: $('#StudyBasisId'+i).val(), 
            entry: $('#StudyLevel'+i).val(),
            isSecond:  $('#IsSecondHidden'+i).val(), 
            isReduced: $('#IsReducedHidden'+i).val(), 
            isParallel: $('#IsParallelHidden'+i).val(), 
            profession: $('#lProfession'+i).val(), 
            obrazprogram: $('#lObrazProgram'+i).val(), 
            specialization: $('#lSpecialization'+i).val(), 
            NeedHostel: $('#NeedHostel' + i).is(':checked'), 
            CommitId: $('#CommitId').val() 
        }, 
          function(json_data) {
              if (json_data.IsOk) { 
                  $(currBlockData_StudyFormId).text(json_data.StudyFormName);
                  $(currBlockData_StudyBasisId).text(json_data.StudyBasisName);
                  $(currBlockData_Profession).text(json_data.Profession);
                  $(currBlockData_ObrazProgram).text(json_data.ObrazProgram);
                  $(currBlockData_Specialization).text(json_data.Specialization);
                  $(currBlockData_Faculty).text(json_data.Faculty); 
                  $(currBlock).hide();
                  $(currBlockData).show();
                  if (BlockIds[nxt] == undefined) {
                      $(nextBlock).show(); 
                      GetStudyLevels(nxt);
                  }
                  BlockIds[i] = json_data.Id; 
              }
              else {
                  $(currObrazProgramErrors).text(json_data.ErrorMessage).show();
              }
          }, 'json');
        $('#Submit').removeAttr("disabled");
    }
    
    function DeleteMsg(i)
    {
        var I = i;
        if (1!=2){
            $("#dialog-form").dialog(
                    {
                        autoOpen: false,
                        height: 400,
                        width: 350,
                        modal: true, 
                        buttons:
                        {
                            "Да (yes)": function () { $(this).dialog("close"); DeleteApp(I); },
                            "Нет (no)": function () { $(this).dialog("close"); }
                        } 
                    });
            $("#dialog-form").dialog("open");
        }
        else
        {
            DeleteApp(I); 
        }
    }
    function DeleteApp(i) {
        var appId = BlockIds[i];
        nextBlock = '#Block' + i;
        nextFinishButton = '#FinishBtn' + i;
        nextSpecs = '#Specs' + i;    
        nextObrazProgramErrors = '#ObrazProgramsErrors' + i; 
        nextProfessions = '#Professions' + i;
        nextNeedHostel = '#NeedHostel' + i;
        nextBlockData = '#BlockData' + i;
        
        nextBlockData_StudyFormId = '#BlockData_StudyFormId' + i;
        nextBlockData_StudyBasisId = '#BlockData_StudyBasisId' + i;
        nextBlockData_Profession = '#BlockData_Profession' + i;
        nextBlockData_ObrazProgram = '#BlockData_ObrazProgram' + i;
        nextBlockData_Specialization = '#BlockData_Specialization' + i; 

        currObrazProgramsErrors_Block = '#ObrazProgramsErrors_Block' + i;
        $(currObrazProgramsErrors_Block).text('').hide();

        $.post('/Abiturient/DeleteApplication_Mag', { id : appId, CommitId : $('#CommitId').val() }, function(json_data) {
            if (json_data.IsOk) {  
                $(nextBlockData_StudyFormId).text('');
                $(nextBlockData_StudyBasisId).text('');
                $(nextBlockData_Profession).text('');
                $(nextBlockData_ObrazProgram).text('');
                $(nextBlockData_Specialization).text('');
                $(nextBlockData).hide();
                $(nextBlock).show();
                GetStudyLevels(i);
            }
            else {
                $(currObrazProgramsErrors_Block).text(json_data.ErrorMessage).show();
            }
        }, 'json');
        $('#Submit').removeAttr("disabled");
    }
</script>
<script type="text/javascript" src="../../Scripts/jquery-ui-1.8.11.js"></script>
<% using (Html.BeginForm("NewApp_AG", "Abiturient", FormMethod.Post))
   { 
       if (Model.HasError)
       { %>    
    <div class="error message"><%= Model.ErrorMessage%></div>
    <% } %>
    <%= Html.HiddenFor(x => x.CommitId)%>
    <% if (!String.IsNullOrEmpty(Model.OldCommitId)){ %><%= Html.HiddenFor(x => x.OldCommitId)%><%} %> 
    <p class = "error message">    
        <%= GetGlobalResourceObject("NewApplication", "AbitMessage")%>    
    </p>
    <input type="hidden" id = "EntryType" name = "EntryType" value="3" />
    <% for (int i = 1; i <= Model.Applications.Count; i++)
       { %>
    <div id="BlockData<%= i.ToString()%>" class="message info panel" style="width:659px;">
        <table class="nopadding" cellspacing="0" cellpadding="0">
            <tr>
                <td style="width:12em;"><%= GetGlobalResourceObject("PriorityChangerForeign", "Priority").ToString()%></td>
                <td style="font-size:1.3em;"><%= i.ToString()%></td>
            </tr>
            <tr>
                <td style="width:12em;"><%= GetGlobalResourceObject("NewApplication", "BlockData_StudyForm")%></td>
                <td id="BlockData_StudyFormId<%= i.ToString()%>" style="font-size:1.3em;"><%= Model.Applications[i - 1].StudyFormName%></td>
            </tr>
            <tr>
                <td style="width:12em;"><%= GetGlobalResourceObject("NewApplication", "BlockData_StudyBasis")%></td>
                <td id="BlockData_StudyBasisId<%= i.ToString()%>" style="font-size:1.3em;"><%= Model.Applications[i - 1].StudyBasisName%></td>
            </tr>
            <tr>
                <td style="width:12em;"><%= GetGlobalResourceObject("NewApplication", "BlockData_Faculty")%></td>
                <td id="BlockData_Faculty<%= i.ToString()%>" style="font-size:1.3em;"><%= Model.Applications[i - 1].FacultyName%></td>
            </tr>
            <tr>
                <td style="width:12em;"><%= GetGlobalResourceObject("NewApplication", "BlockData_LicenseProgram")%></td>
                <td id="BlockData_Profession<%= i.ToString()%>" style="font-size:1.3em;"><%= Model.Applications[i - 1].ProfessionName%></td>
            </tr>
            <tr>
                <td style="width:12em;"><%= GetGlobalResourceObject("NewApplication", "BlockData_ObrazProgram")%></td>
                <td id="BlockData_ObrazProgram<%= i.ToString()%>" style="font-size:1.3em;"><%= Model.Applications[i - 1].ObrazProgramName%></td>
            </tr>
            <tr>
                <td style="width:12em;"><%= GetGlobalResourceObject("NewApplication", "BlockData_Specialization")%></td>
                <td id="BlockData_Specialization<%= i.ToString()%>" style="font-size:1.3em;" ><%= Model.Applications[i - 1].SpecializationName%></td>
            </tr>
        </table> 
        <button type="button" 
                <% if (Model.Applications[i-1].DateOfClose < DateTime.Now) { %>onclick="DeleteMsg(<%= i.ToString()%>)" <% } else { %> onclick="DeleteApp(<%= i.ToString()%>)" <% } %>
                class="error">
            <%= GetGlobalResourceObject("NewApplication", "Delete")%>
        </button>
        <div id="ObrazProgramsErrors_Block<%= i.ToString()%>" class="message error" style="display:none; width:450px;">
        </div>
    </div>
    <div id="Block<%= i.ToString()%>" class="message info panel" style="width:659px; display:none;">
        <p id="SForm<%= i.ToString()%>">
            <span><%= GetGlobalResourceObject("NewApplication", "BlockData_StudyForm")%></span><br /> 
            <%= Html.DropDownList("StudyFormId" + i.ToString(), Model.StudyFormList, new SortedList<string, object>() { { "size", "1" },
                 { "style", "min-width:450px;" }, { "onchange", "GetProfessions(" + i.ToString() + ")" }})%>
        </p>
        <p id="SBasis<%= i.ToString()%>">
            <span><%= GetGlobalResourceObject("NewApplication", "BlockData_StudyBasis")%></span><br />
            <%= Html.DropDownList("StudyBasisId" + i.ToString(), Model.StudyBasisList, new SortedList<string, object>() { { "size", "1" }, 
                { "style", "min-width:450px;" },   { "onchange", "GetProfessions(" + i.ToString() + ")" } })%>
        </p>
        <p id="StudyLevels<%= i.ToString()%>" style="border-collapse:collapse;">
            <span><%= GetGlobalResourceObject("NewApplication", "HeaderStudyLevel")%></span><br />
            <select id="StudyLevel<%= i.ToString()%>" size="2" style="width:659px;" onchange="GetProfessions(<%= i.ToString()%>)"></select>
        </p>
        <p id="Profs<%= i.ToString()%>" style="border-collapse:collapse;width:659px;">
            <span><%= GetGlobalResourceObject("NewApplication", "HeaderProfession")%></span><br />
            <select id="lProfession<%= i.ToString()%>" size="12" name="lProfession" style="width:659px;" onchange="GetObrazPrograms(<%= i.ToString()%>)"></select>
        </p>
        <p id="ObrazPrograms<%= i.ToString()%>" style="border-collapse:collapse;width:659px;">
            <span><%= GetGlobalResourceObject("NewApplication", "HeaderObrazProgram")%></span><br />
            <select id="lObrazProgram<%= i.ToString()%>" size="5" name="lObrazProgram" style="width:659px;" onchange="GetSpecializations(<%= i.ToString()%>)"></select>
        </p>
        <p id="Specs<%= i.ToString()%>" style="border-collapse:collapse;width:659px;">
            <span><%= GetGlobalResourceObject("NewApplication", "HeaderProfile")%></span><br />
            <select id="lSpecialization<%= i.ToString()%>" size="5" name="lSpecialization" style="width:659px;" onchange="MkBtn(<%= i.ToString()%>)"></select>
            <br /><br /><span id="SpecsErrors<%= i.ToString()%>" class="Red"></span>
        </p>
        <p id="Facs<%= i.ToString()%>" style="display:none; border-collapse:collapse;">
            <span><%= GetGlobalResourceObject("NewApplication", "HeaderFaculty")%></span><br />
            <select id="lFaculty<%= i.ToString()%>" size="2" name="lFaculty" onchange="GetProfessions(<%= i.ToString()%>)"></select>
        </p>
        <br />
        <div id="FinishBtn<%= i.ToString()%>" style="border-collapse:collapse;">
            <input id="Submit<%= i.ToString()%>" type="button" value=<%=GetGlobalResourceObject("NewApplication", "btnAdd").ToString()%> onclick="SaveData(<%= i.ToString()%>)" class="button button-blue"/>
        </div><br />
        <span id="ObrazProgramsErrors<%= i.ToString()%>" class="message error" style="display:none;"></span> 
    </div>
    <% } %>
    <span id="NewApp_NoFreeEntries" class="message error" style="display:none;"><%= GetGlobalResourceObject("NewApplication", "NewApp_NoFreeEntries")%></span>
    <span id="ErrorHasApplication" class="message error" style="display:none;"><%= GetGlobalResourceObject("NewApplication", "ErrorHasApplication")%></span>
    <% for (int i = Model.Applications.Count + 1; i <= Model.MaxBlocks; i++)
       { %> 
    <div id="BlockData<%= i.ToString()%>" class="message info panel" style="width:659px; display:none;">
        <table class="nopadding" cellspacing="0" cellpadding="0">
            <tr>
                <td style="width:12em;"><%= GetGlobalResourceObject("PriorityChangerForeign", "Priority").ToString()%></td>
                <td style="font-size:1.3em;"><%= i.ToString()%></td>
            </tr>
            <tr>
                <td style="width:12em;"><%= GetGlobalResourceObject("NewApplication", "BlockData_StudyForm")%></td>
                <td id="BlockData_StudyFormId<%= i.ToString() %>" style="font-size:1.3em;"></td>
            </tr>
            <tr>
                <td style="width:12em;"><%= GetGlobalResourceObject("NewApplication", "BlockData_StudyBasis")%></td>
                <td id="BlockData_StudyBasisId<%= i.ToString() %>" style="font-size:1.3em;"></td>
            </tr>
            <tr>
                <td style="width:12em;"><%= GetGlobalResourceObject("NewApplication", "BlockData_Faculty")%></td>
                <td id="BlockData_Faculty<%= i.ToString()%>" style="font-size:1.3em;"></td>
            </tr>
            <tr>
                <td style="width:12em;"><%= GetGlobalResourceObject("NewApplication", "BlockData_LicenseProgram")%></td>
                <td id="BlockData_Profession<%= i.ToString() %>" style="font-size:1.3em;"></td>
            </tr>
            <tr>
                <td style="width:12em;"><%= GetGlobalResourceObject("NewApplication", "BlockData_ObrazProgram")%></td>
                <td id="BlockData_ObrazProgram<%= i.ToString() %>" style="font-size:1.3em;"></td>
            </tr>
            <tr>
                <td style="width:12em;"><%= GetGlobalResourceObject("NewApplication", "BlockData_Specialization")%></td>
                <td id="BlockData_Specialization<%= i.ToString() %>" style="font-size:1.3em;"></td>
            </tr>
        </table>
        <button type="button" onclick="DeleteApp(<%= i.ToString()%>)" class="error"><%= GetGlobalResourceObject("NewApplication", "Delete")%></button>
        <div id="ObrazProgramsErrors_Block<%= i.ToString()%>" class="message error" style="display:none; width:450px;">
        </div>
    </div>
    <div id="Block<%= i.ToString()%>" class="message info panel" style="width:659px; display:none;">
        <p id="SForm<%= i.ToString()%>">
            <span><%= GetGlobalResourceObject("NewApplication", "BlockData_StudyForm")%></span><br /> 
            <%= Html.DropDownList("StudyFormId" + i.ToString(), Model.StudyFormList, new SortedList<string, object>() { { "size", "1" },
                 { "style", "min-width:450px;" }, { "onchange", "GetProfessions(" + i.ToString() + ")" } })%>
        </p>
        <p id="SBasis<%= i.ToString()%>">
            <span><%= GetGlobalResourceObject("NewApplication", "BlockData_StudyBasis")%></span><br />
            <%= Html.DropDownList("StudyBasisId" + i.ToString(), Model.StudyBasisList, new SortedList<string, object>() { { "size", "1" }, 
                { "style", "min-width:450px;" },   { "onchange", "GetProfessions(" + i.ToString() + ")" } })%>
        </p>
        <p id="StudyLevels<%= i.ToString()%>" style="border-collapse:collapse;">
            <span><%= GetGlobalResourceObject("NewApplication", "HeaderStudyLevel")%></span><br />
            <select id="StudyLevel<%= i.ToString()%>" size="2" style="width:659px;" onchange="GetProfessions(<%= i.ToString()%>)"></select>
        </p>
        <p id="Profs<%= i.ToString()%>" style="border-collapse:collapse;">
            <span><%= GetGlobalResourceObject("NewApplication", "HeaderProfession")%></span><br />
            <select id="lProfession<%= i.ToString()%>" size="12" name="lProfession" style="width:659px;" onchange="GetObrazPrograms(<%= i.ToString()%>)"></select>
        </p>
        <p id="ObrazPrograms<%= i.ToString()%>" style="border-collapse:collapse;">
            <span><%= GetGlobalResourceObject("NewApplication", "HeaderObrazProgram")%></span><br />
            <select id="lObrazProgram<%= i.ToString()%>" size="5" name="lObrazProgram" style="width:659px;" onchange="GetSpecializations(<%= i.ToString()%>)"></select>
        </p>
        <p id="Specs<%= i.ToString()%>" style="border-collapse:collapse;">
            <span><%= GetGlobalResourceObject("NewApplication", "HeaderProfile")%></span><br />
            <select id="lSpecialization<%= i.ToString()%>" size="5" name="lSpecialization" style="width:659px;" onchange="MkBtn(<%= i.ToString()%>)"></select>
            <br /><br /><span id="SpecsErrors<%= i.ToString()%>" class="Red"></span>
        </p>
        <p id="Facs<%= i.ToString()%>" style="display:none; border-collapse:collapse;">
            <span>Факультет</span><br />
            <select id="lFaculty<%= i.ToString()%>" size="2" name="lFaculty" onchange="GetProfessions(<%= i.ToString()%>)"></select>
        </p> <br />
        <div id="FinishBtn<%= i.ToString()%>" style="border-collapse:collapse;">
            <input id="Submit<%= i.ToString()%>" type="button" value=<%=GetGlobalResourceObject("NewApplication", "btnAdd").ToString()%> onclick="SaveData(<%= i.ToString()%>)" class="button button-blue"/>
        </div><br />
        <span id="ObrazProgramsErrors<%= i.ToString()%>" class="message error" style="display:none;"></span>
        
    </div>
    <%} %>
    <br />
    <input id="Submit" type="submit" <% if (!Model.ProjectJuly){ %>disabled <%} %> value=<%=GetGlobalResourceObject("NewApplication", "btnSubmit").ToString()%>  class="button button-green"/>
<% 
   } //using (Html.BeginForm("NewApp_SPO", "Abiturient", FormMethod.Post))
   } //if (Model.Enabled)
%>
    <div id="dialog-form" style="display:none;">
        <p class="errMessage"></p>
        <p>Так как прием на данное направление закрыт, то конкурс нельзя будет добавить снова. Вы хотите удалить заявление?</p>
    </div>
</asp:Content>
