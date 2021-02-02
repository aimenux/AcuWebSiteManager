<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP401000.aspx.cs" Inherits="Page_AP401000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="true" TypeName="PX.Objects.AP.APVendorBalanceEnq" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewDetails" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXPanel ID="PXPanel1" runat="server" ContentLayout-OuterSpacing="Horizontal" CssClass="FormContent" RenderStyle="Simple" ContentLayout-SpacingSize="Small" >
		<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edFinPeriodID" SkinID="Transparent" >
			<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
				<px:PXBranchSelector CommitChanges="True" ID="BranchSelector1" runat="server" DataField="OrgBAccountID" InitialExpandLevel="0" />
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
		FastFilterFields="AcctCD,AcctName" TabIndex="7900" RestrictFields="True" NoteIndicator="false" FilesIndicator="false" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="History">
				<RowTemplate>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="AcctCD" LinkCommand="viewDetails" />
					<px:PXGridColumn DataField="AcctName" />
					<px:PXGridColumn DataField="FinPeriodID" />
					<px:PXGridColumn DataField="CuryID" />
					<px:PXGridColumn DataField="CuryBegBalance" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryEndBalance" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryDepositsBalance" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryPurchases" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryPayments" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryBegRetainedBalance" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryEndRetainedBalance" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryRetainageWithheld" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryRetainageReleased" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryDiscount" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryWhTax" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryCrAdjustments" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryDrAdjustments" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryDeposits" TextAlign="Right" />
					<px:PXGridColumn DataField="BegBalance" TextAlign="Right" />
					<px:PXGridColumn DataField="EndBalance" TextAlign="Right" />
					<px:PXGridColumn DataField="DepositsBalance" TextAlign="Right" />
					<px:PXGridColumn DataField="Purchases" TextAlign="Right" />
					<px:PXGridColumn DataField="Payments" TextAlign="Right" />
					<px:PXGridColumn DataField="BegRetainedBalance" TextAlign="Right" />
					<px:PXGridColumn DataField="EndRetainedBalance" TextAlign="Right" />
					<px:PXGridColumn DataField="RetainageWithheld" TextAlign="Right" />
					<px:PXGridColumn DataField="RetainageReleased" TextAlign="Right" />
					<px:PXGridColumn DataField="Discount" TextAlign="Right" />
					<px:PXGridColumn DataField="WhTax" TextAlign="Right" />
					<px:PXGridColumn DataField="RGOL" TextAlign="Right" />
					<px:PXGridColumn DataField="CrAdjustments" TextAlign="Right" />
					<px:PXGridColumn DataField="DrAdjustments" TextAlign="Right" />
					<px:PXGridColumn DataField="Deposits" TextAlign="Right" />
				</Columns>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar DefaultAction="viewDetails" />
	</px:PXGrid>
</asp:Content>
