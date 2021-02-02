<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="RQ302000.aspx.cs" Inherits="Page_RQ302000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" TypeName="PX.Objects.RQ.RQRequisitionEntry" PrimaryView="Document" BorderStyle="NotSet" Width="100%">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="addRequestLine" Visible="false" />
            <px:PXDSCallbackCommand Name="addRequestContent" Visible="false" />
            <px:PXDSCallbackCommand Name="viewDetails" DependOnGrid="grid" Visible="false" />
            <px:PXDSCallbackCommand Name="viewRequest" DependOnGrid="gridContent" Visible="false" />
            <px:PXDSCallbackCommand Name="viewPOOrder" DependOnGrid="gridPOOrders" Visible="false" />
            <px:PXDSCallbackCommand Name="viewSOOrder" DependOnGrid="gridSOOrders" Visible="false" />
            <px:PXDSCallbackCommand Name="viewOrderByLine" DependOnGrid="gridOrderLines" Visible="false" />
            <px:PXDSCallbackCommand Name="CreatePOOrder" Visible="false" />
            <px:PXDSCallbackCommand Name="CreateQTOrder" Visible="false" />
            <px:PXDSCallbackCommand Name="viewLineDetails" DependOnGrid="grid" Visible="false" />
            <px:PXDSCallbackCommand Name="chooseVendor" DependOnGrid="gridVendor" Visible="false" />
            <px:PXDSCallbackCommand Name="responseVendor" DependOnGrid="gridVendor" Visible="false" />
            <px:PXDSCallbackCommand Name="vendorInfo" DependOnGrid="gridVendor" Visible="false" CommitChanges="true" CommitChangesIDs="formVC, formVA" />
            <px:PXDSCallbackCommand Visible="false" Name="Hold" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Action" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Action@Complete Bidding"  CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" PopupVisible="true"/>
            <px:PXDSCallbackCommand Name="VendorNotifications@Send Request" DependOnGrid="gridVendor" Visible="false" />
            <px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
            <px:PXDSCallbackCommand Name="AddInvBySite" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="AddInvSelBySite" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="merge" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="ValidateAddresses" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="RecalculatePricesAction" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="RecalculatePricesActionOk" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
    <px:PXSmartPanel ID="pnlAddRequest" runat="server" Key="SourceRequests" DesignView="Content" CaptionVisible="true" Caption="Select Requested Items" Width="900px"
        Height="470px" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" AutoCallBack-Target="formRequest" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        LoadOnDemand="true" ShowAfterLoad="true">
        <px:PXFormView ID="formRequest" runat="server" DataSourceID="ds" DataMember="RequestFilter" Width="100%" CaptionVisible="False" BorderWidth="0px" SkinID="Transparent">
            <Activity Height="" HighlightColor="" SelectedColor="" Width="" />
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                <px:PXLayoutRule runat="server" Merge="True" />
                <px:PXSelector Size="m" CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID" />
                <px:PXCheckBox CommitChanges="True" ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner" />
                <px:PXLayoutRule runat="server" Merge="False" />
                <px:PXLayoutRule runat="server" Merge="True" />
                <px:PXSelector CommitChanges="True" Size="m" ID="edWorkGroupID" runat="server" DataField="WorkGroupID" />
                <px:PXCheckBox CommitChanges="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />
                <px:PXLayoutRule runat="server" Merge="False" />
                <px:PXCheckBox CommitChanges="True" ID="chkMyEscalated" runat="server" DataField="MyEscalated" />
                <px:PXCheckBox CommitChanges="True" ID="chkAddExists" runat="server" Checked="True" DataField="AddExists" />
                <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" AllowEdit="True" DataField="InventoryID" />
                <px:PXSegmentMask CommitChanges="True" Size="xs" ID="edSubItemID" runat="server" AutoRefresh="true" DataField="SubItemID" />                
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                 <px:PXSelector CommitChanges="True" ID="edReqClassID" runat="server" DataField="ReqClassID" />
                <px:PXDropDown CommitChanges="True" ID="edSelectedPriority" runat="server" AllowNull="False" DataField="SelectedPriority" />
                <px:PXSegmentMask CommitChanges="True" ID="edRequesterID" runat="server" AutoRefresh="true" DataField="EmployeeID" ValueField="AcctCD" />
                <px:PXSelector CommitChanges="True" ID="edDepartmentID" runat="server" DataField="DepartmentID" />
                <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" />
                <px:PXTextEdit CommitChanges="True" ID="edDescription" runat="server" DataField="Description" /></Template>
        </px:PXFormView>
        <px:PXGrid ID="addRequest" runat="server" DataSourceID="ds" Width="100%" Style="border-width: 1px 0px; top: 0px; left: 0px;" AdjustPageSize="Auto" AllowPaging="true" SkinID="Inquire">
            <Mode AllowAddNew="false" AllowDelete="false" />
            <Levels>
                <px:PXGridLevel DataMember="SourceRequests">
                    <Columns>
                        <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AutoCallBack="true" />
                        <px:PXGridColumn AllowNull="False" DataField="RQRequest__Priority" RenderEditorText="True" />
                        <px:PXGridColumn DataField="RQRequest__ReqClassID" DisplayFormat="&gt;aaaaaaaaaa" />
                        <px:PXGridColumn DataField="RQRequest__OrderNbr" />
                        <px:PXGridColumn DataField="RQRequest__EmployeeID" AllowUpdate="false" />
                        <px:PXGridColumn DataField="LineNbr" TextAlign="Right" Visible="False" />
                        <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" />
                        <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;A" />
                        <px:PXGridColumn DataField="Description" />
                        <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" />
                        <px:PXGridColumn AllowNull="False" DataField="SelectQty" TextAlign="Right" AutoCallBack="true" />
                        <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="OpenQty" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ReqQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="VendorID" DisplayFormat="&gt;AAAAAAAAAA" />
                        <px:PXGridColumn DataField="VendorLocationID" DisplayFormat="&gt;AAAAAA" />
                        <px:PXGridColumn DataField="VendorName" />
                        <px:PXGridColumn DataField="VendorRefNbr" />
                        <px:PXGridColumn DataField="VendorDescription" />
                        <px:PXGridColumn DataField="AlternateID" />
                        <px:PXGridColumn DataField="RequestedDate" />
                        <px:PXGridColumn DataField="PromisedDate" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="true" />
        </px:PXGrid>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnSave0" runat="server" DialogResult="OK" Text="Save" />
            <px:PXButton ID="btnCancel0" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="pnlContent" runat="server" Key="Contents" DesignView="Content" CaptionVisible="true" Caption="Request Details" Width="900px" Height="400px"
        AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" AutoCallBack-Target="gridContent" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" LoadOnDemand="true"
        ShowAfterLoad="true">
        <px:PXGrid ID="gridContent" runat="server" DataSourceID="ds" Width="100%" BorderWidth="0px" SkinID="Details">
            <Mode AllowAddNew="false" />
            <Levels>
                <px:PXGridLevel DataMember="Contents">
                    <RowTemplate>
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                        <px:PXSelector ID="edRQRequest__OrderNbr" runat="server" AllowEdit="True" DataField="RQRequest__OrderNbr" />
                    </RowTemplate>
                    <Columns>
                        <px:PXGridColumn DataField="RQRequest__Priority" />
                        <px:PXGridColumn DataField="RQRequest__OrderNbr" />
                        <px:PXGridColumn DataField="RQRequest__OrderDate" />
                        <px:PXGridColumn DataField="RQRequest__EmployeeID" DisplayFormat="&gt;AAAAAAAAAA" />
                        <px:PXGridColumn DataField="RQRequest__DepartmentID" DisplayFormat="&gt;aaaaaaaaaa" />
                        <px:PXGridColumn DataField="RQRequestLine__InventoryID" DisplayFormat="&gt;AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" AllowUpdate="False" />
                        <px:PXGridColumn DataField="RQRequestLine__UOM" DisplayFormat="&gt;aaaaaa" AllowUpdate="False" />
                        <px:PXGridColumn DataField="RQRequestLine__Description" AllowUpdate="False" />
                        <px:PXGridColumn DataField="ItemQty" TextAlign="Right" AllowNull="False" />
                        <px:PXGridColumn DataField="RQRequestLine__OpenQty" TextAlign="Right" AllowNull="False" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="true" />
            <ActionBar DefaultAction="Content">
                <CustomItems>
                    <px:PXToolBarButton Text="Details" Tooltip="Show requiest details" Key="Content">
                        <AutoCallBack Command="viewRequest" Target="ds">
                            <Behavior CommitChanges="True" />
                        </AutoCallBack>
                    </px:PXToolBarButton>
                    <px:PXToolBarButton Text="Add Line" Tooltip="Add request item line" Key="ReqItem">
                        <AutoCallBack Command="addRequestContent" Target="ds">
                            <Behavior CommitChanges="True" />
                        </AutoCallBack>
                    </px:PXToolBarButton>
                </CustomItems>
            </ActionBar>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="OK" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="pnlOrderLines" runat="server" Key="OrderLines" DesignView="Content" CaptionVisible="true" Caption="Purchase Order Lines" Width="1000px" Height="400px"
        AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" AutoCallBack-Target="gridOrderLines" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        LoadOnDemand="true" ShowAfterLoad="true">
        <px:PXGrid ID="gridOrderLines" runat="server" DataSourceID="ds" Width="100%" BorderWidth="0px" SkinID="Details">
            <Mode AllowAddNew="false" AllowDelete="False" AllowUpdate="False" />
            <Levels>
                <px:PXGridLevel DataMember="OrderLines">
                    <RowTemplate>
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                        <px:PXSelector ID="edPOOrder__OrderNbr" runat="server" AllowEdit="True" DataField="POOrder__OrderNbr" />
                    </RowTemplate>
                    <Columns>
                        <px:PXGridColumn DataField="POOrder__OrderType" RenderEditorText="True" />
                        <px:PXGridColumn DataField="POOrder__OrderDate" />
                        <px:PXGridColumn DataField="POOrder__OrderNbr" />
                        <px:PXGridColumn DataField="POOrder__VendorID" DisplayFormat="&gt;AAAAAAAAAA" />
                        <px:PXGridColumn DataField="POOrder__VendorLocationID" DisplayFormat="&gt;AAAA" />
                        <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="POOrder__Status" RenderEditorText="True" />
                        <px:PXGridColumn DataField="LineType" RenderEditorText="True" />
                        <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" />
                        <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;A" />
                        <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" />
                        <px:PXGridColumn DataField="OrderQty" AllowNull="False" TextAlign="Right" />
                        <px:PXGridColumn DataField="ReceivedQty" AllowNull="False" AllowUpdate="False" TextAlign="Right" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="true" />
            <ActionBar DefaultAction="Content">
                <CustomItems>
                    <px:PXToolBarButton Text="Details" Tooltip="Show order details" Key="Content">
                        <AutoCallBack Command="viewOrderByLine" Target="ds">
                            <Behavior CommitChanges="True" />
                        </AutoCallBack>
                    </px:PXToolBarButton>
                </CustomItems>
            </ActionBar>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton3" runat="server" DialogResult="Cancel" Text="OK" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel runat="server" ID="panelVendor" CaptionVisible="true" Caption="Vendor Address" Key="Vendors" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True"
        AutoCallBack-Target="formVC" AllowCollapse="false">
        <px:PXPanel ID="panelVC" runat="server" Style="margin-top: 5px; z-index: 100; margin-left: 5px; margin-right: 5px; margin-bottom: 20px; position: relative; top: 0px;
            width: 420px; height: 380px;">
            <px:PXFormView ID="formVC" runat="server" Caption="Vendor Contact" DataMember="Bidding_Remit_Contact" DataSourceID="ds" Width="100%" AllowCollapse="false"  RenderStyle="Fieldset">
                <CallbackCommands>
                    <Refresh RepaintControlsIDs="formVA" RepaintControls="None" />
                    <Save RepaintControls="None" />
                </CallbackCommands>
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                    <px:PXCheckBox CommitChanges="True" ID="chkOverrideContact" runat="server" DataField="OverrideContact" />					
                    <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
                    <px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
                    <px:PXTextEdit ID="edPhone1" runat="server" DataField="Phone1" />
                    <px:PXMailEdit ID="edEmail" runat="server" DataField="Email" />
                </Template>
                <ContentStyle BackColor="Transparent" BorderStyle="None">
                </ContentStyle>
            </px:PXFormView>
            <px:PXFormView ID="formVA" DataMember="Bidding_Remit_Address" runat="server" DataSourceID="ds" Width="100%" Caption="Vendor Address" RenderStyle="Fieldset" SyncPosition="true" >
                <CallbackCommands>
                    <Save RepaintControls="None" />
                </CallbackCommands>
                <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                    <px:PXCheckBox CommitChanges="True" ID="chkOverrideAddress" runat="server" DataField="OverrideAddress" />					
                    <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
                    <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
                    <px:PXTextEdit ID="edCity" runat="server" DataField="City" />
                    <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AutoRefresh="True" CommitChanges="true" />
                    <px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="true">
                        <CallBackMode PostData="Container" />
                        <Parameters>
                            <px:PXControlParam ControlID="formVA" Name="PORemitAddress.countryID" PropertyName="DataControls[&quot;edCountryID&quot;].Value" Type="String" />
                        </Parameters>
                    </px:PXSelector>
                    <px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" CommitChanges="true" />
                </Template>
                <ContentStyle BackColor="Transparent" BorderStyle="None">
                </ContentStyle>
            </px:PXFormView>
        </px:PXPanel>
        <px:PXPanel ID="PXPanel4" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton4" runat="server" DialogResult="OK" Text="OK" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelAddSiteStatus" runat="server" Key="sitestatus" LoadOnDemand="true" Width="800px" Height="500px" Caption="Inventory Lookup" CaptionVisible="true"
        AutoCallBack-Command='Refresh' AutoCallBack-Enabled="True" AutoCallBack-Target="formSitesStatus" DesignView="Content">
        <px:PXFormView ID="formSitesStatus" runat="server" CaptionVisible="False" DataMember="sitestatusfilter" DataSourceID="ds" Width="100%" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXTextEdit CommitChanges="True" ID="edInventory" runat="server" DataField="Inventory" />
                <px:PXTextEdit CommitChanges="True" ID="edBarCode" runat="server" DataField="BarCode" />
                <px:PXCheckBox CommitChanges="True" ID="chkOnlyAvailable" runat="server" Checked="True" DataField="OnlyAvailable" />
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" />
                <px:PXSegmentMask CommitChanges="True" ID="edItemClassID" runat="server" DataField="ItemClass" />
                <px:PXSegmentMask CommitChanges="True" ID="edSubItem" runat="server" DataField="SubItem" AutoRefresh="true" /></Template>
        </px:PXFormView>
        <px:PXGrid ID="gripSiteStatus" runat="server" DataSourceID="ds" Style="border-width: 1px 0px; top: 0px; left: 0px;" AutoAdjustColumns="true" Width="100%" SkinID="Details"
            AdjustPageSize="Auto" Height="135px" AllowSearch="True" BatchUpdate="true" FastFilterID="edInventory" FastFilterFields="InventoryCD,Descr">
            <ClientEvents AfterCellUpdate="UpdateItemSiteCell" />
            <ActionBar PagerVisible="False">
                <PagerSettings Mode="NextPrevFirstLast" />
            </ActionBar>
            <Levels>
                <px:PXGridLevel DataMember="siteStatus">
                    <Mode AllowAddNew="false" AllowDelete="false" />
                    <RowTemplate>
                        <px:PXSegmentMask ID="editemClass" runat="server" DataField="ItemClassID" />
                    </RowTemplate>
                    <Columns>
                        <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AutoCallBack="true" AllowCheckAll="true" />
                        <px:PXGridColumn AllowNull="False" DataField="QtySelected" TextAlign="Right" />
                        <px:PXGridColumn DataField="SiteID" />
                        <px:PXGridColumn DataField="SiteCD" 
							AllowNull="False" SyncNullable ="false" 
							Visible="False" SyncVisible="false" 
							AllowShowHide ="False" SyncVisibility="false" />
                        <px:PXGridColumn DataField="ItemClassID" />
                        <px:PXGridColumn DataField="ItemClassDescription" />
                        <px:PXGridColumn DataField="PriceClassID" />
                        <px:PXGridColumn DataField="PriceClassDescription" />
                        <px:PXGridColumn DataField="PreferredVendorID" />
                        <px:PXGridColumn DataField="PreferredVendorDescription" />
                        <px:PXGridColumn DataField="InventoryCD" DisplayFormat="&gt;AAAAAAAAAA" />
                        <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-A" />
						<px:PXGridColumn DataField="SubItemCD"
							AllowNull="False" SyncNullable ="false" 
							Visible="False" SyncVisible="false" 
							AllowShowHide ="False" SyncVisibility="false" />
                        <px:PXGridColumn DataField="Descr" />
                        <px:PXGridColumn DataField="PurchaseUnit" DisplayFormat="&gt;aaaaaa" />
                        <px:PXGridColumn AllowNull="False" DataField="QtyAvailExt" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" DataField="QtyOnHandExt" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" DataField="QtyPOOrdersExt" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" DataField="QtyPOReceiptsExt" TextAlign="Right" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="true" />
        </px:PXGrid>
        <px:PXPanel ID="PXPanel5" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton6" runat="server" CommandName="AddInvSelBySite" CommandSourceID="ds" Text="Add" SyncVisible="false"/>
            <px:PXButton ID="PXButton7" runat="server" Text="Add & Close" DialogResult="OK" />
            <px:PXButton ID="PXButton8" runat="server" DialogResult="Cancel" Text="Cancel" />
           
        </px:PXPanel>
    </px:PXSmartPanel>
    <%-- Recalculate Prices --%>
    <px:PXSmartPanel ID="PanelRecalcPrices" runat="server" Caption="Recalculate Prices" CaptionVisible="true" LoadOnDemand="true" Key="recalcPricesFilter"
        AutoCallBack-Enabled="true" AutoCallBack-Target="formRecalcPrices" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
        CallBackMode-PostData="Page">
        <div style="padding: 5px">
            <px:PXFormView ID="formRecalcPrices" runat="server" DataSourceID="ds" CaptionVisible="False" DataMember="recalcPricesFilter">
                <Activity Height="" HighlightColor="" SelectedColor="" Width="" />
                <ContentStyle BackColor="Transparent" BorderStyle="None" />
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
                    <px:PXDropDown ID="edRecalcTerget" runat="server" DataField="RecalcTarget" Enabled="False"/>
                    <px:PXCheckBox CommitChanges="True" ID="chkRecalcUnitPrices" runat="server" DataField="RecalcUnitPrices" />
                    <px:PXCheckBox CommitChanges="True" ID="chkOverrideManualPrices" runat="server" DataField="OverrideManualPrices" />
                </Template>
            </px:PXFormView>
        </div>
        <px:PXPanel ID="PanelRecalcPricesButtons" runat="server" SkinID="Buttons">
            <px:PXButton ID="RecalculatePricesActionOk" runat="server" DialogResult="OK" Text="OK" CommandName="RecalculatePricesActionOk" CommandSourceID="ds" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Document" Caption="Document Summary" NoteIndicator="True"
        FilesIndicator="True" LinkIndicator="true" NotifyIndicator="true" ActivityIndicator="true" ActivityField="NoteActivity" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects"
        DefaultControlID="edReqNbr">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edReqNbr" runat="server" DataField="ReqNbr" />
            <px:PXDropDown Size="s" ID="edStatus" runat="server" AllowNull="False" DataField="Status" Enabled="False" />
            <px:PXCheckBox ID="chkHold" runat="server" Checked="True" DataField="Hold">
                <AutoCallBack Command="Hold" Target="ds" />
            </px:PXCheckBox>
            <px:PXDateTimeEdit Size="s" ID="edOrderDate" runat="server" DataField="OrderDate" />
            <px:PXCheckBox ID="chkApproved" runat="server" DataField="Approved" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXDropDown ID="edPriority" runat="server" AllowNull="False" DataField="Priority" SelectedIndex="1" />
            <px:PXSegmentMask ID="edEmployeeID" runat="server" DataField="EmployeeID" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" AllowEdit="true" />
            <px:PXSegmentMask ID="edCustomerLocationID" runat="server" DataField="CustomerLocationID" />
            <px:PXCheckBox CommitChanges="True" ID="chkQuoted" runat="server" DataField="Quoted" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <pxa:PXCurrencyRate ID="edCury" runat="server" DataField="CuryID" DataMember="_Currency_" DataSourceID="ds" RateTypeView="_RQRequisition_CurrencyInfo_" />
            <px:PXNumberEdit ID="edCuryEstExtCostTotal" runat="server" DataField="CuryEstExtCostTotal" Enabled="False" /></Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <script type="text/javascript">
        function UpdateItemSiteCell(n, c) {
            var activeRow = c.cell.row;
            var sCell = activeRow.getCell("Selected");
            var qCell = activeRow.getCell("QtySelected");
            if (sCell == c.cell) {
                if (sCell.getValue() == true)
                    qCell.setValue("1");
                else
                    qCell.setValue("0");
            }
            if (qCell == c.cell) {
                if (qCell.getValue() == "0")
                    sCell.setValue(false);
                else
                    sCell.setValue(true);
            }
        }
    </script>
    <px:PXTab ID="tab" runat="server" Height="504px" Style="z-index: 100;" Width="100%" DataSourceID="ds" DataMember="CurrentDocument">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="Document Details">
                <Template>
                    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; top: 0px; height: 388px;" Width="100%" BorderWidth="0px" SkinID="Details" StatusField="Availability" Height="388px" TabIndex="-19336" SyncPosition="true">
                        <Levels>
                            <px:PXGridLevel DataMember="Lines" DataKeyNames="ReqNbr,LineNbr">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXLayoutRule runat="server" Merge="True" />
                                    <px:PXDropDown CommitChanges="True" Size="s" ID="edLineType" runat="server" AllowNull="False" DataField="LineType">
                                        <Parameters>
                                            <px:PXSyncGridParam ControlID="grid" />
                                        </Parameters>
                                    </px:PXDropDown>
                                    <px:PXCheckBox ID="chkCancelled" runat="server" DataField="Cancelled" />
                                    <px:PXLayoutRule runat="server" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AutoRefresh="True" AllowEdit="True" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True" />
                                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
                                    <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID"  CommitChanges="True" AutoRefresh="True"/>
                                    <px:PXTextEdit ID="edAlternateID" runat="server" DataField="AlternateID" />
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edExpenseAcctID" runat="server" DataField="ExpenseAcctID" />
                                    <px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="ExpenseSubID" AutoRefresh="True" />
                                    <px:PXSelector CommitChanges="True" ID="edUOM" runat="server" DataField="UOM" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXSyncGridParam ControlID="grid" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXNumberEdit ID="edOrderQty" runat="server" DataField="OrderQty" CommitChanges="true" />
                                    <px:PXNumberEdit ID="edCuryEstUnitCost" runat="server" DataField="CuryEstUnitCost" />
                                    <px:PXCheckBox ID="chkManualPrice" runat="server" DataField="ManualPrice" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edCuryEstExtCost" runat="server" DataField="CuryEstExtCost" />
                                    <px:PXNumberEdit ID="edOriginQty" runat="server" DataField="OriginQty" Enabled="False" />
                                    <px:PXNumberEdit ID="edMarkupPct" runat="server" DataField="MarkupPct" />
                                    <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIsUseMarkup" runat="server" DataField="IsUseMarkup" />
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXNumberEdit ID="edRcptQtyMin" runat="server" DataField="RcptQtyMin" />
                                    <px:PXNumberEdit ID="edRcptQtyMax" runat="server" DataField="RcptQtyMax" />
                                    <px:PXNumberEdit ID="edRcptQtyThreshold" runat="server" DataField="RcptQtyThreshold" />
                                    <px:PXDropDown ID="edRcptQtyAction" runat="server" DataField="RcptQtyAction" />
                                    <px:PXDateTimeEdit ID="edRequestedDate" runat="server" DataField="RequestedDate" />
                                    <px:PXDateTimeEdit ID="edPromisedDate" runat="server" DataField="PromisedDate" />
                                    <px:PXTextEdit SuppressLabel="True" ID="reqNbr" runat="server" DataField="ReqNbr" />
                                    <px:PXTextEdit SuppressLabel="True" ID="lineNbr" runat="server" DataField="LineNbr" /></RowTemplate>
                                <Columns>
                                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" />
                                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right" Visible="False" />
                                    <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAA" CommitChanges="True" />
                                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;A" CommitChanges="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="LineSource" RenderEditorText="True" AllowUpdate="False" />
                                    <px:PXGridColumn AllowNull="False" DataField="LineType" RenderEditorText="True" MatrixMode="True" CommitChanges="True" />
                                    <px:PXGridColumn DataField="SiteID" DisplayFormat="&gt;AAAAAAAAAA" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" CommitChanges="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="OrderQty" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryEstUnitCost" MatrixMode="True" TextAlign="Right" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ManualPrice" TextAlign="Center" AllowNull="False" Type="CheckBox" CommitChanges="True"/>
                                    <px:PXGridColumn AllowNull="False" DataField="CuryEstExtCost" MatrixMode="True" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ExpenseAcctID" DisplayFormat="&gt;######" CommitChanges="true" />
                                    <px:PXGridColumn DataField="ExpenseSubID" DisplayFormat="&gt;AA-AA-AA-AA-AAA" />
                                    <px:PXGridColumn DataField="AlternateID" />
                                    <px:PXGridColumn AllowNull="False" DataField="IsUseMarkup" Label="Use Markup %" TextAlign="Center" Type="CheckBox" CommitChanges="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="MarkupPct" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RcptQtyMin" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RcptQtyMax" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RcptQtyThreshold" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RcptQtyAction" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="RequestedDate" />
                                    <px:PXGridColumn DataField="PromisedDate" />
                                    <px:PXGridColumn DataField="Cancelled" Type="CheckBox" TextAlign="Center" CommitChanges="True"  />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Layout FormViewHeight="400px" />
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" AllowFormEdit="True" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Details" Tooltip="Show request items details" Key="Content">
                                    <AutoCallBack Command="viewDetails" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Add Item" Key="cmdASI">
                                    <AutoCallBack Command="AddInvBySite" Target="ds">
                                        <Behavior PostData="Page" CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Add Item Request" Tooltip="Add requested items to requisition" Key="ReqItem">
                                    <AutoCallBack Command="addRequestLine" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Tranfer" Tooltip="Transfer checked line in another requisition" Key="Transfer" Visible="False">
                                    <AutoCallBack Command="transfer" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Merge" Tooltip="Merge selected line in requisition" Key="Merge">
                                    <AutoCallBack Command="merge" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Purchase Order" Tooltip="Show purchase order for selected line" Key="ViewOrder">
                                    <AutoCallBack Command="viewLineDetails" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Shipping Instructions">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Ship To:" />
                    <px:PXDropDown CommitChanges="True" ID="edShipDestType" runat="server" AllowNull="False" DataField="ShipDestType" />
                    <px:PXSelector CommitChanges="True" ID="edShipToBAccountID" runat="server" DataField="ShipToBAccountID" AutoRefresh="True" />
					<px:PXSegmentMask CommitChanges="True" SuppressLabel="False" ID="edSiteID" runat="server" DataField="SiteID" />
                    <px:PXSegmentMask ID="edShipToLocationID" runat="server" AutoRefresh="True" DataField="ShipToLocationID" />
                    <px:PXFormView ID="formSC" runat="server" Caption="Ship-To Contact" DataMember="Shipping_Contact" DataSourceID="ds" Width="100%" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkOverrideContact" runat="server" DataField="OverrideContact" />
                            <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
                            <px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
                            <px:PXTextEdit ID="edPhone1" runat="server" DataField="Phone1" />
                            <px:PXMailEdit ID="edEmail" runat="server" DataField="Email" />
                        </Template>
                    </px:PXFormView>
                    <px:PXFormView ID="formSA" runat="server" Caption="Ship-To Address" DataMember="Shipping_Address" DataSourceID="ds" Width="100%" SyncPosition="true" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkOverrideAddress" runat="server" DataField="OverrideAddress" />
					        <px:PXCheckBox ID="chkIsValidated" runat="server" DataField="IsValidated" Enabled="False"/>
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
                            <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
                            <px:PXTextEdit ID="edCity" runat="server" DataField="City" />
                            <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AutoRefresh="True" CommitChanges="true" />
                            <px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="True">
                                <CallBackMode PostData="Container" />
                                <Parameters>
                                    <px:PXControlParam ControlID="formSA" Name="POShipAddress.countryID" PropertyName="DataControls[&quot;edCountryID&quot;].Value" Type="String" />
                                </Parameters>
                            </px:PXSelector>
                            <px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" CommitChanges="true" />
                        </Template>
                    </px:PXFormView>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Ship Via:" />
                    <px:PXSelector ID="edFOBPoint" runat="server" DataField="FOBPoint" />
                    <px:PXSelector ID="edShipVia" runat="server" DataField="ShipVia" />
                    
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Bidding">
                <Template>
									<px:PXPanel runat="server" ID="pnlBidding" RenderStyle="Simple" ContentLayout-OuterSpacing="Around" RenderSimple="True">
										<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
										<px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" />
										<px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server" DataField="VendorLocationID"
											AutoRefresh="True" />
										<px:PXTextEdit ID="edVendorRefNbr" runat="server" DataField="VendorRefNbr" />
									</px:PXPanel>
									<px:PXGrid ID="gridVendor" runat="server" DataSourceID="ds" Width="100%" Height="200px" SkinID="DetailsWithFilter"
										Caption="Bidding Vendors" SyncPosition="True">
										<Mode InitNewRow="True" />
										<ActionBar>
											<CustomItems>
												<px:PXToolBarButton Text="Vendor Info" Tooltip="View vendor information" Key="vendor">
													<AutoCallBack Command="vendorInfo" Target="ds">
														<Behavior CommitChanges="True" />
													</AutoCallBack>
												</px:PXToolBarButton>
												<px:PXToolBarButton Text="Response" Tooltip="Show vendor response" Key="Content">
													<AutoCallBack Command="responseVendor" Target="ds">
														<Behavior CommitChanges="True" />
													</AutoCallBack>
												</px:PXToolBarButton>
												<px:PXToolBarButton Text="Choose" Tooltip="Choose selected vendor for all requisition" Key="Content">
													<AutoCallBack Command="chooseVendor" Target="ds">
														<Behavior CommitChanges="True" />
													</AutoCallBack>
												</px:PXToolBarButton>
												<px:PXToolBarButton Text="Send Request" Tooltip="Send request for proposal to the selected vendor">
													<AutoCallBack Command="VendorNotifications@Send Request" Target="ds">
														<Behavior CommitChanges="True" />
													</AutoCallBack>
												</px:PXToolBarButton>
											</CustomItems>
										</ActionBar>
										<Levels>
											<px:PXGridLevel DataMember="Vendors" DataKeyNames="LineID">
												<RowTemplate>
													<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
													<px:PXLayoutRule runat="server" Merge="True" />
													<px:PXSegmentMask ID="edBidVendorID" runat="server" DataField="VendorID" />
													<px:PXDateTimeEdit Size="s" ID="edExpireDateBv" runat="server" DataField="ExpireDate" />
													<px:PXLayoutRule runat="server" />
													<px:PXLayoutRule runat="server" Merge="True" />
													<px:PXSegmentMask ID="edBidVendorLocationID" runat="server" DataField="VendorLocationID" AutoRefresh="True">
														<Parameters>
															<px:PXSyncGridParam ControlID="gridVendor" />
														</Parameters>
													</px:PXSegmentMask>
													<px:PXDateTimeEdit Size="s" ID="edPromisedDateBv" runat="server" DataField="PromisedDate" />
													<px:PXLayoutRule runat="server" />
													<px:PXSelector ID="edFOBPointBv" runat="server" DataField="FOBPoint" />
													<px:PXSelector ID="edShipViaBv" runat="server" DataField="ShipVia" />
													<pxa:CurrencyEditor SuppressLabel="True" ID="CurrencyEditor1" Hidden="True" DataField="CuryInfoID" runat="server"
														DataSourceID="ds" DataMember="_RQBiddingVendor_CurrencyInfo_" />
												</RowTemplate>
												<Columns>
													<px:PXGridColumn DataField="VendorID" DisplayFormat="&gt;AAAAAAAAAA" CommitChanges="True" />
													<px:PXGridColumn DataField="VendorID_Vendor_AcctName" />
													<px:PXGridColumn DataField="VendorLocationID" DisplayFormat="&gt;AAAAAA" CommitChanges="True" />
													<px:PXGridColumn DataField="VendorLocationID_Location_Descr" />
													<px:PXGridColumn DataField="CuryInfoID" TextField="CuryID" />
													<px:PXGridColumn DataField="Location__VShipTermsID" />
													<px:PXGridColumn DataField="FOBPoint" DisplayFormat="&gt;aaaaaaaaaaaaaaa" />
													<px:PXGridColumn DataField="Location__VLeadTime" TextAlign="Right" />
													<px:PXGridColumn DataField="ShipVia" DisplayFormat="&gt;aaaaaaaaaaaaaaa" />
													<px:PXGridColumn DataField="ExpireDate" />
													<px:PXGridColumn DataField="PromisedDate" />
													<px:PXGridColumn AllowNull="False" DataField="Status" TextAlign="Center" Type="CheckBox" />
													<px:PXGridColumn DataField="RemitContactID" Visible="False" AllowShowHide="False" />
													<px:PXGridColumn DataField="RemitAddressID" Visible="False" AllowShowHide="False" />
												</Columns>
											</px:PXGridLevel>
										</Levels>
										<AutoSize Enabled="True" />
									</px:PXGrid>
								</Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Vendor Info">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
                    <px:PXFormView ID="formVC" runat="server" Caption="Vendor Contact" DataMember="Remit_Contact" DataSourceID="ds" Width="100%" AllowCollapse="False" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                            <px:PXCheckBox CommitChanges="True" ID="chkOverrideContact" runat="server" DataField="OverrideContact" />
                            <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
                            <px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
                            <px:PXTextEdit ID="edPhone1" runat="server" DataField="Phone1" />
                            <px:PXMailEdit ID="edEmail" runat="server" DataField="Email" />
                        </Template>
                        <ContentStyle BackColor="Transparent" BorderStyle="None">
                        </ContentStyle>
                    </px:PXFormView>
                    <px:PXFormView ID="formVA" DataMember="Remit_Address" runat="server" DataSourceID="ds" Width="100%" Caption="Vendor Address" AllowCollapse="False" SyncPosition="true" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                            <px:PXCheckBox CommitChanges="True" ID="chkOverrideAddress" runat="server" DataField="OverrideAddress" />
                            <px:PXCheckBox ID="chkIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
                            <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
                            <px:PXTextEdit ID="edCity" runat="server" DataField="City" />
                            <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AutoRefresh="True" CommitChanges="true" />
                            <px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="True">
                                <CallBackMode PostData="Container" />
                                <Parameters>
                                    <px:PXControlParam ControlID="formVA" Name="PORemitAddress.countryID" PropertyName="DataControls[&quot;edCountryID&quot;].Value" Type="String" />
                                </Parameters>
                            </px:PXSelector>
                            <px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" CommitChanges="true" />
                        </Template>
                        <ContentStyle BackColor="Transparent" BorderStyle="None">
                        </ContentStyle>
                    </px:PXFormView>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Info" />
                    <px:PXSelector CommitChanges="True" ID="edTermsID" runat="server" DataField="TermsID" />
                    <px:PXDropDown CommitChanges="True" ID="edPOType" runat="server" AllowNull="False" DataField="POType" />
                    <px:PXCheckBox Height="20px" ID="chkSplittable" runat="server" DataField="Splittable" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Approval Details" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" NoteIndicator="True" BorderWidth="0px">
                        <AutoSize Enabled="True" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <Levels>
                            <px:PXGridLevel DataMember="Approval" DataKeyNames="ApprovalID,AssignmentMapID">
                               <Columns>
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctCD" />
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctName" />
                                    <px:PXGridColumn DataField="WorkgroupID" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctCD" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctName" />
                                    <px:PXGridColumn DataField="ApproveDate" />
                                    <px:PXGridColumn DataField="Status" AllowNull="False" AllowUpdate="False" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Reason" AllowUpdate="False" />
                                    <px:PXGridColumn DataField="AssignmentMapID"  Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="RuleID" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="StepID" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="CreatedDateTime" Visible="false" SyncVisible="false" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Other Information">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                    <px:PXSelector ID="edWorkgroupID" runat="server" DataField="WorkgroupID" />
                    <px:PXSelector ID="edOwnerID" runat="server" DataField="OwnerID" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Purchase Orders">
                <Template>
                    <px:PXGrid ID="gridPOOrders" runat="server" DataSourceID="ds" Width="100%" BorderWidth="0px" SkinID="Details">
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <Levels>
                            <px:PXGridLevel DataMember="POOrders" DataKeyNames="OrderType,OrderNbr">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXDropDown ID="edOrderType" runat="server" DataField="OrderType" />
                                    <px:PXSelector ID="edOrderNbr" runat="server" AllowEdit="True" DataField="OrderNbr" edit="1" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="POOrder__OrderType" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="OrderNbr" LinkCommand="viewPOOrder" />
                                    <px:PXGridColumn DataField="Status" AllowUpdate="False" AllowNull="False" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="OrderDate" />
                                    <px:PXGridColumn DataField="VendorID" DisplayFormat="&gt;AAAAAAAAAA" />
                                    <px:PXGridColumn DataField="VendorLocationID" DisplayFormat="&gt;AAAA" />
                                    <px:PXGridColumn DataField="VendorRefNbr" />
                                    <px:PXGridColumn DataField="CuryID" DisplayFormat="&gt;LLLLL" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryLineTotal" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryTaxTotal" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryOrderTotal" TextAlign="Right" AllowNull="False" AllowUpdate="False" />
                                    <px:PXGridColumn DataField="EmployeeID" DisplayFormat="&gt;AAAAAAAAAA" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <ActionBar DefaultAction="Content">
                            <CustomItems>
                                <px:PXToolBarButton Text="Details" Tooltip="Show order details" Key="Content">
                                    <AutoCallBack Command="viewPOOrder" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Create Orders" Tooltip="Create purchase orders for requisition" Key="CreatePO">
                                    <AutoCallBack Command="CreatePOOrder" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Sales Orders">
                <Template>
                    <px:PXGrid ID="gridSOOrders" runat="server" DataSourceID="ds" Width="100%" BorderWidth="0px" SkinID="Details">
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <Levels>
                            <px:PXGridLevel DataMember="SOOrders" DataKeyNames="OrderType,OrderNbr">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXSelector ID="edOrderNbr1" runat="server" AllowEdit="True" DataField="OrderNbr" edit="1" />
                                    <px:PXSelector ID="edOrderType1" runat="server" AllowNull="False" DataField="OrderType" Text="SO" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="OrderType" AllowNull="False" DisplayFormat="&gt;aa" />
                                    <px:PXGridColumn DataField="OrderNbr" LinkCommand="viewSOOrder" />
                                    <px:PXGridColumn DataField="OrderDate" />
                                    <px:PXGridColumn DataField="Status" />
                                    <px:PXGridColumn DataField="CustomerID" DisplayFormat="&gt;AAAAAAAAAA" />
                                    <px:PXGridColumn DataField="CustomerLocationID" DisplayFormat="&gt;AAAA" />
                                    <px:PXGridColumn DataField="CuryID" DisplayFormat="&gt;LLLLL" />
                                    <px:PXGridColumn DataField="CuryLineTotal" AllowNull="False" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryTaxTotal" AllowNull="False" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryOrderTotal" TextAlign="Right" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <ActionBar DefaultAction="Content">
                            <CustomItems>
                                <px:PXToolBarButton Text="Details" Tooltip="Show order details" Key="Content">
                                    <AutoCallBack Command="viewSOOrder" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Create Quotation" Tooltip="Create quotation for requisition" Key="CreateQT">
                                    <AutoCallBack Command="CreateQTOrder" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <CallbackCommands>
            <Search CommitChanges="True" PostData="Page" />
            <Refresh CommitChanges="True" PostData="Page" />
        </CallbackCommands>
        <AutoSize Container="Window" Enabled="True" MinHeight="180" />
    </px:PXTab>
    <!--#include file="~\Pages\Includes\CRApprovalReasonPanel.inc"-->
</asp:Content>
