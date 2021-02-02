<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM202030.aspx.cs" Inherits="Page_SM202030"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="WikiStyles"
		TypeName="PX.SM.WikiCssMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%"  DataMember="WikiStyles" TemplateContainer="">
		<AutoSize Container="Window" Enabled="True" />
		<Activity Width="" Height=""></Activity>
		<Template>			 
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="XXL" />
			<px:PXSelector ID="edNam1e" runat="server" AutoRefresh="True" DataField="Name" DataSourceID="ds" Width="700px"  />
			<px:PXTextEdit ID="edDescription1" runat="server" DataField="Description" Height="53px" TextMode="MultiLine" Width="700px" />
			<px:PXLabel ID="PXHole1" runat="server"></px:PXLabel>
			<px:PXLabel ID="PXHole2" runat="server"></px:PXLabel>
			<px:PXTextEdit ID="edStyle1" runat="server" DataField="Style" Height="500px"	TextMode="MultiLine" Required="True" Width="700px" />
		</Template>		
	</px:PXFormView>
</asp:Content>
