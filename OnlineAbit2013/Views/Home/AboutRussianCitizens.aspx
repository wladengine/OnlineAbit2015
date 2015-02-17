﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Граждане, имеющие равные права с гражданами РФ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<h5 style="text-align:center;">Категории иностранных граждан и соотечественников, проживающих за рубежом, которые имеют право на прием для обучения по основным образовательным программам высшего профессионального образования за счет средств федерального бюджета Российской Федерации</h5>
<hr />
<table class="paginate">
    <thead>
        <tr>
            <th style="width:30%;border:1px solid #000000; background-color:#D1D1D1;">Гражданство абитуриента</th>
            <th style="width:30%;border:1px solid #000000; background-color:#D1D1D1;">Нормативно-правовые акты, предоставляющие возможность обучения по основным образовательным программам высшего профессионального образования за счет средств федерального бюджета Российской Федерации</th>
            <th style="width:40%;border:1px solid #000000; background-color:#D1D1D1;">Необходимые условия</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td style="border:1px solid #000000;">Граждане стран ближнего и дальнего зарубежья</td>
            <td style="border:1px solid #000000;">Постановление Правительства Российской Федерации от 25.08.2008 № 638 «О сотрудничестве с зарубежными странами в области образования»</td>
            <td style="border:1px solid #000000;">Прием осуществляется в пределах квоты мест, установленной Постановлением  Правительства Российской Федерации от 25.08.2008 № 638 «О сотрудничестве с зарубежными странами в области образования», выделенных СПбГУ в соответствующем году для приема иностранных граждан и соотечественников, проживающих за рубежом. Отбор кандидатов на обучение в соответствующем году проводится СПбГУ самостоятельно и (или) Министерством образования и науки Российской Федерации</td>
        </tr>
        <tr>
            <td style="border:1px solid #000000;">Республика Беларусь<br />Республика Казахстан<br />Кыргызская Республика<br />Республика Таджикистан<br /></td>
            <td style="border:1px solid #000000;">Соглашение о предоставлении равных прав гражданам государств – участников Договора об углублении интеграции в экономической и гуманитарной областях от 29.03.1996, утвержденное Постановлением Правительства Российской Федерации от 22.06.1999 № 662</td>
            <td style="border:1px solid #000000;">Правом поступления на обучение за счет средств федерального бюджета Российской Федерации обладают граждане Республики Беларусь, Республики Казахстан, Кыргызской Республики, Республики Таджикистан</td>
        </tr>
        <tr>
            <td style="border:1px solid #000000;">Республика Армения<br />Республика Беларусь<br />Республика Казахстан<br />Кыргызская Республика<br />Республика Молдова<br />Республика Таджикистан<br />Туркменистан<br />Республика Узбекистан<br />Украина</td>
            <td style="border:1px solid #000000;">Соглашение о сотрудничестве в области образования, заключенное в г. Ташкент 15.05.1992</td>
            <td style="border:1px solid #000000;">Правом поступления на обучение за счет средств федерального бюджета Российской Федерации обладают граждане государств – участников Соглашения о сотрудничестве в области образования, заключенное в г. Ташкент 15.05.1992, постоянно проживающие на территории Российской Федерации и имеющие документы: подтверждающие правомерность их пребывания на территории Российской Федерации</td>
        </tr>
        <tr>
            <td style="border:1px solid #000000;">Грузия</td>
            <td style="border:1px solid #000000;">Соглашение между Правительством Российской Федерации и Правительством Республики Грузия о сотрудничестве в области культуры, науки и образования, одобренное Постановлением Правительства Российской Федерации от 02.02.1994 № 49 </td>
            <td style="border:1px solid #000000;">Правом поступления на обучение за счет средств федерального бюджета Российской Федерации обладают граждане Грузии, постоянно проживающие на территории Российской Федерации и имеющие документы: подтверждающие правомерность их пребывания на территории Российской Федерации</td>
        </tr>
        <tr>
            <td style="border:1px solid #000000;">Соотечественники, проживающие за рубежом</td>
            <td style="border:1px solid #000000;">Государственная программа по оказанию содействия добровольному переселению в Российскую Федерацию соотечественников, проживающих за рубежом, утвержденная Указом Президента Российской Федерации от 22.06.2006 № 637 </td>
            <td style="border:1px solid #000000;">Наличие свидетельства участника Государственной программы по оказанию содействия добровольному переселению в Российскую Федерацию соотечественников, проживающих за рубежом, утвержденной Указом Президента Российской Федерации от 22.06.2006 № 637</td>
        </tr>
        <tr>
            <td style="border:1px solid #000000;">Соотечественники, проживающие за рубежом</td>
            <td style="border:1px solid #000000;">Федеральный закон Российской Федерации от 24.05.1999 № 99-ФЗ «О государственной политике Российской Федерации в отношении соотечественников за рубежом»</td>
            <td style="border:1px solid #000000;">
                Самоидентификация, подкрепленная общественной либо профессиональной деятельностью по сохранению русского языка, родных языков народов Российской Федерации, развитию российской культуры за рубежом, укреплению дружественных отношений государств проживания соотечественников с Российской Федерацией, поддержке общественных объединений соотечественников и защите прав соотечественников либо иными свидетельствами свободного выбора в пользу духовной и культурной связи с Российской Федерацией, лица с соотечественниками.
            <br />В соответствии со статьей 1 Федерального закона Российской Федерации от 24.05.1999 № 99-ФЗ «О государственной политике Российской Федерации в отношении соотечественников за рубежом» соотечественникам за рубежом признаются:
            <ul>
                <li>граждане Российской Федерации, постоянно проживающие за пределами территории Российской Федерации;</li>
                <li>лица и их потомки, проживающие за пределами территории Российской Федерации и относящиеся, как правило, к народам, исторически проживающим на территории Российской Федерации, а также сделавшие свободный выбор в пользу духовной, культурной и правовой связи с Российской Федерацией лица, чьи родственники по прямой восходящей линии ранее проживали на территории Российской Федерации, в том числе:<br />
                    <ul>
                        <li>лица, состоявшие в гражданстве СССР, проживающие в государствах, входивших в состав СССР, получившие гражданство этих государств или ставшие лицами без гражданства;</li>
                        <li>выходцы (эмигранты) из Российского государства, Российской республики, РСФСР, СССР и Российской Федерации, имевшие соответствующую гражданскую принадлежность и ставшие гражданами иностранного государства или лицами без гражданства</li>
                    </ul>
                </li>
            </ul>
            </td>
        </tr>
    </tbody>
</table>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="NavigationList" runat="server">
    <ul class="clearfix">
        <li><a href="../../AbiturientNew/Main"><%= GetGlobalResourceObject("Common", "MainNavLogon").ToString()%></a></li>
        <li><a href="../../Account/Register"><%= GetGlobalResourceObject("Common", "MainNavRegister").ToString()%></a></li>
        <li class="active"><a>Информация</a></li>
    </ul>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="Subheader" runat="server">
    <h2>Граждане, имеющие равные права с гражданами РФ</h2>
</asp:Content>
