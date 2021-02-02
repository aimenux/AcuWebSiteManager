<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM206506.aspx.cs" Inherits="Page_SM206506" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="ScanJobInfoRecord" TypeName="PX.SM.ScanJobInfoMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand Visible="true" Name="attachFromScanner" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="ScanJobInfoRecord" TabIndex="900">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" ControlSize="XM" LabelsWidth="S" />
		    <px:PXSelector ID="edScannerID" runat="server" CommitChanges="True" DataField="ScannerID">
            </px:PXSelector>
            <px:PXDropDown ID="edPaperSource" runat="server" CommitChanges="True" DataField="PaperSource">
            </px:PXDropDown>
            <px:PXDropDown ID="edPixelType" runat="server" CommitChanges="True" DataField="PixelType">
            </px:PXDropDown>
            <px:PXDropDown ID="edResolution" runat="server" CommitChanges="True" DataField="Resolution">
            </px:PXDropDown>
            <px:PXDropDown ID="edFileType" runat="server" CommitChanges="True" DataField="FileType">
            </px:PXDropDown>
            <px:PXTextEdit ID="edFileName" runat="server" CommitChanges="True" DataField="FileName">
            </px:PXTextEdit>
		</Template>
		<AutoSize Container="Window" Enabled="True"/>
	</px:PXFormView>
</asp:Content>
