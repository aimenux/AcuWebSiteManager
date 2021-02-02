<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR402000.aspx.cs" Inherits="Page_AR402000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="true" TypeName="PX.Objects.AR.ARDocumentEnq" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues"/>
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewOriginalDocument" Visible="False" />                        
		</CallbackCommands>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edCustomerID" TabIndex="1100">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edOrganizationID" runat="server" DataField="OrganizationID" DataSourceID="ds" />
			<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" AutoRefresh="true" DataSourceID="ds" />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" DataSourceID="ds" />
			<px:PXSelector CommitChanges="True" ID="edPeriod" runat="server" DataField="Period" DataSourceID="ds" AutoRefresh="true" />
			<px:PXSelector CommitChanges="True" ID="edCuryID" runat="server" DataField="CuryID" DataSourceID="ds" />
			<px:PXSegmentMask CommitChanges="True" ID="edARAcctID" runat="server" DataField="ARAcctID" DataSourceID="ds" />
			<px:PXSegmentMask CommitChanges="True" ID="edSubCD" runat="server" DataField="SubCD" SelectMode="Segment" DataSourceID="ds" />

			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
				<px:PXCheckBox CommitChanges="True" ID="chkShowOpenDocsOnly" runat="server" DataField="ShowAllDocs" AlignLeft ="True" TextAlign="Left" LabelWidth="m" />
				<px:PXCheckBox CommitChanges="True" ID="chkIncludeUnreleased" runat="server" DataField="IncludeUnreleased" AlignLeft ="True" TextAlign="Left" LabelWidth="m" />
				<px:PXCheckBox CommitChanges="True" ID="chkUseMasterCalendar" runat="server" DataField="UseMasterCalendar" AlignLeft ="True" TextAlign="Left" LabelWidth="m" />

			<px:PXPanel runat="server" ID="pnlBalances" RenderStyle="Simple">
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="S" />
				<px:PXNumberEdit ID="edBalanceSummary" runat="server" DataField="BalanceSummary" Enabled="False" />
				<px:PXNumberEdit ID="edCustomerBalance" runat="server" DataField="CustomerBalance" Enabled="False" />
				<px:PXNumberEdit ID="edCustomerDepositsBalance" runat="server" DataField="CustomerDepositsBalance" Enabled="False" />
				<px:PXNumberEdit ID="edDifference" runat="server" DataField="Difference" Enabled="False" />
				<px:PXNumberEdit ID="edCustomerRetainedBalance" runat="server" DataField="CustomerRetainedBalance"/>

				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="S" />
				<px:PXNumberEdit ID="edCuryBalanceSummary" runat="server" DataField="CuryBalanceSummary" Enabled="False" />
				<px:PXNumberEdit ID="edCuryCustomerBalance" runat="server" DataField="CuryCustomerBalance" Enabled="False" />
				<px:PXNumberEdit ID="edCuryCustomerDepositsBalance" runat="server" DataField="CuryCustomerDepositsBalance" Enabled="False" />
				<px:PXNumberEdit ID="edCuryDifference" runat="server" DataField="CuryDifference" Enabled="False" />
				<px:PXNumberEdit ID="edCuryCustomerRetainedBalance" runat="server" DataField="CuryCustomerRetainedBalance"/>
			</px:PXPanel>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXCheckBox CommitChanges="True" ID="edIncludeChildAccounts" runat="server" DataField="IncludeChildAccounts" AlignLeft="True" TextAlign="Left" LabelWidth="m" />
		</Template>
		<Activity Width="" Height=""></Activity>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="153px" Style="z-index: 100" Width="100%" Caption="Documents" AllowSearch="True" AdjustPageSize="Auto" SkinID="PrimaryInquire" AllowPaging="True" 
		TabIndex="1300" RestrictFields="True" SyncPosition="true" NoteIndicator="true" FilesIndicator="true" FastFilterFields="RefNbr, ExtRefNbr, DocDesc">
		<ActionBar DefaultAction="ViewDocument"/>
		<Levels>
			<px:PXGridLevel DataMember="Documents">
				<Columns>
					<px:PXGridColumn DataField="CustomerID" Width="100px" />
					<px:PXGridColumn DataField="BranchID" Width="100px" />
					<px:PXGridColumn DataField="DisplayDocType" Type="DropDownList" Width="81px" />
					<px:PXGridColumn DataField="RefNbr" Width="108px" LinkCommand ="ViewDocument" />
					<px:PXGridColumn DataField="FinPeriodID" />
					<px:PXGridColumn DataField="DocDate" Width="90px" />
					<px:PXGridColumn DataField="DueDate" Width="90px" />
					<px:PXGridColumn DataField="Status" Type="DropDownList" Width="72px" />
					<px:PXGridColumn DataField="CuryID" Width="80px" />
					<px:PXGridColumn DataField="CuryOrigDocAmt" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryBegBalance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryDiscActTaken" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryRetainageTotal" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryOrigDocAmtWithRetainageTotal" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryRetainageUnreleasedAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="OrigDocAmt" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="BegBalance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="DocBal" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="DiscActTaken" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="RetainageTotal" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="OrigDocAmtWithRetainageTotal" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="RetainageUnreleasedAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="IsRetainageDocument" Type="CheckBox" Width="80px" />
					<px:PXGridColumn DataField="OrigRefNbr" Width="100px" LinkCommand="ViewOriginalDocument" />
					<px:PXGridColumn DataField="RGOLAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="PaymentMethodID" />
					<px:PXGridColumn DataField="ExtRefNbr" Width="120px" />
					<px:PXGridColumn DataField="DocDesc" Width="180px" />
				</Columns>

<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
