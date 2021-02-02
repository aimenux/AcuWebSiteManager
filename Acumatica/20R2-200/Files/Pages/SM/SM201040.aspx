<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM201040.aspx.cs" Inherits="Page_SM206000"
	Title="Relation Entity Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.RelationEntities"
		PrimaryView="HeaderEntity">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" HideText="True"/>
			<px:PXDSCallbackCommand CommitChanges="False" Name="Cancel" HideText="True"/>
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" HideText="True"/>
			<px:PXDSCallbackCommand Name="Previous" PostData="Self" HideText="True"/>
			<px:PXDSCallbackCommand Name="Next" PostData="Self" HideText="True"/>
			<px:PXDSCallbackCommand Name="Last" PostData="Self" HideText="True"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		Caption="Restricted Entity" DataMember="HeaderEntity" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
			<px:PXSelector ID="edEntityTypeName" runat="server" DataField="EntityTypeName" DataSourceID="ds" CommitChanges="True" DisplayMode="Text" />
			<px:PXSelector ID="edEntity" runat="server" DataField="Entity" DataSourceID="ds" CommitChanges="True" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" AllowPaging="True" AdjustPageSize="Auto" ActionsPosition="Top" Caption="Restriction Groups"
		BatchUpdate="True" AllowSearch="True" SkinID="Inquire">
		<Levels>
			<px:PXGridLevel DataMember="DetailsEntity">
				<Mode AllowAddNew="False" AllowDelete="False" />
				<Columns>
					<px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox"
						Width="40px" />
					<px:PXGridColumn DataField="GroupName" Width="150px" />
					<px:PXGridColumn DataField="Description" Width="300px" />
					<px:PXGridColumn AllowNull="False" DataField="Active" TextAlign="Center" Type="CheckBox"
						Width="60px" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="GroupType" Label="Visible To Entities"
						RenderEditorText="True" Width="171px" />
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" />
					<px:PXCheckBox SuppressLabel="True" ID="chkSelected" runat="server" DataField="Selected"
						AlignLeft="True" />
					<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXCheckBox SuppressLabel="True" ID="chkActive" runat="server" Checked="True"
						DataField="Active" AlignLeft="True" />
					<px:PXDropDown ID="edGroupType" runat="server" AllowNull="False" DataField="GroupType"
						Enabled="False" />
				</RowTemplate>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" />
		<Mode AllowAddNew="False" AllowDelete="False" />
		<ActionBar>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
