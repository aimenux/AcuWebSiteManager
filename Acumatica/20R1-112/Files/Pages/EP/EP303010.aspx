<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP303010.aspx.cs"
    Inherits="Page_EP303010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.EP.EPApprovalMaint" PrimaryView="Approval">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" ClosePopup="true" PopupVisible="True" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />

        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Approval" Caption="Approval Summary"
        NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" LinkIndicator="True"
        NotifyIndicator="True" TabIndex="100">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />            
            <px:PXTextEdit ID="DocType" runat="server" DataField="DocType"/>                        
            <px:PXTextEdit ID="RefNoteID" runat="server" DataField="RefNoteID"/>            
            <px:PXDateTimeEdit ID="DocDate" runat="server" DataField="DocDate"/>            
            <px:PXSelector ID="BAccountID" runat="server" DataField="BAccountID" DataSourceID="ds"/>            
            <px:PXTextEdit ID="Descr" runat="server" DataField="Descr"/>            
            <px:PXNumberEdit ID="CuryTotalAmount" runat="server" DataField="CuryTotalAmount"/>                        
            <px:PXDateTimeEdit ID="CreatedDateTime" runat="server" DataField="CreatedDateTime"/>            
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="true"/>
            <px:PXSelector ID="WorkgroupID" runat="server" DataField="WorkgroupID" DataSourceID="ds" />
            <px:PXSelector ID="OwnerID" runat="server" DataField="OwnerID" DataSourceID="ds" />
            <px:PXSelector ID="ApprovedByID" runat="server" DataField="ApprovedByID" DataSourceID="ds" />
            <px:PXDateTimeEdit ID="ApproveDate" runat="server" DataField="ApproveDate" />
            <px:PXDropDown ID="Status" runat="server" DataField="Status" />            
        </Template>
    </px:PXFormView>
</asp:Content>
