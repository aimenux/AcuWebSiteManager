<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM201030.aspx.cs" Inherits="Page_SM205000"
	Title="Relation Group Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.RelationGroups" PrimaryView="HeaderGroup">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="HeaderGroup" Caption="Restriction Group" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
			<px:PXSelector Size="M" ID="edGroupName" runat="server" DataField="GroupName" DataSourceID="ds" />
			<px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
			<px:PXDropDown ID="edGroupType" runat="server" AllowNull="False" DataField="GroupType" Size="M" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
			<px:PXLabel ID="PXHole" runat="server" />
			<px:PXSelector ID="edEntityType" runat="server" DataField="EntityTypeName"	DataSourceID="ds" Size="M" CommitChanges="True" />
			<px:PXSelector ID="edSpecificType" runat="server" DataField="SpecificType" DataSourceID="ds" Size="M" CommitChanges="True" />
			<px:PXSelector ID="edSpecificModule" runat="server" DataField="SpecificModule" DataSourceID="ds" Size="M" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" AllowPaging="True" AdjustPageSize="Auto" ActionsPosition="Top" Caption="Entities"
		BatchUpdate="True" AllowSearch="True" SkinID="Details">
		<Levels>
			<px:PXGridLevel DataMember="DetailsGroup">
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="100px" />
					<px:PXGridColumn DataField="ID" TextAlign="Right" AllowShowHide="False" Visible="False" />
					<px:PXGridColumn DataField="Entity" AllowUpdate="False" Width="300px" />
				</Columns>
				<Mode AllowAddNew="False" AllowDelete="False" />
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXCheckBox SuppressLabel="True" ID="chkSelected" runat="server" DataField="Selected" />
					<px:PXTextEdit ID="edEntity" runat="server" DataField="Entity" Enabled="False" /></RowTemplate>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" />
		<Mode AllowAddNew="False" AllowDelete="False" />
		<ActionBar>
			<Actions>
				<AddNew Enabled="False" />
				<Delete Enabled="False" />
			</Actions>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
