<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM304000.aspx.cs"
    Inherits="Page_PM304000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.RegisterEntry" PrimaryView="Document"
        PageLoadBehavior="GoLastRecord">
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
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" DataSourceID="ds" />
            <px:PXDropDown ID="edStatus" runat="server"  DataField="Status" Enabled="False" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXDropDown ID="edOrigDocType" runat="server" DataField="OrigDocType" Enabled="False" />
            <px:PXTextEdit ID="edOrigDocNbr" runat="server" DataField="OrigDocNbr" Enabled="False" />
							
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
            <px:PXNumberEdit ID="edQtyTotal" runat="server" DataField="QtyTotal" Enabled="False" />
            <px:PXNumberEdit ID="edBillableQtyTotal" runat="server" DataField="BillableQtyTotal" Enabled="False" />
            <px:PXNumberEdit ID="edAmtTotal" runat="server" DataField="AmtTotal" Enabled="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Details" Caption="Transaction Details"
               SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="Transactions">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                    <px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" AutoRefresh="True" />
                    <px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" CommitChanges="True" />
                    <px:PXSelector Size="s" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" AutoRefresh="True" />
                    <px:PXSegmentMask Size="xs" ID="edTaskID" runat="server" DataField="TaskID" AutoRefresh="True" CommitChanges="True" />
                    <px:PXSelector Size="s" ID="edBatchNbr" runat="server" DataField="BatchNbr" AllowEdit="True" />
                    <px:PXSegmentMask ID="edCostCode" runat="server" DataField="CostCodeID" AutoRefresh="True"/>
                    <px:PXSegmentMask ID="edAccountGroupID" runat="server" DataField="AccountGroupID" />
                    <px:PXCheckBox ID="chkBilled" runat="server" DataField="Billed" />
                    <px:PXSegmentMask ID="edResourceID" runat="server" DataField="ResourceID" />
                    <px:PXSelector ID="edBAccountID" runat="server" DataField="BAccountID" />
                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" />
                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" />
                    <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="S" StartColumn="True" />
                    <px:PXNumberEdit Size="xs" ID="edQty" runat="server" DataField="Qty" CommitChanges="True"/>
                    <px:PXCheckBox ID="chkAllocated" runat="server" DataField="Allocated" />
                    <px:PXCheckBox ID="chkBillable" runat="server" Checked="True" DataField="Billable" />
                    <px:PXCheckBox ID="chkReleased" runat="server" DataField="Released" />
                    <px:PXNumberEdit ID="edBillableQty" runat="server" DataField="BillableQty" CommitChanges="True"/>
                    <px:PXNumberEdit ID="edTranCuryUnitRate" runat="server" DataField="TranCuryUnitRate" CommitChanges="True"/>
                    <px:PXNumberEdit ID="edTranCuryAmount" runat="server" DataField="TranCuryAmount" />
                    <px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" AutoRefresh="True"/>
                    <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" />
                    <px:PXSegmentMask ID="edOffsetAccountID" runat="server" DataField="OffsetAccountID" AutoRefresh="True"/>
                    <px:PXSegmentMask ID="edOffsetSubID" runat="server" DataField="OffsetSubID" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="BranchID" Label="Branch" Width="81px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="ProjectID" Label="Project" Width="108px" AutoCallBack="true" LinkCommand="ViewProject" />
                    <px:PXGridColumn DataField="TaskID" Label="Task" Width="108px" LinkCommand="ViewTask" AutoCallBack="True"/>
                    <px:PXGridColumn DataField="CostCodeID" Width="108px" AutoCallBack="true" />
                    <px:PXGridColumn DataField="AccountGroupID" Label="Account Group" Width="108px" AutoCallBack="true" />
                    <px:PXGridColumn DataField="ResourceID" Label="Resource" Width="108px" AutoCallBack="true" />
                    <px:PXGridColumn DataField="BAccountID" Label="Customer/Vendor" Width="108px" AutoCallBack="true" />
                    <px:PXGridColumn DataField="LocationID" Label="Location" Width="108px" />
                    <px:PXGridColumn DataField="InventoryID" Label="InventoryID" Width="108px" AutoCallBack="true" LinkCommand="ViewInventory"/>
                    <px:PXGridColumn DataField="Description" Label="Description" Width="108px" />
                    <px:PXGridColumn DataField="UOM" Label="UOM" Width="63px" />
                    <px:PXGridColumn DataField="Qty" Label="Qty" TextAlign="Right" Width="63px" AutoCallBack="true" />
                    <px:PXGridColumn DataField="Billable" Label="Billable" TextAlign="Center" Type="CheckBox" Width="63px" />
                    <px:PXGridColumn DataField="BillableQty" Label="BillableQty" TextAlign="Right" Width="63px" AutoCallBack="true" />
                    <px:PXGridColumn DataField="TranCuryUnitRate" Label="TranCuryUnitRate" TextAlign="Right" Width="63px" AutoCallBack="true"/>

                    <px:PXGridColumn DataField="TranCuryAmount" Label="TranCuryAmount" TextAlign="Right" Width="80px"  AutoCallBack="true" />
					<px:PXGridColumn DataField="TranCuryId" Label="Currency" Width="70px" AutoCallBack="true" />
					<px:PXGridColumn DataField="BaseCuryRate" Label="Base Currency Rate" TextAlign="Right" Width="70px" />

					<px:PXGridColumn DataField="ProjectCuryAmount" Label="Project Transaction Amount" TextAlign="Right" Width="80px" />
					<px:PXGridColumn DataField="ProjectCuryId" Label="Project Currency" Width="70px" />
					<px:PXGridColumn DataField="ProjectCuryRate" Label="Project Currency Rate" TextAlign="Right" Width="70px" />

					<px:PXGridColumn DataField="StartDate" Width="90px" Visible="false" />
                    <px:PXGridColumn DataField="EndDate" Width="90px" Visible="false" />
                    <px:PXGridColumn DataField="AccountID" Label="Account" Width="108px" AutoCallBack="true" />
                    <px:PXGridColumn DataField="SubID" Label="Subaccount" Width="108px" />
                    <px:PXGridColumn DataField="OffsetAccountID" Label="Offset Account" Width="108px" AutoCallBack="true" />
                    <px:PXGridColumn DataField="OffsetSubID" Label="Offset SubAccount" Width="108px" />
                    <px:PXGridColumn DataField="Date" AutoCallBack="true" />
                    <px:PXGridColumn DataField="FinPeriodID" Width="108px" AutoCallBack="true"/>
                    <px:PXGridColumn DataField="BatchNbr" Width="108px" />
                    <px:PXGridColumn DataField="EarningType" Width="100px" AutoCallBack="true" />
                    <px:PXGridColumn DataField="OvertimeMultiplier" Width="70px" />
                    <px:PXGridColumn DataField="UseBillableQty" Label="UseBillableQty" TextAlign="Center" Type="CheckBox"  Width="140px" />
                    <px:PXGridColumn DataField="Allocated" Label="Allocated" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="Released" Label="Released" TextAlign="Center" Type="CheckBox" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar>
            <CustomItems>
                <px:PXToolBarButton Text="Transaction" Key="cmdViewAllocationSorce">
                    <AutoCallBack Command="ViewAllocationSorce" Target="ds" />
                </px:PXToolBarButton>
				<px:PXToolBarButton Text="Select project currency rate">
					<AutoCallBack Command="SelectProjectRate" Target="ds">
						<Behavior CommitChanges="True" />
					</AutoCallBack>
				</px:PXToolBarButton>
				<px:PXToolBarButton Text="Select base currency rate">
					<AutoCallBack Command="SelectBaseRate" Target="ds">
						<Behavior CommitChanges="True" />
					</AutoCallBack>
				</px:PXToolBarButton>
                <px:PXToolBarButton>
					<AutoCallBack Command="CuryToggle" Target="ds">
						<Behavior CommitChanges="True" />
					</AutoCallBack>
				</px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
        <Mode InitNewRow="True" AllowUpload="true"></Mode>
    </px:PXGrid>
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
