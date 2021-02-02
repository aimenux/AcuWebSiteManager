<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP503000.aspx.cs" Inherits="Page_AP503000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AP.APPayBills" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
			<px:PXDSCallbackCommand DependOnGrid="grid2" Name="ViewInvoice" Visible="False"/>
			<px:PXDSCallbackCommand DependOnGrid="grid1" Name="ViewOriginalDocument" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="grid1" CommitChanges="True" Name="AddJointAmounts" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edPayTypeID" TabIndex="6100">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
		    <px:PXSelector CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
			<px:PXSelector CommitChanges="True" ID="edPayTypeID" runat="server" DataField="PayTypeID" AutoRefresh="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edPayAccountID" runat="server" DataField="PayAccountID" AutoRefresh="True" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edSelectionDate" runat="server" DataField="PayDate" />
			<px:PXSelector CommitChanges="True" ID="edPayFinPeriodID" runat="server" DataField="PayFinPeriodID" AutoRefresh="True" />
			<pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server" RateTypeView="_PayBillsFilter_CurrencyInfo_" DataMember="_Currency_"></pxa:PXCurrencyRate>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
		    <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXCheckBox AlignLeft="True" Size="M" CommitChanges="True" ID="chkOverDueIn" runat="server" DataField="ShowPayInLessThan" />
			<px:PXNumberEdit CommitChanges="True" Size="xxs" ID="edOverDueIn" runat="server" DataField="PayInLessThan" SuppressLabel="True" />
			<px:PXTextEdit SuppressLabel="True" ID="edDays1" DataField="Days" runat="server" SkinID="Label" Enabled="False" />
			<px:PXLayoutRule runat="server" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXCheckBox AlignLeft="True" Size="M" CommitChanges="True" ID="chkDiscountExparedWithinLast" runat="server" DataField="ShowDueInLessThan" />
			<px:PXNumberEdit CommitChanges="True" Size="xxs" ID="edDiscountExparedWithinLast" runat="server" DataField="DueInLessThan" SuppressLabel="True" />
			<px:PXTextEdit SuppressLabel="True" ID="edDays2" DataField="Days" runat="server" SkinID="Label" Enabled="False" />
			<px:PXLayoutRule runat="server" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXCheckBox AlignLeft="True" Size="M" CommitChanges="True" ID="chkDiscountExpiresInLessThan" runat="server" DataField="ShowDiscountExpiresInLessThan" />
			<px:PXNumberEdit CommitChanges="True" Size="xxs" ID="edDiscountExpiresInLessThan" runat="server" DataField="DiscountExpiresInLessThan" SuppressLabel="True" />
			<px:PXTextEdit SuppressLabel="True" ID="edDays3" DataField="Days" runat="server" SkinID="Label" Enabled="False" />
			<px:PXLayoutRule runat="server" />
			<px:PXCheckBox AlignLeft="True" Size="M" CommitChanges="True" ID="chkTakeDiscAlways" runat="server" DataField="TakeDiscAlways" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXNumberEdit ID="edGLBalance" runat="server" DataField="GLBalance" Enabled="False" />
			<px:PXNumberEdit ID="edCashBalance" runat="server" DataField="CashBalance" Enabled="False" />
			<px:PXNumberEdit ID="edCurySelTotal" runat="server" DataField="CurySelTotal" Enabled="False" />
			<px:PXNumberEdit ID="edSelCount" runat="server" DataField="SelCount" Enabled="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server">
		<Items>
			<px:PXTabItem Text="Documents to Pay">
				<Template>
					<px:PXGrid ID="grid1" runat="server" Height="288px" Style="z-index: 100" Width="100%" AllowPaging="True" PageSize="100" SkinID="Details" DataSourceID="ds" TabIndex="6300" NotesIndicator="false" FilesIndicator="False" NoteField="" NoteDocsField="">
						
						<Levels>
							<px:PXGridLevel DataMember="APDocumentList">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
									<px:PXDropDown ID="AdjdDocType" runat="server" DataField="AdjdDocType"
										CommitChanges="True" />
									<px:PXSelector ID="edAdjdRefNbr" runat="server" DataField="AdjdRefNbr"
										AutoRefresh="True" AllowEdit="True" edit="1" CommitChanges="true">
										<Parameters>
											<px:PXControlParam ControlID="grid1" Name="APAdjust.adjdDocType" PropertyName="DataValues[&quot;AdjdDocType&quot;]" Type="String" />
										</Parameters>
									</px:PXSelector>
								    <px:PXSelector ID="edAdjdLineNbr" runat="server" DataField="AdjdLineNbr"
								                   AutoRefresh="True" CommitChanges="true"  >
								        <Parameters>
								            <px:PXControlParam ControlID="grid1" Name="APAdjust.adjdDocType" PropertyName="DataValues[&quot;AdjdDocType&quot;]" Type="String" />
								            <px:PXControlParam ControlID="grid1" Name="APAdjust.adjdRefNbr" PropertyName="DataValues[&quot;AdjdRefNbr&quot;]" Type="String" />
								        </Parameters>
								    </px:PXSelector>
									<px:PXCheckBox ID="SeparateCheck" runat="server" DataField="SeparateCheck" Text="Pay Separately" />
									<px:PXNumberEdit ID="CuryAdjgDiscAmt" runat="server" DataField="CuryAdjgDiscAmt" />
									<px:PXNumberEdit ID="CuryAdjgAmt" runat="server" DataField="CuryAdjgAmt" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" AutoCallBack="True" />
									<px:PXGridColumn DataField="AdjdDocType" Type="DropDownList" AutoCallBack="true" />
									<px:PXGridColumn DataField="AdjdRefNbr" AutoCallBack="true"/>
									<px:PXGridColumn DataField="VendorID" />
									<px:PXGridColumn DataField="AdjdLineNbr" AutoCallBack="true"/>
									<px:PXGridColumn DataField="VendorID_Vendor_acctName" />
									<px:PXGridColumn DataField="APTran__ProjectID" />
									<px:PXGridColumn DataField="APTran__TaskID" />
									<px:PXGridColumn DataField="APTran__CostCodeID" />
									<px:PXGridColumn DataField="APTran__AccountID" />
									<px:PXGridColumn DataField="APTran__InventoryID" />
									<px:PXGridColumn DataField="APInvoice__VendorLocationID" />
									<px:PXGridColumn DataField="APInvoice__SuppliedByVendorID" DisplayMode="Hint"/>
									<px:PXGridColumn DataField="SeparateCheck" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="APInvoice__IsRetainageDocument" Type="CheckBox" />
									<px:PXGridColumn DataField="APInvoice__OrigRefNbr"  LinkCommand="ViewOriginalDocument" />
									<px:PXGridColumn DataField="APInvoice__PayDate" />
									<px:PXGridColumn DataField="APInvoice__DueDate" />
									<px:PXGridColumn DataField="APInvoice__DiscDate" />
									<px:PXGridColumn DataField="APInvoice__DocDate" />
									<px:PXGridColumn DataField="CuryAdjgAmt" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="APInvoice__IsJointPayees" Type="CheckBox" TextAlign="Center" />
									<px:PXGridColumn DataField="CuryAdjgDiscAmt" TextAlign="Right" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryDiscBal" TextAlign="Right" />
									<px:PXGridColumn DataField="APInvoice__CuryID" />
									<px:PXGridColumn DataField="APInvoice__InvoiceNbr" />
									<px:PXGridColumn DataField="APInvoice__DocDesc" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Window" Enabled="True" MinHeight="300" />
						<Mode AllowDelete="True" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton StateColumn="APInvoice__IsJointPayees">
                                    <AutoCallBack Command="AddJointAmounts" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Exceptions">
				<Template>
					<px:PXGrid ID="grid2" runat="server" Height="288px" Style="z-index: 100" Width="100%" AllowPaging="True" AdjustPageSize="Auto" SkinID="Details" DataSourceID="ds" TabIndex="6300" NotesIndicator="false" FilesIndicator="False" NoteField="" NoteDocsField="">
						<Mode InitNewRow="True"></Mode>
						<Levels>
							<px:PXGridLevel DataMember="APExceptionsList">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXCheckBox ID="PXCheckBox1" runat="server" DataField="Selected" />
									<px:PXDropDown ID="PXDropDown1" runat="server" DataField="AdjdDocType"
										CommitChanges="True" />
									<px:PXSelector ID="PXSelector1" runat="server" DataField="AdjdRefNbr"
										AutoRefresh="True" AllowEdit="True" edit="1" >
										<Parameters>
											<px:PXControlParam ControlID="grid2" Name="APAdjust.adjdDocType" PropertyName="DataValues[&quot;AdjdDocType&quot;]" Type="String" />
										</Parameters>
									</px:PXSelector>
									<px:PXCheckBox ID="PXCheckBox2" runat="server" DataField="SeparateCheck" Text="Pay Separately" />
									<px:PXNumberEdit ID="PXNumberEdit1" runat="server" DataField="CuryAdjgDiscAmt" />
									<px:PXNumberEdit ID="PXNumberEdit2" runat="server" DataField="CuryAdjgAmt" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="AdjdDocType" Type="DropDownList" AutoCallBack="true" />
									<px:PXGridColumn DataField="AdjdRefNbr" AutoCallBack="true" LinkCommand="ViewInvoice"/>
									<px:PXGridColumn DataField="VendorID" />
									<px:PXGridColumn DataField="AdjdLineNbr" AutoCallBack="true"/>
									<px:PXGridColumn DataField="VendorID_Vendor_acctName" />
									<px:PXGridColumn DataField="APTran__ProjectID" />
									<px:PXGridColumn DataField="APTran__TaskID" />
									<px:PXGridColumn DataField="APTran__CostCodeID" />
									<px:PXGridColumn DataField="APTran__AccountID" />
									<px:PXGridColumn DataField="APTran__InventoryID" />
									<px:PXGridColumn DataField="APInvoice__VendorLocationID" />
									<px:PXGridColumn DataField="APInvoice__SuppliedByVendorID" DisplayMode="Hint"/>
									<px:PXGridColumn DataField="SeparateCheck" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="APInvoice__IsRetainageDocument" Type="CheckBox" />
									<px:PXGridColumn DataField="APInvoice__OrigRefNbr" LinkCommand="ViewInvoice" />
									<px:PXGridColumn DataField="APInvoice__PayDate" />
									<px:PXGridColumn DataField="APInvoice__DueDate" />
									<px:PXGridColumn DataField="APInvoice__DiscDate" />
									<px:PXGridColumn DataField="APInvoice__DocDate" />
									<px:PXGridColumn CommitChanges="True" LinkCommand="EditAmountPaid" DataField="CuryAdjgAmt" TextAlign="Right" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryAdjgDiscAmt" TextAlign="Right" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryDiscBal" TextAlign="Right" />
									<px:PXGridColumn DataField="APInvoice__CuryID" />
									<px:PXGridColumn DataField="APInvoice__InvoiceNbr" />
									<px:PXGridColumn DataField="APInvoice__DocDesc" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Window" Enabled="True" MinHeight="300" />
						<Mode InitNewRow="True" AllowDelete="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
	</px:PXTab>
    <px:PXSmartPanel runat="server" ID="PanelPayBillJC" Height="400px" Width="950px" LoadOnDemand="True" CaptionVisible="True" Caption="Indicate Amounts to Pay" Key="JointPayees" CommandSourceID="ds" AutoCallBack-Command="Refresh" AutoCallBack-Target="PXGridJointCheck" AutoReload="True">
        <px:PXFormView runat="server" ID="frmVendor" SkinID="Transparent" DataSourceID="ds" DataMember="CurrentBill">
            <Template>
                <px:PXLayoutRule runat="server" ID="PXLayoutRule7" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                <px:PXSegmentMask runat="server" DataField="VendorID" Enabled="False" ID="edUsrVendor" />
                <px:PXLayoutRule runat="server" ID="PXLayoutRule9" StartColumn="True" LabelsWidth="S" ControlSize="S" />
                <px:PXNumberEdit runat="server" DataField="AmountToPay" ID="edAmountToPay" CommitChanges="True" />
                <px:PXLayoutRule runat="server" ID="PXLayoutRule11" StartColumn="True" LabelsWidth="M" ControlSize="S" />
                <px:PXNumberEdit runat="server" Enabled="False" DataField="VendorBalance" ID="edUsrBalance" />
            </Template>
        </px:PXFormView>
        <px:PXGrid runat="server" ID="PXGridJointCheck" Height="200px" SkinID="Inquire" TabIndex="17500" Width="100%" BatchUpdate="True" FastFilterFields="JointPayeeInternalId">
            <Levels>
                <px:PXGridLevel DataMember="JointPayeePayments">
                    <Columns>
                        <px:PXGridColumn DataField="JointPayee__JointPayeeInternalId" />
                        <px:PXGridColumn DataField="JointPayee__JointPayeeExternalName" />
                        <px:PXGridColumn DataField="JointPayee__BillLineNumber" />
                        <px:PXGridColumn DataField="JointAmountToPay" CommitChanges="True" />
                        <px:PXGridColumn DataField="JointPayee__JointBalance" />
                    </Columns>
                    <RowTemplate>
                        <px:PXNumberEdit runat="server" ID="PXNumberEdit8" DataField="JointAmountToPay" />
                    </RowTemplate>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel runat="server" ID="IndicateAmounts" SkinID="Buttons">
            <px:PXButton runat="server" ID="btnOK123" Text="Confirm" DialogResult="OK" />
            <px:PXButton runat="server" ID="btnCancel123" Text="Cancel" DialogResult="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
