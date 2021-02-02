<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS204000.aspx.cs" Inherits="Page_CS204000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CS.CountryMaint" PrimaryView="Country">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" StartNewGroup="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Country" Caption="Country Summary">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="M" />
			<px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AutoRefresh ="True"/>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXSelector ID="edAddressValidatorPluginID" runat="server" DataField="AddressValidatorPluginID" />
			<px:PXCheckBox ID="edAutoOverrideAddress" runat="server" DataField="AutoOverrideAddress" />
			
			<px:PXLayoutRule runat="server" StartRow="True" ControlSize="M" LabelsWidth="M" />
            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Country Validation" ControlSize="M" LabelsWidth="M" />
			<px:PXDropDown ID="edCountryValidationMethod" runat="server" DataField="CountryValidationMethod" AutoRefresh ="True"/>
			<px:PXTextEdit ID="edCountryRegexp" runat="server" DataField="CountryRegexp" />
			
			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="State Validation" ControlSize="M" LabelsWidth="M" />
			<px:PXDropDown ID="edStateValidationMethod" runat="server" DataField="StateValidationMethod" AutoRefresh ="True"/>

            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="M" />
            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Postal Code" ControlSize="M" LabelsWidth="M" />
			<px:PXTextEdit ID="edZipCodeMask" runat="server" DataField="ZipCodeMask" />
			<px:PXTextEdit ID="edZipCodeRegexp" runat="server" DataField="ZipCodeRegexp" />
			
            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Language/Locale" ControlSize="M" LabelsWidth="M" />
			<px:PXSelector ID="edLanguageID" runat="server" AllowEdit="True" DataField="LanguageID"
		                   TextMode="Search" DataSourceID="ds" DisplayMode="Hint" />
			
			
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" SkinID="Details" Caption="States" FastFilterFields="StateID,Name" TabIndex="26100">
		<Levels>
			<px:PXGridLevel DataMember="CountryStates">
				<Columns>
					<px:PXGridColumn DataField="StateID" Width="200px" AllowShowHide="False" />
					<px:PXGridColumn DataField="Name" Width="350px" AllowShowHide="False" />
					<px:PXGridColumn DataField="StateRegexp" Width="350px" AllowShowHide="False" />
					<px:PXGridColumn DataField="NonTaxable" Width="100px" Type="CheckBox" />
                    <px:PXGridColumn DataField="LocationCode" Width="75px" AllowShowHide="True" />
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXTextEdit ID="edStateID" runat="server" DataField="StateID" />
					<px:PXTextEdit ID="edName" runat="server" DataField="Name" />
					<px:PXCheckBox ID="chkNonTaxable" runat="server" DataField="NonTaxable" />
                    <px:PXTextEdit ID="edLocationCode" runat="server" DataField="LocationCode" />
				</RowTemplate>
				<Layout FormViewHeight="" />
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<CallbackCommands>
			<Save PostData="Page" />
		</CallbackCommands>
		<Mode AllowColMoving="False" />
	</px:PXGrid>
</asp:Content>
