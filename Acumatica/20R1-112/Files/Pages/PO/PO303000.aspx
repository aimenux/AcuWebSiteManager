<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PO303000.aspx.cs" Inherits="Page_PO303000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PO.POLandedCostDocEntry" PrimaryView="Document" >
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
            <px:PXDSCallbackCommand Name="Action" CommitChanges="True" />
			<px:PXDSCallbackCommand Visible="false" Name="CreateAPInvoice" CommitChanges="true" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="Release" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddPOReceipt" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddPOReceipt2"  StartNewGroup="True" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddPOReceiptLine" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddPOReceiptLine2" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddLC" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddLC2" CommitChanges="true" />

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%"
		DataMember="Document" Caption="Document Summary" NoteIndicator="True"
		FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity"
		LinkIndicator="True" NotifyIndicator="True" DefaultControlID="edDocType"
		TabIndex="100" DataSourceID="ds" MarkRequired="Dynamic">
		<CallbackCommands>
			<Save PostData="Self" />
		</CallbackCommands>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXDropDown ID="edDocType" runat="server" DataField="DocType" Enabled="False"/>
			<px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" DataSourceID="ds">
				<GridProperties FastFilterFields="POLandedCostDoc__RefNbr, VendorID, VendorID_Vendor_acctName" />
			</px:PXSelector>
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
			<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkHold" runat="server" DataField="Hold" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edDocDate" runat="server" DataField="DocDate" />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" DataSourceID="ds" />
			
			<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="S" StartColumn="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID"
			                  AllowAddNew="True" AllowEdit="True" DataSourceID="ds" AutoRefresh="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server"
			                  AutoRefresh="True" DataField="VendorLocationID" DataSourceID="ds" />
			<pxa:PXCurrencyRate ID="edCury" DataField="CuryID" runat="server" RateTypeView="_POLandedCostDoc_CurrencyInfo_"
			                    DataMember="_Currency_" DataSourceID="ds"/>
			<px:PXCheckBox SuppressLabel="True" ID="edCreateBill" runat="server" DataField="CreateBill" CommitChanges="True"/>
			<px:PXTextEdit ID="edVendorRefNbr" runat="server" DataField="VendorRefNbr"  CommitChanges="True"/>
			
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXPanel ID="PXPanel1" runat="server" RenderSimple="True" RenderStyle="Simple">
				<px:PXLayoutRule runat="server" StartColumn="True" />
				<px:PXNumberEdit ID="edCuryAllocatedTotal" runat="server" DataField="CuryAllocatedTotal" Enabled="False" />
				<px:PXNumberEdit ID="edCuryLineTotal" runat="server" CommitChanges="True" DataField="CuryLineTotal" />
				<px:PXNumberEdit ID="edCuryVatTaxableTotal" runat="server" DataField="CuryVatTaxableTotal" Enabled="False" />
				<px:PXNumberEdit ID="edCuryVatExemptTotal" runat="server" DataField="CuryVatExemptTotal" Enabled="False" />
				<px:PXNumberEdit ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" />
				<px:PXNumberEdit ID="edCuryDocTotal" runat="server" DataField="CuryDocTotal" Enabled="False" />
				<px:PXNumberEdit CommitChanges="True" ID="edCuryControlTotal" runat="server" DataField="CuryControlTotal" />
			</px:PXPanel>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="300px" Style="z-index: 100;" Width="100%" DataSourceID="ds" DataMember="CurrentDocument">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="Landed Costs">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100"
					           Width="100%" Height="150px" SkinID="DetailsInTab">
						<Levels>
							<px:PXGridLevel DataMember="Details">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSelector ID="edLandedCostCodeID" runat="server" DataField="LandedCostCodeID" CommitChanges="true" />
									<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
									<px:PXDropDown ID="edAllocationMethod" runat="server" DataField="AllocationMethod" />
									<px:PXNumberEdit ID="edCuryLineAmt" runat="server" DataField="CuryLineAmt" CommitChanges="true" />
									<px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" CommitChanges="true" />
									<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AutoRefresh="True" CommitChanges="true" />
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" CommitChanges="true"/>
									<px:PXDropDown ID="edAPDocType" runat="server" DataField="APDocType" Enabled="False" AllowEdit="True"/>
									<px:PXSelector ID="edAPRefNbr" runat="server" DataField="APRefNbr" Enabled="False" AllowEdit="True"/>
									<px:PXDropDown ID="edINDocType" runat="server" DataField="INDocType" Enabled="False" AllowEdit="True"/>
									<px:PXSelector ID="edINRefNbr" runat="server" DataField="INRefNbr" Enabled="False" AllowEdit="True"/>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="BranchID" AutoCallBack="True" AllowShowHide="Server" DisplayFormat="&gt;AAAAAAAAAA" RenderEditorText="True" />
									<px:PXGridColumn DataField="LineNbr" TextAlign="Right" />
									<px:PXGridColumn DataField="SortOrder" TextAlign="Right" Visible="False" />
									<px:PXGridColumn DataField="LandedCostCodeID" CommitChanges="True"/>
									<px:PXGridColumn DataField="Descr" />
									<px:PXGridColumn DataField="AllocationMethod" />
									<px:PXGridColumn DataField="CuryLineAmt" TextAlign="Right" CommitChanges="True"/>
									<px:PXGridColumn DataField="TaxCategoryID" CommitChanges="True" />
									<px:PXGridColumn DataField="InventoryID" AutoCallBack="True" LinkCommand="ViewItem" CommitChanges="True" />
									<px:PXGridColumn DataField="APDocType" />
									<px:PXGridColumn DataField="APRefNbr" LinkCommand="ViewItem"/>
									<px:PXGridColumn DataField="INDocType" />
									<px:PXGridColumn DataField="INRefNbr" LinkCommand="ViewItem"/>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Window" Enabled="True" MinHeight="150" />
						<Mode InitNewRow="True" AllowFormEdit="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Document Details">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="DetailsInTab" ActionsPosition="Top">
						<Levels>
							<px:PXGridLevel DataMember="ReceiptLines">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSegmentMask SuppressLabel="True" ID="edRlBranchID" runat="server" DataField="BranchID" Enabled="False"/>
									<px:PXSegmentMask SuppressLabel="True" ID="edRlInventoryID" runat="server" DataField="InventoryID" Enabled="False"/>
									<px:PXSegmentMask SuppressLabel="True" ID="edRlSiteID" runat="server" DataField="SiteID" Enabled="False"/>
									<px:PXSelector SuppressLabel="True" ID="edRlUOM" runat="server" DataField="UOM" Enabled="False"/>
									<px:PXNumberEdit SuppressLabel="True" ID="edRlExtWeight" runat="server" DataField="ExtWeight" />
									<px:PXNumberEdit SuppressLabel="True" ID="edRlExtVolume" runat="server" DataField="ExtVolume" />
									<px:PXNumberEdit SuppressLabel="True" ID="edLineAmt" runat="server" DataField="LineAmt" Enabled="False"/>
									<px:PXSelector SuppressLabel="True" ID="edPOReceiptBaseCuryID" runat="server" DataField="POReceiptBaseCuryID" />
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSelector SuppressLabel="True" ID="edRlPOReceiptNbr" runat="server" DataField="POReceiptNbr" Enabled="False"/>
									<px:PXNumberEdit SuppressLabel="True" ID="edRlPOReceiptLineNbr" runat="server" DataField="POReceiptLineNbr" Enabled="False"/>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="BranchID" AutoCallBack="True" AllowShowHide="Server" DisplayFormat="&gt;AAAAAAAAAA" RenderEditorText="True"/>
									<px:PXGridColumn DataField="LineNbr" TextAlign="Right" />
									<px:PXGridColumn DataField="SortOrder" TextAlign="Right" Visible="False" />
									<px:PXGridColumn DataField="InventoryID" AutoCallBack="True" LinkCommand="ViewItem" />
									<px:PXGridColumn DataField="SiteID" />
									<px:PXGridColumn DataField="UOM" />
									<px:PXGridColumn DataField="ReceiptQty" DataType="Decimal" TextAlign="Right" DefValueText="0.0" />
									<px:PXGridColumn AllowNull="False" DataField="ExtWeight" TextAlign="Right" />
									<px:PXGridColumn AllowNull="False" DataField="ExtVolume" TextAlign="Right" />
									<px:PXGridColumn DataField="LineAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="POReceiptBaseCuryID" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryAllocatedLCAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="POReceiptNbr" LinkCommand="ViewItem"/>
									<px:PXGridColumn DataField="POReceiptLineNbr" TextAlign="Right"/>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Window" Enabled="True" MinHeight="150" />
						<ActionBar>
							<Actions>
								<AddNew Enabled="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="Add PO Receipt" Key="cmdAddPOReceipt" CommandSourceID="ds" CommandName="AddPOReceipt" />
								<px:PXToolBarButton Text="Add PO Receipt Line" Key="cmdPOReceiptLine" CommandSourceID="ds" CommandName="AddPOReceiptLine" />
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Tax Details">
				<Template>
					<px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" TabIndex="200" Width="100%" BorderWidth="0px"
                        SkinID="Details">
                        <Levels>
                            <px:PXGridLevel DataMember="Taxes">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector SuppressLabel="True" ID="edTaxID" runat="server" DataField="TaxID" />
                                    <px:PXNumberEdit SuppressLabel="True" ID="edTaxRate" runat="server" DataField="TaxRate" Enabled="False" />
                                    <px:PXNumberEdit SuppressLabel="True" ID="edCuryTaxableAmt" runat="server" DataField="CuryTaxableAmt" />
                                    <px:PXNumberEdit SuppressLabel="True" ID="edCuryTaxAmt" runat="server" DataField="CuryTaxAmt" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="TaxID" AllowUpdate="False" CommitChanges="true" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="TaxRate" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryTaxableAmt" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryTaxAmt" TextAlign="Right" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <ActionBar PagerGroup="3" PagerOrder="2">
                        </ActionBar>
                    </px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Financial Details">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Billing Settings " />
					<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
					<px:PXSelector CommitChanges="True" ID="edTermsID" runat="server" DataField="TermsID" />
					<px:PXDateTimeEdit CommitChanges="True" ID="edBillDate" runat="server" DataField="BillDate" />
					<px:PXDateTimeEdit ID="edDueDate" runat="server" DataField="DueDate" />
					<px:PXDateTimeEdit ID="edDiscDate" runat="server" DataField="DiscDate" />
					<px:PXNumberEdit ID="edCuryDiscAmt" runat="server" DataField="CuryDiscAmt" CommitChanges="True" />
					<px:PXSelector CommitChanges="True" ID="edTaxZoneID" runat="server" DataField="TaxZoneID" Text="ZONE1" />
					<px:PXSegmentMask CommitChanges="True" ID="edPayToVendorID" runat="server" DataField="PayToVendorID" AllowEdit="True" AutoRefresh="True" />
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartGroup="True" GroupCaption="Assign To" LabelsWidth="SM" ControlSize="XM" />
					<px:PXSelector ID="trsWorkgroupID" runat="server" DataField="WorkgroupID" CommitChanges="True" />
					<px:PXSelector ID="edOwnerID" runat="server" DataField="OwnerID" AutoRefresh="true" />
				</Template>
			</px:PXTabItem>
		</Items>
		<CallbackCommands>
			<Search CommitChanges="True" PostData="Page" />
			<Refresh CommitChanges="True" PostData="Page" />
		</CallbackCommands>
		<AutoSize Container="Window" Enabled="True" MinHeight="180" />
	</px:PXTab>
	<px:PXSmartPanel ID="PanelAddPOReceipt" runat="server" Style="z-index: 108; height: 396px;" Width="960px" Caption="Add Receipt"
        CaptionVisible="True" LoadOnDemand="true" Key="poReceiptSelection" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True"
        AutoCallBack-Target="filter" AutoRepaint="True">
        <px:PXFormView ID="filter" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="filter"
            Caption="PO Selection" SkinID="Transparent" CaptionVisible="False">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXDropDown CommitChanges="True" ID="edReceiptType" runat="server" DataField="ReceiptType" SelectedIndex="2" />
                <px:PXSelector CommitChanges="True" ID="edReceiptNbr" runat="server" DataField="ReceiptNbr" />
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
	            <px:PXSelector CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" />
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="grdReceipts" runat="server" Height="304px" Width="100%" DataSourceID="ds" SkinID="Inquire" Style="border-width: 1px 0px;"
            AutoAdjustColumns="true">
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="poReceiptSelection">
                    <Columns>
                        <px:PXGridColumn AllowCheckAll="True" AllowNull="False" AutoCallBack="True" DataField="Selected" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="ReceiptType" />
	                    <px:PXGridColumn DataField="ReceiptNbr" />
                        <px:PXGridColumn DataField="InvoiceNbr" />
	                    <px:PXGridColumn DataField="VendorID" />
	                    <px:PXGridColumn DataField="BranchID" />
	                    <px:PXGridColumn DataField="CuryID" />
	                    <px:PXGridColumn DataField="ReceiptDate" />
	                    <px:PXGridColumn DataField="OrderQty" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel4" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton8" runat="server" Text="Add" SyncVisible="false">
                <AutoCallBack Command="AddPOReceipt2" Target="ds" />
            </px:PXButton>
            <px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="Add & Close" />
            <px:PXButton ID="PXButton4" runat="server" DialogResult="No" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelAddPOReceiptLine" runat="server" Style="z-index: 108; height: 396px;" Width="960px" Caption="Add Receipt Line"
        CaptionVisible="True" LoadOnDemand="true" Key="poReceiptLinesSelection" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True"
        AutoCallBack-Target="filter" AutoRepaint="True">
        <px:PXFormView ID="PXFormView1" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="filter"
            Caption="PO Selection" SkinID="Transparent" CaptionVisible="False">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXDropDown CommitChanges="True" ID="edReceiptType" runat="server" DataField="ReceiptType" />
                <px:PXSelector CommitChanges="True" ID="edReceiptNbr" runat="server" DataField="ReceiptNbr" />
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXSelector CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" />
                <px:PXSelector CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" />
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="PXGrid1" runat="server" Height="304px" Width="100%" DataSourceID="ds" SkinID="Inquire" Style="border-width: 1px 0px;"
            AutoAdjustColumns="true">
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="poReceiptLinesSelection">
                    <Columns>
                        <px:PXGridColumn AllowCheckAll="True" AllowNull="False" AutoCallBack="True" DataField="Selected" TextAlign="Center" Type="CheckBox" />
	                    <px:PXGridColumn DataField="PONbr" />
	                    <px:PXGridColumn DataField="POType" />
                        <px:PXGridColumn DataField="ReceiptNbr" />
                        <px:PXGridColumn DataField="InvoiceNbr" />
	                    <px:PXGridColumn DataField="VendorID" />
	                    <px:PXGridColumn DataField="InventoryID" />
	                    <px:PXGridColumn DataField="SiteID" />
	                    <px:PXGridColumn DataField="ReceiptQty" />
	                    <px:PXGridColumn DataField="UOM" />
	                    <px:PXGridColumn DataField="TranCostFinal" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server" Text="Add" SyncVisible="false">
                <AutoCallBack Command="AddPOReceiptLine2" Target="ds" />
            </px:PXButton>
            <px:PXButton ID="PXButton2" runat="server" DialogResult="OK" Text="Add & Close" />
            <px:PXButton ID="PXButton5" runat="server" DialogResult="No" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
