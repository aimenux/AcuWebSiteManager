<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS201000.aspx.cs" Inherits="Page_FS201000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        BorderStyle="NotSet" PrimaryView="LicenseRecords" SuspendUnloading="False" 
        TypeName="PX.Objects.FS.LicenseMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" 
        Style="z-index: 100" Width="100%" DataMember="LicenseRecords" TabIndex="500" DefaultControlID="RefNbr">
		<Template>
            <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartRow="True">
            </px:PXLayoutRule>
            <px:PXSelector ID="RefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" DataSourceID="ds">
            </px:PXSelector>
            <px:PXSelector ID="LicenseTypeID" runat="server" DataField="LicenseTypeID" DataSourceID="ds" AllowEdit = "True" CommitChanges="True">
            </px:PXSelector>
            <px:PXTextEdit ID="Descr" runat="server" DataField="Descr">
            </px:PXTextEdit>
            <px:PXSegmentMask ID="edEmployeeID" runat="server" DataField="EmployeeID"  AllowEdit = "True" CommitChanges="True" DataSourceID="ds">
            </px:PXSegmentMask>
            <px:PXDateTimeEdit ID="IssueDate" runat="server" DataField="IssueDate" CommitChanges="True">
            </px:PXDateTimeEdit>
            <px:PXLayoutRule runat="server" Merge="True">
            </px:PXLayoutRule>
            <px:PXDateTimeEdit ID="ExpirationDate" runat="server" DataField="ExpirationDate" CommitChanges="True">
            </px:PXDateTimeEdit>
            <px:PXCheckBox ID="edNeverExpires" runat="server" DataField="NeverExpires" CommitChanges="True">
            </px:PXCheckBox>
        </Template>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXFormView>
</asp:Content>
