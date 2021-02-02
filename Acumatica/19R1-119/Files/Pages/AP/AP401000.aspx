<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP401000.aspx.cs" Inherits="Page_AP401000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="true" TypeName="PX.Objects.AP.APVendorBalanceEnq" 
		PrimaryView="Filter">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXPanel ID="PXPanel1" runat="server" ContentLayout-OuterSpacing="Horizontal" CssClass="FormContent" RenderStyle="Simple" ContentLayout-SpacingSize="Small" >
		<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edFinPeriodID" SkinID="Transparent" >
			<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
				<px:PXSelector CommitChanges="True" ID="edOrganizationID" runat="server" DataField="OrganizationID"/>
				<px:PXSelector CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" AutoRefresh="true"/>
				<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" AutoRefresh="true"/>
				<px:PXSelector CommitChanges="True" ID="edVendorClassID" runat="server" DataField="VendorClassID" />
				<px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" />
				<px:PXSegmentMask CommitChanges="True" ID="edSubID" runat="server" DataField="SubID" SelectMode="Segment" />
				<px:PXSelector CommitChanges="True" ID="edCuryID" runat="server" DataField="CuryID" />
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
				<px:PXCheckBox CommitChanges="True" ID="chkSplitByCurrency" runat="server" DataField="SplitByCurrency" AlignLeft="True"  />
				<px:PXCheckBox CommitChanges="True" ID="chkShowWithBalanceOnly" runat="server" DataField="ShowWithBalanceOnly" AlignLeft="True"  />
			    <px:PXCheckBox CommitChanges="True" ID="chkUseMasterCalendar" runat="server" DataField="UseMasterCalendar" AlignLeft="True"  />
			</Template>
		</px:PXFormView>
		<px:PXLayoutRule runat="server" StartColumn="True" />
		<px:PXFormView ID="form2" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Summary" SkinID="Transparent" >
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
						<px:PXNumberEdit ID="edBalanceSummary" runat="server" DataField="BalanceSummary" Enabled="False" />
						<px:PXNumberEdit ID="edDepositsSummary" runat="server" DataField="DepositsSummary" Enabled="False" />
						<px:PXNumberEdit ID="edCuryBalanceSummary" runat="server" DataField="CuryBalanceSummary" Enabled="False" />
						<px:PXNumberEdit ID="edCuryDepositsSummary" runat="server" DataField="CuryDepositsSummary" Enabled="False" />
						<px:PXNumberEdit ID="edBalanceRetainedSummary" runat="server" DataField="BalanceRetainedSummary" />
						<px:PXNumberEdit ID="edCuryBalanceRetainedSummary" runat="server" DataField="CuryBalanceRetainedSummary" />
					</Template>
				</px:PXFormView>		
	</px:PXPanel>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="153px" Style="z-index: 100" Width="100%" Caption="Vendors" AllowSearch="True" AllowPaging="True" AdjustPageSize="Auto" SkinID="PrimaryInquire" MatrixMode="True"
		FastFilterFields="VendorID,AcctName" TabIndex="7900" RestrictFields="True" SyncPosition="True">
		<AutoCallBack Target="grid" Command="Refresh" ActiveBehavior="true"><Behavior RepaintControls="None" RepaintControlsIDs="form2" /></AutoCallBack>
		<Levels>
			<px:PXGridLevel DataMember="History">
				<RowTemplate>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="AcctCD" Width="100px" LinkCommand="viewDetails" />
					<px:PXGridColumn DataField="AcctName" Width="250px" />
					<px:PXGridColumn DataField="FinPeriodID" />
					<px:PXGridColumn DataField="CuryID" Width="80px" />
					<px:PXGridColumn DataField="CuryBegBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryEndBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryDepositsBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryPurchases" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryPayments" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryBegRetainedBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryEndRetainedBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryRetainageWithheld" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryRetainageReleased" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryDiscount" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryWhTax" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryCrAdjustments" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryDrAdjustments" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryDeposits" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="BegBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="EndBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="DepositsBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="Purchases" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="Payments" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="BegRetainedBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="EndRetainedBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="RetainageWithheld" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="RetainageReleased" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="Discount" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="WhTax" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="RGOL" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CrAdjustments" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="DrAdjustments" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="Deposits" TextAlign="Right" Width="81px" />
				</Columns>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar DefaultAction="viewDetails" />
	</px:PXGrid>
</asp:Content>
