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
            <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="M" StartRow="True">
            </px:PXLayoutRule>
            <px:PXSelector ID="RefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" DataSourceID="ds">
            </px:PXSelector>
            <px:PXSelector ID="LicenseTypeID" runat="server" DataField="LicenseTypeID" DataSourceID="ds" 
            Size="M" AllowEdit = "True" CommitChanges="True">
            </px:PXSelector>
            <px:PXTextEdit ID="Descr" runat="server" DataField="Descr" Size="M">
            </px:PXTextEdit>
            <px:PXLayoutRule runat="server" GroupCaption="License Settings" 
                StartGroup="True" ControlSize="M" LabelsWidth="M">
            </px:PXLayoutRule>
            <px:PXSegmentMask ID="edEmployeeID" runat="server" DataField="EmployeeID"
                Size="SM" DataSourceID="ds">
            </px:PXSegmentMask>
            <px:PXDateTimeEdit ID="IssueDate" runat="server" DataField="IssueDate" 
                Size="SM" CommitChanges="True">
            </px:PXDateTimeEdit>
            <px:PXDateTimeEdit ID="ExpirationDate" runat="server" 
                DataField="ExpirationDate" Size="SM" CommitChanges="True">
            </px:PXDateTimeEdit>
            <px:PXSegmentMask ID="IssueByVendorID" runat="server" CommitChanges="True" DataField="IssueByVendorID" 
            DataSourceID="ds" DisplayMode="Value" Size="SM" AllowEdit = "True">
            </px:PXSegmentMask>
            <px:PXTextEdit ID="IssuingAgencyName" runat="server" DataField="IssuingAgencyName">
            </px:PXTextEdit>
            <px:PXCheckBox ID="CertificateRequired" runat="server" 
                DataField="CertificateRequired" Text="Certificate Required">
            </px:PXCheckBox>
            <px:PXNumberEdit ID="InitialAmount" runat="server" DataField="InitialAmount" 
                Size="S">
            </px:PXNumberEdit>
            <px:PXLayoutRule runat="server" Merge="True">
            </px:PXLayoutRule>
            <px:PXNumberEdit ID="InitialTerm" runat="server" DataField="InitialTerm" 
                Size="S">
            </px:PXNumberEdit>
            <px:PXDropDown ID="InitialTermType" runat="server" DataField="InitialTermType" Size="XS" SuppressLabel="True">
            </px:PXDropDown>
            <px:PXLayoutRule runat="server">
            </px:PXLayoutRule>
            <px:PXNumberEdit ID="RenewalAmount" runat="server" DataField="RenewalAmount" 
                Size="S">
            </px:PXNumberEdit>
            <px:PXLayoutRule runat="server" Merge="True">
            </px:PXLayoutRule>
            <px:PXNumberEdit ID="RenewalTerm" runat="server" DataField="RenewalTerm">
            </px:PXNumberEdit>
            <px:PXDropDown ID="RenewalTermType" runat="server" DataField="RenewalTermType" 
                Size="XS" SuppressLabel="True">
            </px:PXDropDown>
            <px:PXLayoutRule runat="server">
            </px:PXLayoutRule>
        </Template>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXFormView>
</asp:Content>
