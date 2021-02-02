<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM200550.aspx.cs" Inherits="Page_SM200550"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Locales" TypeName="PX.SM.LocaleMaintenance" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="editFormatCommand" DependOnGrid="grid" Visible="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXSmartPanel ID="PanelSetUpAlternatives" runat="server" Style="z-index: 108;" Width="300px" Height="300px" 
		Key="AlternativeHeader" AutoCallBack-Command="Refresh" AutoCallBack-Target="gridAlternativeDetails" CommandSourceID="ds"
		Caption="Languages" CaptionVisible="True" LoadOnDemand="True" ContentLayout-OuterSpacing="None">
		<px:PXFormView ID="frmAlternativeHeader" runat="server" DataMember="AlternativeHeader" Style="z-index: 100;" Width="100%" CaptionVisible="false" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="MS" ControlSize="MS" />
				<px:PXSelector CommitChanges="True" ID="edDefaultLanguageName" runat="server" DataField="DefaultLanguageName" AutoRefresh="True" DisplayMode="Hint" />
			</Template>
		</px:PXFormView>
        <px:PXGrid ID="gridAlternativeDetails" runat="server" Width="100%" Height="200px" SkinID="Attributes"
			DataSourceID="ds">
			<Levels>
				<px:PXGridLevel DataMember="AlternativeDetails">
					<Columns>
                        <px:PXGridColumn AllowCheckAll="True" AllowMove="False" AllowSort="False" DataField="IsAlternative" TextAlign="Center" Type="CheckBox" />
						<px:PXGridColumn DataField="LanguageName" Width="100px" />
						<px:PXGridColumn DataField="NativeName" Width="200px" />
					</Columns>
					<Layout FormViewHeight="" />
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="True" />
		</px:PXGrid>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Apply" />
			<px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="pnlEditFormat" runat="server" CaptionVisible="True" Caption="Locale Preferences"
		ForeColor="Black" Style="position: static" Height="305px" Width="850px" LoadOnDemand="True"
		Key="Formats" AutoCallBack-Target="formEditFormat" AutoCallBack-Command="Refresh"
		DesignView="Content">
		<px:PXFormView ID="formEditFormat" runat="server" DataSourceID="ds" Style="z-index: 100"
			Width="100%" DataMember="Formats" SkinID="Transparent" Caption="Custom Locale Format"
			TemplateContainer="">
			<AutoSize Container="Window" Enabled="True" MinHeight="200" />
			<Template>
			    
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"
					ColumnSpan="2" ColumnWidth="XXL" />
				<px:PXSelector CommitChanges="True" ID="edTemplateLocale" runat="server" DataField="TemplateLocale"
					DataSourceID="ds" />

				<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Date and Time Formats"
					ControlSize="M" LabelsWidth="SM" StartColumn="True" StartRow="True" />
				<px:PXSelector ID="edDateTimePattern" runat="server" DataField="DateTimePattern"
					AutoRefresh="True" DataSourceID="ds" />
				<px:PXSelector ID="edTimeShortPattern" runat="server" DataField="TimeShortPattern"
					AutoRefresh="True" DataSourceID="ds" />
				<px:PXSelector ID="edTimeLongPattern" runat="server" DataField="TimeLongPattern"
					AutoRefresh="True" DataSourceID="ds" />
				<px:PXSelector ID="edDateShortPattern" runat="server" DataField="DateShortPattern"
					AutoRefresh="True"  />
				<px:PXSelector ID="edDateLongPattern" runat="server" DataField="DateLongPattern"
					AutoRefresh="True" DataSourceID="ds" />
				<px:PXTextEdit ID="edAMDesignator" runat="server" DataField="AMDesignator" />
				<px:PXTextEdit ID="edPMDesignator" runat="server" DataField="PMDesignator" />

				<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True"
					GroupCaption="Number Format" StartGroup="True" />
				<px:PXDropDown CommitChanges="True" ID="edNumberDecimalSeporator" runat="server"
					AllowEdit="True" DataField="NumberDecimalSeporator" />
				<px:PXDropDown CommitChanges="True" ID="edNumberGroupSeparator" runat="server" AllowEdit="True"
					DataField="NumberGroupSeparator" />

			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnEditFormatOK" runat="server" DialogResult="Cancel" Text="Close" >
				<AutoCallBack Enabled="true" Target="formEditFormat" Command="Save" />
			</px:PXButton>
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" ActionsPosition="Top" AutoAdjustColumns="True" AllowSearch="true"
		DataSourceID="ds" SkinID="Primary" SyncPosition="True">
		<Levels>
			<px:PXGridLevel DataMember="Locales">
				<Columns>
					<px:PXGridColumn DataField="LocaleName" Width="50px" />
                    <px:PXGridColumn DataField="CultureReadableName" Width="200px" />
					<px:PXGridColumn DataField="TranslatedName" Width="150px" />
					<px:PXGridColumn DataField="Description" Multiline="True" Width="200px" />
					<px:PXGridColumn DataField="Number" TextAlign="Right" Width="50px" />
					<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" />
                    <px:PXGridColumn DataField="ShowValidationWarnings" TextAlign="Center" Type="CheckBox" Width="60px" />
                    <px:PXGridColumn DataField="IsDefault" TextAlign="Center" Type="CheckBox" Width="100px" />
                    <px:PXGridColumn DataField="IsAlternative" TextAlign="Center" Type="CheckBox" Width="100px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar>
			<Actions>
				<EditRecord Enabled="False" />
				<NoteShow Enabled="False" />
			</Actions>
			<%--<CustomItems>
				<px:PXToolBarButton CommandName="editFormatCommand" CommandSourceID="ds" Text="Edit Locale Format" />
			</CustomItems>--%>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
