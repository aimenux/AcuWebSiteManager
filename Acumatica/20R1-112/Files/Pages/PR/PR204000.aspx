<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="PR204000.aspx.cs" Inherits="Page_PR204000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PR.PTOBankMaint" PrimaryView="Bank">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Bank">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="SM" ControlSize="SM" />
			<px:PXSelector runat="server" ID="edBankID" DataField="BankID" CommitChanges="true" />
			<px:PXNumberEdit runat="server" ID="edAccrualRate" DataField="AccrualRate" />
			<px:PXNumberEdit runat="server" ID="edAccrualLimit" DataField="AccrualLimit" />
			<px:PXSelector runat="server" ID="edEarningTypeCD" DataField="EarningTypeCD" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit runat="server" ID="edDescription" DataField="Description" />

			<px:PXLayoutRule runat="server" StartColumn="true" LabelsWidth="S" ControlSize="SM" />
			<px:PXCheckBox runat="server" ID="edIsActive" DataField="IsActive" />
			<px:PXCheckBox runat="server" ID="edIsCertifiedJobAccrual" DataField="IsCertifiedJobAccrual" />
			<px:PXDateTimeEdit runat="server" ID="edStartDate" DataField="StartDate" TimeMode="false" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds" DataMember="CurrentBank">
		<Items>
			<px:PXTabItem Text="General Settings">
				<Template>
					<px:PXLayoutRule runat="server" StartRow="True" GroupCaption="Carryover Rules" LabelsWidth="SM" ControlSize="SM" />
					<px:PXDropDown runat="server" ID="edCarryoverType" DataField="CarryoverType" CommitChanges="true" />
					<px:PXNumberEdit runat="server" ID="edCarryoverAmount" DataField="CarryoverAmount" />

					<px:PXLayoutRule runat="server" StartColumn="true" GroupCaption="Front Loading Rules" LabelsWidth="SM" ControlSize="SM" />
					<px:PXNumberEdit runat="server" ID="edFrontLoadingAmount" DataField="FrontLoadingAmount" />
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
