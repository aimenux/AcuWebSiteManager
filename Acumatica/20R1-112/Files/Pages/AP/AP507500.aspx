<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP507500.aspx.cs" Inherits="Page_AP507500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AP.MISC1099EFileProcessing" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="view1099Summary" Visible="False" CommitChanges="true"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="Filter" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
			<px:PXBranchSelector CommitChanges="True" ID="BranchSelector1" runat="server" DataField="OrgBAccountID"/>
			<px:PXSelector ID="edFinYear" runat="server" DataField="FinYear" CommitChanges="True" AutoRefresh="True" />
			<px:PXDropDown ID="edInclude" runat="server" DataField="Include" CommitChanges="True" />
			<px:PXDropDown ID="edBox7" runat="server" DataField="Box7" CommitChanges="True" />
			<px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
			<px:PXCheckBox ID="edIsPriorYear" runat="server" DataField="IsPriorYear" />
			<px:PXCheckBox ID="edIsCorrectionReturn" runat="server" DataField="IsCorrectionReturn" />
			<px:PXCheckBox ID="edIsLastFiling" runat="server" DataField="IsLastFiling" />
			<px:PXCheckBox ID="edReportingDirectSalesOnly" runat="server" DataField="ReportingDirectSalesOnly" />
			<px:PXCheckBox ID="edIsTestMode" runat="server" DataField="IsTestMode" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" Height="150px" AllowPaging="true" AdjustPageSize="Auto" SkinID="Details" FastFilterFields="VAcctCD, VAcctName" SyncPosition="true">
		<ActionBar>
			<Actions>
				<AddNew ToolBarVisible="False" />
				<Delete ToolBarVisible="False" />
			</Actions>
			<CustomItems>
				<px:PXToolBarButton Text="View 1099 Vendor History">
					<AutoCallBack Command="view1099Summary" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<Levels>
			<px:PXGridLevel DataMember="Records">
				<Columns>
					<px:PXGridColumn DataField="Selected" Width="30px" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False"></px:PXGridColumn>
					<px:PXGridColumn DataField="VAcctCD" />
					<px:PXGridColumn DataField="VAcctName" />
					<px:PXGridColumn DataField="HistAmt" TextAlign="Right" />
					<px:PXGridColumn DataField="LTaxRegistrationID" />
					<px:PXGridColumn DataField="PayerBAccountID" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400"></AutoSize>
	</px:PXGrid>
</asp:Content>
