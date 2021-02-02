<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AU203001.aspx.cs" Inherits="Page_AU203001" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<label class="projectLink transparent border-box">Customized Data Classes</label>
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="PX.SM.GraphTableList"
        PrimaryView="Filter"
        >
		<CallbackCommands>

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%">
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px"  SyncPosition="True"
		SkinID="Primary" BatchUpdate="False" AutoAdjustColumns="True">
		<Levels>
			<px:PXGridLevel DataMember="ViewTables">
				<Columns>
					<px:PXGridColumn DataField="ShortName" Width="150px" LinkCommand="actionEdit" />
					<px:PXGridColumn DataField="TableName" Width="300px" />
					<px:PXGridColumn DataField="DBName" Width="150px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar ActionsText="False" Position="Top">
			<Actions>
				<AddNew ToolBarVisible="False"/>
				<ExportExcel ToolBarVisible="False"/>
				<AdjustColumns ToolBarVisible="False"/>
			</Actions>
		</ActionBar>
		<Mode InitNewRow="False" AllowRowSelect="False"></Mode>
	</px:PXGrid>

	<px:PXSmartPanel ID="WizardAddTable" runat="server"
		CaptionVisible="True"
		Caption="Select Existing Data Access Class"
		AutoRepaint="True"
		Key="TableSelectorDlg">
		<px:PXFormView ID="FormSelectTable" runat="server"
			SkinID="Transparent" DataMember="TableSelectorDlg">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="S" ControlSize="M" />
				<px:PXSelector ID="TableName" runat="server" DataField="TableName" CommitChanges="True"/>
			</Template>
		</px:PXFormView>

		<px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartRow="True" />
		<px:PXPanel ID="PXPanel7" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton13" runat="server" DialogResult="OK" Text="OK"/>
			<px:PXButton ID="PXButton14" runat="server" DialogResult="Cancel" Text="Cancel" CausesValidation="False"/>
			
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
