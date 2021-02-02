<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM201045.aspx.cs" Inherits="Page_SM201030"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Width="100%" PrimaryView="TraceFilter"
		TypeName="PX.SM.LoginTraceMaintenance" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="previousperiod" StartNewGroup="True" HideText="True"/>
			<px:PXDSCallbackCommand Name="nextperiod" HideText="True"/>
			<px:PXDSCallbackCommand Name="deletehistory" StartNewGroup="True" HideText="True"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="TraceFilter" Caption="Selection" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
			<px:PXSelector CommitChanges="True" ID="edUsername" runat="server" DataField="Username"
				ValueField="Username" DataSourceID="ds" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edDateFrom" runat="server" DataField="DateFrom"
				EditFormat="g" Size="M" />
			<px:PXDropDown CommitChanges="True" ID="edOperation" runat="server" DataField="Operation" Size="M" />
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
			<px:PXLabel ID="PXHole" runat="server"></px:PXLabel>
			<px:PXDateTimeEdit CommitChanges="True" ID="edDateTo" runat="server" DataField="DateTo"
				EditFormat="g" Size="M" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;
		left: 0px; top: 0px;" Width="100%" ActionsPosition="Top" AllowPaging="True" Caption="Activity"
		AllowSearch="True" AdjustPageSize="Auto" SkinID="Inquire">
		<Levels>
			<px:PXGridLevel DataMember="LoginTraces">
				<Columns>
					<px:PXGridColumn DataField="Date" Width="100px" DisplayFormat="g" />
					<px:PXGridColumn DataField="Username" Width="200px" />
					<px:PXGridColumn DataField="Operation" RenderEditorText="True" Width="100px" />
					<px:PXGridColumn DataField="Host" Width="200px"/>
					<px:PXGridColumn DataField="IPAddress" Width="150px" />
					<px:PXGridColumn DataField="ScreenID" Width="90px" />
					<px:PXGridColumn DataField="ScreenID_SiteMap_Title" Width="90px" />
					<px:PXGridColumn DataField="Comment" Width="200px" />
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" />
					<px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date" />
					<px:PXTextEdit ID="edUsername" runat="server" DataField="Username" />
					<px:PXDropDown ID="edOperation" runat="server" DataField="Operation" />
					<px:PXTextEdit ID="edHost" runat="server" DataField="Host"/>
					<px:PXTextEdit ID="edIPAddress" runat="server" DataField="IPAddress" /></RowTemplate>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar DefaultAction="cmdAcctDetails" PagerVisible="False">
			<PagerSettings Mode="NextPrevFirstLast" />
		</ActionBar>
	</px:PXGrid>
</asp:Content>
