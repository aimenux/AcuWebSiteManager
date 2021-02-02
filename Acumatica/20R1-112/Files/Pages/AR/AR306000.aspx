<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR306000.aspx.cs" Inherits="Page_AR306000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Document" TypeName="PX.Objects.AR.ARDunningLetterMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Visible="true" Name="ViodLetter" />
			<px:PXDSCallbackCommand Visible="true" Name="PrintLetter" />
			<px:PXDSCallbackCommand Visible="false" Name="Revoke" CommitChanges="true" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewDocument" CommitChanges="true" DependOnGrid="grid" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Document">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="S" />
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
			<px:PXNumberEdit ID="edLevel" runat="server" DataField="DunningLetterLevel" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="S" />
			<px:PXFormView ID="DunningForm" runat="server" DataMember="CurrentCustomer" RenderStyle="Simple">
				<Template>
					<px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="S" />
					<px:PXSelector ID="edCustomerID" runat="server" DataField="AcctCD" />
				</Template>
			</px:PXFormView>
			<px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="S" />
			<px:PXDateTimeEdit ID="edDate" runat="server" DataField="DunningLetterDate" Size="S" />
			<px:PXSelector ID="edFeeRefNbr" runat="server" DataField="FeeRefNbr" AllowEdit="true" Enabled="false" />
		</Template>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100; height: 150px;"
		Width="100%" SkinID="Details" AllowPaging="true"
		AdjustPageSize="Auto">
		<Levels>
			<px:PXGridLevel DataMember="Details">
				<Columns>
					<px:PXGridColumn DataField="BAccountID" />
					<px:PXGridColumn DataField="DocType" DisplayMode="Text" />
					<px:PXGridColumn DataField="RefNbr" LinkCommand="ViewDocument" />
					<px:PXGridColumn DataField="DueDate" />
					<px:PXGridColumn AllowNull="False" DataField="OrigDocAmt" TextAlign="Right" />
					<px:PXGridColumn AllowNull="False" DataField="OverdueBal" TextAlign="Right" />
					<px:PXGridColumn DataField="ARDunningLetter__DunningLetterDate" />
					<px:PXGridColumn DataField="DunningLetterLevel" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
		<ActionBar>
			<Actions>
				<AddNew Enabled="False" MenuVisible="False" GroupIndex="1" />
			</Actions>
			<CustomItems>
				<px:PXToolBarButton Text="Revoke" Key="cmdeRevoke">
					<AutoCallBack Command="Revoke" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
