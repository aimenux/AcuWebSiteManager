<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM304000.aspx.cs"
    Inherits="Page_PM304000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="True" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.RegisterEntry" PrimaryView="Document">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Release" CommitChanges="true" StartNewGroup="True"/>
            <px:PXDSCallbackCommand Name="CuryToggle" Visible="False"/>
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewAllocationSorce" Visible="False"/>
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewProject" Visible="False"/>
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewTask" Visible="False"/>
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewInventory" Visible="False"/>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="SelectProjectRate" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="SelectBaseRate" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewCustomer" Visible="False"/>
            <px:PXDSCallbackCommand Name="ComplianceDocument$PurchaseOrder$Link" Visible="false" DependOnGrid="grid" CommitChanges="True" />
	        <px:PXDSCallbackCommand CommitChanges="True" Name="ComplianceDocument$Subcontract$Link" Visible="false" DependOnGrid="grid" />
	        <px:PXDSCallbackCommand Name="ComplianceDocument$InvoiceID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
	        <px:PXDSCallbackCommand Name="ComplianceDocument$BillID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
	        <px:PXDSCallbackCommand Name="ComplianceDocument$ApCheckID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
	        <px:PXDSCallbackCommand Name="ComplianceDocument$ArPaymentID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
	        <px:PXDSCallbackCommand Name="ComplianceDocument$ProjectTransactionID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Document" DefaultControlID="edModule"
        Caption="Transaction Summary">
        <Parameters>
            <px:PXQueryStringParam Name="PMRegister.module" QueryStringField="Module" Type="String" OnLoadOnly="True" />
        </Parameters>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXDropDown ID="edModule" runat="server"  DataField="Module" SelectedIndex="-1" />
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" DataSourceID="ds">
                <GridProperties FastFilterFields="RefNbr, Description"/>
            </px:PXSelector>
            <px:PXDropDown ID="edStatus" runat="server"  DataField="Status" Enabled="False" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXDropDown ID="edOrigDocType" runat="server" DataField="OrigDocType" Enabled="False" />
			<px:PXTextEdit ID="edOrigNoteID" runat="server" DataField="OrigNoteID" Enabled="False" />
							
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
            <px:PXNumberEdit ID="edQtyTotal" runat="server" DataField="QtyTotal" Enabled="False" />
            <px:PXNumberEdit ID="edBillableQtyTotal" runat="server" DataField="BillableQtyTotal" Enabled="False" />
            <px:PXNumberEdit ID="edAmtTotal" runat="server" DataField="AmtTotal" Enabled="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab runat="server" ID="tab" Height="300px" Width="100%" DataSourceID="ds">
        <Items>
            <px:PXTabItem Text="Document Details">
                <Template>
                    <px:PXGrid runat="server" ID="grid" SyncPosition="true" Height="150px" SkinID="Details" Width="100%" Caption="Transaction Details" DataSourceID="ds">
                        <AutoSize Enabled="True" Container="Window" MinHeight="150" />
                        <Mode InitNewRow="True" AllowUpload="true" />
                        <Levels>
                            <px:PXGridLevel DataMember="Transactions">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                                    <px:PXSegmentMask runat="server" ID="edProjectID" DataField="ProjectID" AutoRefresh="True" CommitChanges="True" />
                                    <px:PXSegmentMask runat="server" ID="edBranchID" DataField="BranchID" CommitChanges="True" />
                                    <px:PXSelector runat="server" ID="edFinPeriodID" DataField="FinPeriodID" AutoRefresh="True" Size="s" />
                                    <px:PXSegmentMask runat="server" ID="edTaskID" DataField="TaskID" AutoRefresh="True" Size="xs" CommitChanges="True" />
                                    <px:PXSelector runat="server" ID="edBatchNbr" DataField="BatchNbr" AllowEdit="True" Size="s" />
                                    <px:PXSegmentMask runat="server" ID="edCostCode" DataField="CostCodeID" AutoRefresh="True" />
                                    <px:PXSegmentMask runat="server" ID="edAccountGroupID" DataField="AccountGroupID" />
                                    <px:PXCheckBox runat="server" DataField="Billed" ID="chkBilled" />
                                    <px:PXSegmentMask runat="server" ID="edResourceID" DataField="ResourceID" />
                                    <px:PXSelector runat="server" ID="edBAccountID" DataField="BAccountID" />
                                    <px:PXSegmentMask runat="server" ID="edLocationID" DataField="LocationID" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" />
                                    <px:PXTextEdit runat="server" DataField="Description" ID="edDescription" />
                                    <px:PXSelector runat="server" ID="edUOM" DataField="UOM" />
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                                    <px:PXNumberEdit runat="server" ID="edQty" Size="xs" CommitChanges="True" DataField="Qty" />
                                    <px:PXCheckBox runat="server" DataField="Allocated" ID="chkAllocated" />
                                    <px:PXCheckBox runat="server" Checked="True" DataField="Billable" ID="chkBillable" />
                                    <px:PXCheckBox runat="server" DataField="Released" ID="chkReleased" />
                                    <px:PXNumberEdit runat="server" ID="edBillableQty" CommitChanges="True" DataField="BillableQty" />
                                    <px:PXNumberEdit runat="server" ID="edTranCuryUnitRate" CommitChanges="True" DataField="TranCuryUnitRate" />
                                    <px:PXNumberEdit runat="server" ID="edTranCuryAmount" DataField="TranCuryAmount" />
                                    <px:PXSegmentMask runat="server" ID="edAccountID" DataField="AccountID" AutoRefresh="True" />
                                    <px:PXSegmentMask runat="server" ID="edSubID" DataField="SubID" />
                                    <px:PXSegmentMask runat="server" ID="edOffsetAccountID" DataField="OffsetAccountID" AutoRefresh="True" />
                                    <px:PXSegmentMask runat="server" ID="edOffsetSubID" DataField="OffsetSubID" />
                                </RowTemplate>

                                <Columns>
                                    <px:PXGridColumn DataField="BranchID" AutoCallBack="True" Label="Branch" />
                                    <px:PXGridColumn DataField="ProjectID" AutoCallBack="true" LinkCommand="ViewProject" Label="Project" />
                                    <px:PXGridColumn DataField="DirectCostType" Type="DropDownList" Visible="False" />
                                    <px:PXGridColumn DataField="TaskID" AutoCallBack="True" LinkCommand="ViewTask" Label="Task" />
                                    <px:PXGridColumn DataField="CostCodeID" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="AccountGroupID" AutoCallBack="true" Label="Account Group" />
                                    <px:PXGridColumn DataField="ResourceID" AutoCallBack="true" Label="Resource" />
                                    <px:PXGridColumn DataField="BAccountID" AutoCallBack="true" LinkCommand="ViewCustomer" Label="Customer/Vendor" />
                                    <px:PXGridColumn DataField="LocationID" Label="Location" />
                                    <px:PXGridColumn DataField="InventoryID" AutoCallBack="true" LinkCommand="ViewInventory" Label="InventoryID" />
                                    <px:PXGridColumn DataField="Description" Label="Description" />
                                    <px:PXGridColumn DataField="UOM" Label="UOM" />
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" AutoCallBack="true" Label="Qty" />
                                    <px:PXGridColumn DataField="Billable" Type="CheckBox" TextAlign="Center" Label="Billable" />
                                    <px:PXGridColumn DataField="BillableQty" TextAlign="Right" AutoCallBack="true" Label="BillableQty" />
                                    <px:PXGridColumn DataField="TranCuryUnitRate" TextAlign="Right" AutoCallBack="true" Label="TranCuryUnitRate" />

                                    <px:PXGridColumn DataField="TranCuryAmount" TextAlign="Right" AutoCallBack="true" Label="TranCuryAmount" />
                                    <px:PXGridColumn DataField="TranCuryId" AutoCallBack="true" Label="Currency" />
                                    <px:PXGridColumn DataField="BaseCuryRate" TextAlign="Right" Label="Base Currency Rate" />

                                    <px:PXGridColumn DataField="ProjectCuryAmount" TextAlign="Right" Label="Project Transaction Amount" />
                                    <px:PXGridColumn DataField="ProjectCuryId" Label="Project Currency" />
                                    <px:PXGridColumn DataField="ProjectCuryRate" TextAlign="Right" Label="Project Currency Rate" />

                                    <px:PXGridColumn DataField="StartDate" Visible="false" />
                                    <px:PXGridColumn DataField="EndDate" Visible="false" />
                                    <px:PXGridColumn DataField="AccountID" AutoCallBack="true" Label="Account" />
                                    <px:PXGridColumn DataField="SubID" Label="Subaccount" />
                                    <px:PXGridColumn DataField="OffsetAccountID" AutoCallBack="true" Label="Offset Account" />
                                    <px:PXGridColumn DataField="OffsetSubID" Label="Offset SubAccount" />
                                    <px:PXGridColumn DataField="Date" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="FinPeriodID" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="BatchNbr" />
                                    <px:PXGridColumn DataField="EarningType" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="OvertimeMultiplier" />
                                    <px:PXGridColumn DataField="UseBillableQty" Type="CheckBox" TextAlign="Center" Label="UseBillableQty" />
                                    <px:PXGridColumn DataField="Allocated" Type="CheckBox" TextAlign="Center" Label="Allocated" />
                                    <px:PXGridColumn DataField="Released" Type="CheckBox" TextAlign="Center" Label="Released" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>




                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdViewAllocationSorce" Text="Transaction">
                                    <AutoCallBack Target="ds" Command="ViewAllocationSorce" />
                                </px:PXToolBarButton>

                                <px:PXToolBarButton Text="Select project currency rate">
                                    <AutoCallBack Target="ds" Command="SelectProjectRate">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>


                                <px:PXToolBarButton Text="Select base currency rate">
                                    <AutoCallBack Target="ds" Command="SelectBaseRate">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton>
                                    <AutoCallBack Target="ds" Command="CuryToggle">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Compliance">
                <Template>
                    <px:PXGrid runat="server" ID="grdComplianceDocuments" AllowPaging="True" Height="300px" SkinID="DetailsInTab" Width="100%" AutoGenerateColumns="Append" DataSourceID="ds" KeepPosition="True" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="ComplianceDocuments">
                                <Columns>
                                    <px:PXGridColumn DataField="ExpirationDate" CommitChanges="True" TextAlign="Left" />
                                    <px:PXGridColumn DataField="DocumentType" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CreationDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="Status" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Required" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="Received" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="ReceivedDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="IsProcessed" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="IsVoided" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="IsCreatedAutomatically" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="SentDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ProjectID" CommitChanges="True" LinkCommand="ComplianceDocuments_Project_ViewDetails" />
                                    <px:PXGridColumn DataField="CostTaskID" TextAlign="Left" LinkCommand="ComplianceDocuments_Task_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="RevenueTaskID" TextAlign="Left" LinkCommand="ComplianceDocuments_Task_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CostCodeID" TextAlign="Left" LinkCommand="ComplianceDocuments_CostCode_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CustomerID" CommitChanges="True" LinkCommand="ComplianceDocuments_Customer_ViewDetails" />
                                    <px:PXGridColumn DataField="CustomerName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="VendorID" CommitChanges="True" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" />
                                    <px:PXGridColumn DataField="VendorName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="AccountID" TextAlign="Left" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ProjectTransactionID" LinkCommand="ComplianceDocument$ProjectTransactionID$Link" CommitChanges="True" DisplayMode="Text" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ApCheckID" LinkCommand="ComplianceDocument$ApCheckID$Link" DisplayMode="Text" CommitChanges="True" TextAlign="Left" />
                                    <px:PXGridColumn DataField="CheckNumber" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ArPaymentID" LinkCommand="ComplianceDocument$ArPaymentID$Link" DisplayMode="Text" CommitChanges="True" TextAlign="Left" />
                                    <px:PXGridColumn DataField="BillAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="BillID" LinkCommand="ComplianceDocument$BillID$Link" CommitChanges="True" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="CertificateNumber" TextAlign="Left" />
                                    <px:PXGridColumn DataField="CreatedByID" />
                                    <px:PXGridColumn DataField="DateIssued" TextAlign="Left" />
                                    <px:PXGridColumn DataField="DocumentTypeValue" CommitChanges="True" />
                                    <px:PXGridColumn DataField="EffectiveDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="InsuranceCompany" TextAlign="Left" />
                                    <px:PXGridColumn DataField="InvoiceAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="InvoiceID" LinkCommand="ComplianceDocument$InvoiceID$Link" CommitChanges="True" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="IsExpired" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="IsRequiredJointCheck" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="JointAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="JointRelease" TextAlign="Left" />
                                    <px:PXGridColumn DataField="JointReleaseReceived" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="JointVendorInternalId" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" TextAlign="Left" />
                                    <px:PXGridColumn DataField="JointVendorExternalName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="LastModifiedByID" />
                                    <px:PXGridColumn DataField="LienWaiverAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="Limit" TextAlign="Right" />
                                    <px:PXGridColumn DataField="MethodSent" TextAlign="Left" />
                                    <px:PXGridColumn DataField="PaymentDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="Policy" TextAlign="Left" />
                                    <px:PXGridColumn DataField="PurchaseOrder" TextAlign="Left" DisplayMode="Text" CommitChanges="True" LinkCommand="ComplianceDocument$PurchaseOrder$Link" />
                                    <px:PXGridColumn DataField="PurchaseOrderLineItem" TextAlign="Left" />
                                    <px:PXGridColumn DataField="Subcontract" DisplayMode="Text" CommitChanges="True" LinkCommand="ComplianceDocument$Subcontract$Link" />
                                    <px:PXGridColumn DataField="SubcontractLineItem" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ChangeOrderNumber" DisplayMode="Text" LinkCommand="ComplianceDocument$ChangeOrderNumber$Link" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ReceiptDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ReceiveDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ReceivedBy" TextAlign="Left" />
                                    <px:PXGridColumn DataField="SecondaryVendorID" CommitChanges="True" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" />
                                    <px:PXGridColumn DataField="SecondaryVendorName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="SourceType" TextAlign="Left" />
                                    <px:PXGridColumn DataField="SponsorOrganization" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ThroughDate" TextAlign="Left" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSegmentMask runat="server" DataField="CostCodeID" AutoRefresh="True" ID="edCostCode2" />
                                    <px:PXSelector runat="server" DataField="DocumentTypeValue" AutoRefresh="True" ID="edDocumentTypeValue" />
                                    <px:PXSelector runat="server" DataField="BillID" FilterByAllFields="True" AutoRefresh="True" ID="edBillID" />
                                    <px:PXSelector runat="server" DataField="InvoiceID" FilterByAllFields="True" AutoRefresh="True" ID="edInvoiceID" />
                                    <px:PXSelector runat="server" DataField="ApCheckID" FilterByAllFields="True" AutoRefresh="True" ID="edApCheckID" />
                                    <px:PXSelector runat="server" DataField="ArPaymentID" FilterByAllFields="True" AutoRefresh="True" ID="edArPaymentID" />
                                    <px:PXSelector runat="server" DataField="ProjectTransactionID" FilterByAllFields="True" AutoRefresh="True" ID="edProjectTransactionID" />
                                    <px:PXSelector runat="server" DataField="PurchaseOrder" FilterByAllFields="True" AutoRefresh="True" ID="edPurchaseOrder" CommitChanges="True" />
                                    <px:PXSelector runat="server" DataField="PurchaseOrderLineItem" AutoRefresh="True" ID="edPurchaseOrderLineItem" />
                                    <px:PXSelector runat="server" DataField="Subcontract" FilterByAllFields="True" AutoRefresh="True" CommitChanges="True" ID="edSubcontract" />
                                    <px:PXSelector runat="server" DataField="SubcontractLineItem" AutoRefresh="True" ID="edSubcontractLineItem" />
                                    <px:PXSelector runat="server" DataField="ChangeOrderNumber" AutoRefresh="True" ID="edChangeOrderNumber" />
                                    <px:PXSelector runat="server" DataField="CostTaskID" FilterByAllFields="True" AutoRefresh="True" ID="edCostTaskID" />
                                    <px:PXSelector runat="server" DataField="RevenueTaskID" FilterByAllFields="True" AutoRefresh="True" ID="edRevenueTaskID" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode InitNewRow="True" />
                        <CallbackCommands>
                            <InitRow CommitChanges="True" />
                        </CallbackCommands>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
                <AutoCallBack>
                    <Behavior CommitChanges="True" />
                </AutoCallBack>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="300" MinWidth="300" />
    </px:PXTab>
</asp:Content>
<asp:Content ID="Dialogs" ContentPlaceHolderID="phDialogs" runat="Server">
	<px:PXSmartPanel ID="SelectProjectRatePanel" runat="server" Height="200px" Width="500px" Caption="Select project currency rate" CaptionVisible="True" Key="ProjectCuryInfo" AutoCallBack-Command="Refresh"
        AutoCallBack-Enabled="True" LoadOnDemand="true" DesignView="Content" AllowResize="false" AutoRepaint="true">
			<px:PXFormView ID="rf" runat="server" DataMember="ProjectCuryInfo" SkinID="Transparent">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S"/>
					<px:PXSelector ID="edProjectRateType" runat="server" DataField="CuryRateTypeID" CommitChanges="true"/>
					<px:PXDateTimeEdit ID="edProjectEffDate" runat="server" DataField="CuryEffDate" CommitChanges="true"/>
					<px:PXPanel ID="pnProjectRate" runat="server" Caption="Currency Unit Equivalents" RenderStyle="Fieldset">
						<px:PXLabel ID="PXLabel3" runat="server" Text="1.000" />
						<px:PXLabel ID="PXLabel4" runat="server" Text="1.000" />
						<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" />
						<px:PXTextEdit ID="PXTextEdit3" runat="server" DataField="DisplayCuryID" Width="50px" SuppressLabel="true" />
						<px:PXTextEdit ID="PXTextEdit4" runat="server" DataField="BaseCuryID" Width="50px" SuppressLabel="true" />
						<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="XS" />
						<px:PXLabel ID="PXLabel1" runat="server" Text="=" />
						<px:PXLabel ID="PXLabel2" runat="server" Text="=" />
						<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" />
						<px:PXNumberEdit ID="PXNumberEdit1" runat="server" DataField="SampleCuryRate" SuppressLabel="true" CommitChanges="true" />
						<px:PXNumberEdit ID="PXNumberEdit2" runat="server" DataField="SampleRecipRate" SuppressLabel="true" CommitChanges="true" />
						<px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" />
						<px:PXTextEdit ID="PXTextEdit5" runat="server" DataField="BaseCuryID" Width="50px" SuppressLabel="true" />
						<px:PXTextEdit ID="PXTextEdit6" runat="server" DataField="DisplayCuryID" Width="50px" SuppressLabel="true" />
					</px:PXPanel>
				</Template>
			</px:PXFormView>
			<px:PXPanel ID="pnlChangeIDButton" runat="server" SkinID="Buttons" Width="470px">
				<px:PXButton ID="btnOk" runat="server" DialogResult="OK" Text="OK">
					<AutoCallBack Target="rf" Command="Save" />
				</px:PXButton>
			</px:PXPanel>
    </px:PXSmartPanel>
	<px:PXSmartPanel ID="SelectBaseRatePanel" runat="server" Height="200px" Width="500px" Caption="Select base currency rate" CaptionVisible="True" Key="BaseCuryInfo" AutoCallBack-Command="Refresh"
        AutoCallBack-Enabled="True" LoadOnDemand="true" DesignView="Content" AllowResize="false" AutoRepaint="true">
			<px:PXFormView ID="PXFormView1" runat="server" DataMember="BaseCuryInfo" SkinID="Transparent">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S"/>
					<px:PXSelector ID="edProjectRateType" runat="server" DataField="CuryRateTypeID" CommitChanges="true"/>
					<px:PXDateTimeEdit ID="edProjectEffDate" runat="server" DataField="CuryEffDate" CommitChanges="true"/>
					<px:PXPanel ID="pnProjectRate" runat="server" Caption="Currency Unit Equivalents" RenderStyle="Fieldset">
						<px:PXLabel ID="PXLabel3" runat="server" Text="1.000" />
						<px:PXLabel ID="PXLabel4" runat="server" Text="1.000" />
						<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" />
						<px:PXTextEdit ID="PXTextEdit3" runat="server" DataField="DisplayCuryID" Width="50px" SuppressLabel="true" />
						<px:PXTextEdit ID="PXTextEdit4" runat="server" DataField="BaseCuryID" Width="50px" SuppressLabel="true" />
						<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="XS" />
						<px:PXLabel ID="PXLabel1" runat="server" Text="=" />
						<px:PXLabel ID="PXLabel2" runat="server" Text="=" />
						<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" />
						<px:PXNumberEdit ID="PXNumberEdit1" runat="server" DataField="SampleCuryRate" SuppressLabel="true" CommitChanges="true" />
						<px:PXNumberEdit ID="PXNumberEdit2" runat="server" DataField="SampleRecipRate" SuppressLabel="true" CommitChanges="true" />
						<px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" />
						<px:PXTextEdit ID="PXTextEdit5" runat="server" DataField="BaseCuryID" Width="50px" SuppressLabel="true" />
						<px:PXTextEdit ID="PXTextEdit6" runat="server" DataField="DisplayCuryID" Width="50px" SuppressLabel="true" />
					</px:PXPanel>
				</Template>
			</px:PXFormView>
			<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons" Width="470px">
				<px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="OK">
					<AutoCallBack Target="rf" Command="Save" />
				</px:PXButton>
			</px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
