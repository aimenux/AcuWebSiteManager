<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CM502000.aspx.cs" Inherits="Page_CM502000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="TranslationReleaseList" TypeName="PX.Objects.CM.TranslationRelease" >         <CallbackCommands>            <px:PXDSCallbackCommand Visible="False" Name="TranslationReleaseList_referenceNbr_ViewDetails" />        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" ActionsPosition="Top" AllowSearch="true" DataSourceID="ds" Caption="Translation Worksheets"
        SkinID="PrimaryInquire" FastFilterFields="ReferenceNbr, Description, TranslDefId" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="TranslationReleaseList">
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" />
                    <px:PXGridColumn DataField="ReferenceNbr" />
                    <px:PXGridColumn DataField="Description" />
                    <px:PXGridColumn DataField="TranslDefId" />
                    <px:PXGridColumn DataField="BranchID" />
                    <px:PXGridColumn DataField="LedgerID" />
                    <px:PXGridColumn DataField="DateEntered" />
                    <px:PXGridColumn DataField="FinPeriodID" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
