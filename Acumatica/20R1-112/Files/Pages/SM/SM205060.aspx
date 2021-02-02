<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM205060.aspx.cs" Inherits="Page_SM205060" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter"
		TypeName="PX.SM.AUReportProcess" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand StartNewGroup="True" CommitChanges="True" Name="Process" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" />			
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" Caption="Selection"
		DataMember="Filter" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" 
				LabelsWidth="SM" />
			<px:PXSelector ID="edReportID" runat="server" DataField="ReportID"  DisplayMode="Text" FilterByAllFields="true" CommitChanges="True" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXSelector CommitChanges="True" Size="M" ID="edUsername" runat="server" DataField="Username"
				TextField="username" DataSourceID="ds" />
			<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMy" runat="server"
				DataField="My" AlignLeft="True" />
			<px:PXLayoutRule runat="server" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Width="100%"
		Caption="Templates" SkinID="Inquire" AutoAdjustColumns="True">		
		<Levels>
			<px:PXGridLevel DataMember="Templates">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
					<px:PXTextEdit Size="s" ID="edScreenID_description" runat="server" DataField="ScreenID_description" />
					<px:PXLayoutRule runat="server" />
					<px:PXSelector ID="edScreenID" runat="server" DataField="ScreenID" />
					<px:PXSelector ID="edUsername" runat="server" DataField="Username" TextField="username" />
					<px:PXMaskEdit ID="edName" runat="server" DataField="Name" />
					<px:PXCheckBox ID="chkIsDefault" runat="server" DataField="IsDefault" />
					<px:PXCheckBox ID="chkIsShared" runat="server" DataField="IsShared" />
					<px:PXCheckBox ID="chkMergeReports" runat="server" DataField="MergeReports" />
					<px:PXNumberEdit ID="edMergingOrder" runat="server" DataField="MergingOrder"  />					
				</RowTemplate>
				<Columns>
					<px:PXGridColumn AllowNull="False" DataField="Selected" Label="Selected" TextAlign="Center"
						Type="CheckBox" />
					<px:PXGridColumn DataField="Name" Label="Template" Width="158px" />
					<px:PXGridColumn DataField="ScreenID" Label="Report ID" Width="108px" />
					<px:PXGridColumn DataField="ScreenID_description" Label="Title" Width="300px" />
					<px:PXGridColumn DataField="Username" Label="User" Width="158px" TextField="Username_description" />
					<px:PXGridColumn AllowNull="False" DataField="IsDefault" Label="Default" TextAlign="Center"
						Type="CheckBox" />
					<px:PXGridColumn AllowNull="False" DataField="IsShared" Label="Shared" TextAlign="Center"
						Type="CheckBox" />
					<px:PXGridColumn DataField="MergeReports" TextAlign="Center" Type="CheckBox" AutoCallBack="True"/>
					<px:PXGridColumn DataField="MergingOrder" TextAlign="Right"/>
				</Columns>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowSort="False" />
	</px:PXGrid>
</asp:Content>
