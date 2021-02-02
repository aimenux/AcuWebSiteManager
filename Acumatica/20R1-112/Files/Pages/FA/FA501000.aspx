<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA501000.aspx.cs"
	Inherits="Page_FA501000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Years" TypeName="PX.Objects.FA.GenerationPeriods" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Years" Caption="Parameters">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XS" ControlSize="S" />
			<px:PXTextEdit CommitChanges="True" runat="server" DataField="FromYear" ID="edFromYear" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XS" ControlSize="S" />
			<px:PXTextEdit CommitChanges="True" runat="server" DataField="ToYear" ID="edToYear" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" Caption="Books"
		AdjustPageSize="Auto" AllowPaging="True" FastFilterFields="BookCode, Description" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="Books">
				<Columns>
					<px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="30px"
						AllowCheckAll="True" />
					<px:PXGridColumn AllowUpdate="False" DataField="OrganizationCD" />
					<px:PXGridColumn AllowUpdate="False" DataField="BookCode" DisplayFormat="&gt;CCCCCCCCCC"/>
					<px:PXGridColumn AllowUpdate="False" DataField="Description" />
					<px:PXGridColumn AllowUpdate="False" DataField="FirstCalendarYear" TextAlign="Right" />
					<px:PXGridColumn AllowUpdate="False" DataField="LastCalendarYear" TextAlign="Right" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
	</px:PXGrid>
</asp:Content>
