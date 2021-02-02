<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA301000.aspx.cs"
    Inherits="Page_FA301000" Title="FARegister Entry" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.FA.TransactionEntry" PrimaryView="Document">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Release" StartNewGroup="True" PopupCheckSave="false" />
            <px:PXDSCallbackCommand Name="ViewBatch" Visible="False" DependOnGrid="gridTrans" />
            <px:PXDSCallbackCommand DependOnGrid="gridTrans" Name="ViewAsset" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="gridTrans" Name="ViewBook" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Document" Caption="Transaction Summary"
        NoteIndicator="True" FilesIndicator="True" LinkIndicator="True" NotifyIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity"
        EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" />
            <px:PXDateTimeEdit ID="edDocDate" runat="server" DataField="DocDate" CommitChanges="True"/>
            <px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="Status" Enabled="False" />
            <px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
            <px:PXDropDown ID="edOrigin" runat="server" AllowNull="False" DataField="Origin" Enabled="False" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edReason" runat="server" DataField="Reason" Enabled="False" />
            <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" Size="XL" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S"/>
            <px:PXNumberEdit ID="edTranAmt" DataField="TranAmt" Enabled="False" runat="server"/> 
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid SkinID="Details" ID="gridTrans" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; top: 0px; height: 360px;"
        Width="100%" AllowPaging="True" AdjustPageSize="Auto" ActionsPosition="Top" AllowSearch="True" Caption="Transaction Details"
        SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="Trans">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXSelector CommitChanges="True" ID="edAssetID" runat="server" DataField="AssetID">
                        <GridProperties FastFilterFields="AssetCD,Description">
                            <PagerSettings Mode="NextPrevFirstLast" />
                        </GridProperties>
                    </px:PXSelector>
                    <px:PXSelector CommitChanges="True" ID="edBookID" runat="server" DataField="BookID" AutoRefresh="True">
                        <Parameters>
                            <px:PXSyncGridParam ControlID="gridTrans" />
                        </Parameters>
                    </px:PXSelector>
                    <px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
                    <px:PXDropDown CommitChanges="True" ID="edTranType" runat="server" DataField="TranType" />
                    <px:PXNumberEdit ID="edTranAmt" runat="server" DataField="TranAmt" />
                    <px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" />
                    <px:PXLayoutRule runat="server" ColumnSpan="2" />
                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXSegmentMask ID="edDebitAccountID" runat="server" DataField="DebitAccountID" />
                    <px:PXSegmentMask ID="edDebitSubID" runat="server" DataField="DebitSubID" AutoRefresh="True" />
                    <px:PXSegmentMask ID="edCreditAccountID" runat="server" DataField="CreditAccountID" />
                    <px:PXSegmentMask ID="edCreditSubID" runat="server" DataField="CreditSubID" AutoRefresh="True" />
                    <px:PXTextEdit ID="edMethodDesc" runat="server" DataField="MethodDesc" Enabled="False" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="AssetID" AutoCallBack="True" LinkCommand="ViewAsset" />
                    <px:PXGridColumn DataField="AssetID_FixedAsset_description" Label="Description" AllowUpdate="False" />
                    <px:PXGridColumn DataField="BookID" AutoCallBack="True" LinkCommand="ViewBook" />
                    <px:PXGridColumn DataField="TranType" RenderEditorText="True" AutoCallBack="True" MatrixMode="True"/>
                    <px:PXGridColumn DataField="DebitAccountID" DisplayFormat="&gt;######" AutoCallBack="True" />
                    <px:PXGridColumn DataField="DebitAccountID_Account_description" />
                    <px:PXGridColumn DataField="DebitSubID" DisplayFormat="&gt;AA-AA-AA-AA-AAA" />
                    <px:PXGridColumn DataField="DebitSubID_Sub_description" />
                    <px:PXGridColumn DataField="CreditAccountID" DisplayFormat="&gt;######" AutoCallBack="True" />
                    <px:PXGridColumn DataField="CreditAccountID_Account_description" />
                    <px:PXGridColumn DataField="CreditSubID" DisplayFormat="&gt;AA-AA-AA-AA-AAA" />
                    <px:PXGridColumn DataField="CreditSubID_Sub_description" />
                    <px:PXGridColumn DataField="TranAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="BatchNbr" Label="Batch Nbr." LinkCommand="ViewBatch" />
                    <px:PXGridColumn DataField="TranDesc" />
                    <px:PXGridColumn DataField="FinPeriodID" Label="Tran. Period" />
                    <px:PXGridColumn AllowUpdate="False" DataField="MethodDesc" Label="Method" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
        <Mode AllowFormEdit="True" InitNewRow="True" />
    </px:PXGrid>
</asp:Content>
