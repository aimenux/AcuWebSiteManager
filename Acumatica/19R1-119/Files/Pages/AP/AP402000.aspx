<%@ page language="C#" masterpagefile="~/MasterPages/FormDetail.master" autoeventwireup="true" validaterequest="false" codefile="AP402000.aspx.cs" inherits="Page_AP402000" title="Untitled Page" %>

<%@ mastertype virtualpath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="true" TypeName="PX.Objects.AP.APDocumentEnq" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
		    <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewOriginalDocument" Visible="False" />
	            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewDocument" Visible="False" />                        
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edVendorID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSelector CommitChanges="True" ID="edOrganizationID" runat="server" DataField="OrganizationID"/>
			<px:PXSelector CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" AutoRefresh="true"/>
			<px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" AutoRefresh="true"/>
			<px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" />
			<px:PXSegmentMask CommitChanges="True" ID="edSubCD" runat="server" DataField="SubCD" SelectMode="Segment" />
			<px:PXSelector CommitChanges="True" ID="edCuryID" runat="server" DataField="CuryID" />

			<px:PXLayoutRule runat="server" StartColumn="True" />
				<px:PXCheckBox CommitChanges="True" ID="chkShowOpenDocsOnly" runat="server" DataField="ShowAllDocs" AlignLeft="True" TextAlign="Left" LabelWidth="m" />
				<px:PXCheckBox CommitChanges="True" ID="chkIncludeUnreleased" runat="server" DataField="IncludeUnreleased" AlignLeft="True" TextAlign="Left" LabelWidth="m" />
				<px:PXCheckBox CommitChanges="True" ID="chkUseMasterCalendar" runat="server" DataField="UseMasterCalendar" AlignLeft="True" TextAlign="Left" LabelWidth="m" />

			<px:PXPanel runat="server" ID="pnlBalances" RenderStyle="Simple">
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="S" />
				<px:PXNumberEdit ID="edBalanceSummary" runat="server" DataField="BalanceSummary" Enabled="False" />
				<px:PXNumberEdit ID="edVendorBalance" runat="server" DataField="VendorBalance" Enabled="False" />
				<px:PXNumberEdit ID="edVendorDepositsBalance" runat="server" DataField="VendorDepositsBalance" Enabled="False" />
				<px:PXNumberEdit ID="edDifference" runat="server" DataField="Difference" Enabled="False" />
				<px:PXNumberEdit ID="edVendorRetainedBalance" runat="server" DataField="VendorRetainedBalance"/>

				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="S" />
				<px:PXNumberEdit ID="edCuryBalanceSummary" runat="server" DataField="CuryBalanceSummary" Enabled="False" />
				<px:PXNumberEdit ID="edCuryVendorBalance" runat="server" DataField="CuryVendorBalance" Enabled="False" />
				<px:PXNumberEdit ID="edCuryVendorDepositsBalance" runat="server" DataField="CuryVendorDepositsBalance" Enabled="False" />
				<px:PXNumberEdit ID="edCuryDifference" runat="server" DataField="CuryDifference" Enabled="False" />
				<px:PXNumberEdit ID="edCuryVendorRetainedBalance" runat="server" DataField="CuryVendorRetainedBalance"/>
			</px:PXPanel>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="153px" Style="z-index: 100" Width="100%" Caption="Documents" AllowSearch="True" AllowPaging="True" AdjustPageSize="Auto" SkinID="PrimaryInquire"
		FastFilterFields="RefNbr,ExtRefNbr,DocDesc,APInvoice__SuppliedByVendorID" TabIndex="9700" RestrictFields="True" SyncPosition="true" >
		<ActionBar DefaultAction="ViewDocument" />
		<Levels>
			<px:PXGridLevel DataMember="Documents">
				<RowTemplate>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="BranchID" Width="100px" />
					<px:PXGridColumn DataField="DocType" Type="DropDownList" Width="81px" />
					<px:PXGridColumn DataField="RefNbr" Width="100px" LinkCommand="ViewDocument" />
					<px:PXGridColumn DataField="DocDate" Width="90px" />
					<px:PXGridColumn DataField="FinPeriodID" />
					<px:PXGridColumn DataField="Status" Type="DropDownList" Width="72px" />
					<px:PXGridColumn DataField="CuryID" Width="80px"/>
					<px:PXGridColumn DataField="CuryOrigDocAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryBegBalance" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryDiscActTaken" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryTaxWheld" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryRetainageTotal" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryOrigDocAmtWithRetainageTotal" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryRetainageUnreleasedAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="OrigDocAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="BegBalance" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="DocBal" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="DiscActTaken" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="TaxWheld" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="RetainageTotal" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="OrigDocAmtWithRetainageTotal" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="RetainageUnreleasedAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="IsRetainageDocument" Type="CheckBox" Width="80px" />
					<px:PXGridColumn DataField="OrigRefNbr" Width="100px" LinkCommand="ViewOriginalDocument" />
					<px:PXGridColumn DataField="RGOLAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="PaymentMethodID" />
					<px:PXGridColumn DataField="ExtRefNbr" Width="120px" />
					<px:PXGridColumn DataField="DocDesc" Width="180px" />
					<px:PXGridColumn DataField="APInvoice__SuppliedByVendorID" Width="220px" DisplayMode="Hint" />
				</Columns>

				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
