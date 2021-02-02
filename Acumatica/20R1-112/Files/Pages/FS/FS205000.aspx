<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="FS205000.aspx.cs" Inherits="Page_FS205000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" PageLoadBehavior="InsertRecord" 
        BorderStyle="NotSet" PrimaryView="EquipmentRecords" SuspendUnloading="False" 
        TypeName="PX.Objects.FS.SMEquipmentMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Action" CommitChanges="True"/>
            <px:PXDSCallbackCommand Name="InqueriesMenu" CommitChanges="True"/>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="Delete">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="OpenSource" Visible="False" />
            <px:PXDSCallbackCommand Name="ReplaceComponent" Visible="False" RepaintControls="All"/>
        </CallbackCommands>
    </px:PXDataSource>
    <%--Replace Component Smart Panel --%>
    <px:PXSmartPanel
            ID="edPXSmarthPanel"
            runat="server"
            Caption="Replace Component"
            CaptionVisible="True"
            Key="ReplaceComponentInfo"
            AutoCallBack-Command="Refresh"
            AutoCallBack-Target="PXGridWarranty"
            ShowAfterLoad="True"
            Width="800px"
            CloseAfterAction ="True"
            AutoReload="True"
            LoadOnDemand="True"
            TabIndex="8500"
            AutoRepaint="true"
            CallBackMode-CommitChanges="True"
            CallBackMode-PostData="Page" AllowResize="False"
            AcceptButtonID="btnOK" CancelButtonID="btnCancel">
        <px:PXLayoutRule runat="server" StartRow="true"></px:PXLayoutRule>
        <px:PXGrid ID="PXGridComponentSelected" runat="server" DataSourceID="ds" 
            SkinID="Inquire" TabIndex="4200" Width="100%" RepaintColumns="true" 
            AllowPaging="true" Caption="Selected Component for Replacement" CaptionVisible="true" AdjustPageSize="Auto">
            <Levels>
                <px:PXGridLevel 
                    DataMember="ComponentSelected">
                    <RowTemplate>
                        <px:PXSelector runat="server" ID="smItemClassID" DataField="ItemClassID" AutoRefresh="true" AllowEdit="true"> 
                        </px:PXSelector>
                        <px:PXSelector runat="server" ID="smInventoryID" DataField="InventoryID" AutoRefresh="true" AllowEdit="true"> 
                        </px:PXSelector>
                        <px:PXSelector runat="server" ID="smInstServiceOrderID" DataField="InstServiceOrderID" AllowEdit="true"> 
                        </px:PXSelector>
                        <px:PXSelector runat="server" ID="smInstAppointmentID" DataField="InstAppointmentID" AllowEdit="true"> 
                        </px:PXSelector>
                        <px:PXSelector runat="server" ID="smInvoiceRefNbr" DataField="InvoiceRefNbr" AllowEdit="true"> 
                        </px:PXSelector>
                        <px:PXSelector runat="server" ID="smSalesOrderNbr" DataField="SalesOrderNbr" AllowEdit="true"> 
                        </px:PXSelector>
                        <px:PXSelector runat="server" ID="smComponentReplaced" DataField="ComponentReplaced" AutoRefresh="true"> 
                        </px:PXSelector>
                        <px:PXSelector runat="server" ID="smVendorID" DataField="VendorID" AllowEdit="true"> 
                        </px:PXSelector>
                    </RowTemplate>
                    <Columns>
                        <px:PXGridColumn DataField="LineRef">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="ComponentID">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="Status" Type="DropDownList" CommitChanges="true">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="LongDescr">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="ItemClassID" CommitChanges="true">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="InventoryID" CommitChanges="true">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="SerialNumber">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="CpnyWarrantyDuration" TextAlign="Right" CommitChanges="True">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="CpnyWarrantyType" CommitChanges="True">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="CpnyWarrantyEndDate">
                        </px:PXGridColumn>  
                        <px:PXGridColumn DataField="VendorWarrantyDuration" TextAlign="Right" CommitChanges="True">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="VendorWarrantyType" CommitChanges="True">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="VendorWarrantyEndDate">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="VendorID">
                        </px:PXGridColumn>          
                        <px:PXGridColumn DataField="Comment">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="InstallationDate" CommitChanges="True">
                        </px:PXGridColumn>    
                        <px:PXGridColumn DataField="SalesDate" CommitChanges="True">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="InstServiceOrderID">
                        </px:PXGridColumn> 
                        <px:PXGridColumn DataField="InstAppointmentID">
                        </px:PXGridColumn>        
                        <px:PXGridColumn DataField="InvoiceRefNbr">
                        </px:PXGridColumn>    
                        <px:PXGridColumn DataField="SalesOrderNbr">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="ComponentReplaced">
                        </px:PXGridColumn>                           
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="200" />
            <Mode AllowAddNew="False" AllowDelete="False" InitNewRow="true"/>
        </px:PXGrid>
        <px:PXFormView ID="PXReplaceForm" runat="server" DataSourceID="ds" Style="z-index: 100" MarkRequired="Dynamic"
        Width="100%" DataMember="ReplaceComponentInfo" TabIndex="29900" DefaultControlID="edComponentID"
            CaptionVisible="true" Caption="Replacement Information"> 
            <Template>
                <px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="S">
                </px:PXLayoutRule>
                <px:PXDateTimeEdit ID="rInstallationDate" runat="server" 
                        DataField="InstallationDate" CommitChanges="true">
                </px:PXDateTimeEdit>
                <px:PXDateTimeEdit ID="rSalesDate" runat="server" 
                        DataField="SalesDate" CommitChanges="true">
                </px:PXDateTimeEdit>
                <px:PXLabel runat="server" Height="20px"></px:PXLabel>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S">
                </px:PXLayoutRule>
                <px:PXSelector runat="server" ID="rComponentID" DataField="ComponentID" AutoRefresh="true" CommitChanges="true"> 
                </px:PXSelector>
                <px:PXSelector runat="server" ID="rInventoryID" DataField="InventoryID" AutoRefresh="true" CommitChanges="true"> 
                </px:PXSelector>
                <px:PXLayoutRule runat="server"  StartRow="True" Merge="true">
                </px:PXLayoutRule>
                <px:PXButton ID="btnOK" runat="server" Text="Replace Component" AlignLeft="true" DialogResult="OK" >
                </px:PXButton>
                <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" AlignLeft="true">
                </px:PXButton>
            </Template>
        </px:PXFormView>
    </px:PXSmartPanel>
    <%--/Replace Component Smart Panel --%>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" MarkRequired="Dynamic"
        ActivityIndicator="True" Width="100%" DataMember="EquipmentRecords" TabIndex="29900" AllowCollapse="True" DefaultControlID="edDescr">
        <Template>
            <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" LabelsWidth="S" ControlSize="M">
            </px:PXLayoutRule>
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" DataSourceID="ds">
            </px:PXSelector>
            <px:PXSelector ID="edEquipmentTypeID" runat="server" AllowEdit="True" AutoRefresh="True"
                        DataField="EquipmentTypeID" CommitChanges ="true">
            </px:PXSelector>
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Size="S">
            </px:PXDropDown>
            <px:PXTextEdit ID="edSerialNumber" runat="server" DataField="SerialNumber">
            </px:PXTextEdit>
            <px:PXLayoutRule runat="server" StartColumn="True" >
            </px:PXLayoutRule>
            <px:PXLayoutRule runat="server" StartColumn="True" >
            </px:PXLayoutRule>
            <px:PXCheckBox ID="edIsVehicle" runat="server" AlignLeft="true" DataField="IsVehicle">
            </px:PXCheckBox>
            <px:PXLayoutRule runat="server">
            </px:PXLayoutRule>
            <px:PXCheckBox ID="edRequireMaintenance" runat="server" AlignLeft="true" DataField="RequireMaintenance" CommitChanges ="true">
            </px:PXCheckBox>
            <px:PXLayoutRule runat="server">
            </px:PXLayoutRule>
            <px:PXCheckBox ID="edResourceEquipment" runat="server" AlignLeft="true" DataField="ResourceEquipment" CommitChanges="true">
            </px:PXCheckBox>
            <px:PXLayoutRule runat="server" StartRow="True" ColumnSpan="2" LabelsWidth="S"></px:PXLayoutRule>
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr">
            </px:PXTextEdit>
            <px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="S"></px:PXLayoutRule>
            <px:PXGroupBox 
                ID="ebOwnerType" 
                runat="server" 
                Caption="Owner" 
                DataField="OwnerType" 
                CommitChanges="True" Width="320px">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="SM" LabelsWidth="S">
                    </px:PXLayoutRule>
                    <px:PXRadioButton ID="gbOwnerType_op0" runat="server" GroupName="gbOwnerType" Value="OW" />
                    <px:PXRadioButton ID="gbOwnerType_op1" runat="server" GroupName="gbOwnerType" Value="TP" />
                    <px:PXSegmentMask ID="edOwnerID" runat="server" DataField="OwnerID"
                        AllowEdit="True" AutoRefresh="True" CommitChanges="true">
                    </px:PXSegmentMask>
                </Template>
            </px:PXGroupBox>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S">
            </px:PXLayoutRule>
            <px:PXGroupBox 
                ID="ebLocationType" 
                runat="server" 
                Caption="Location" 
                DataField="LocationType" 
                CommitChanges="True" Width="320px">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="SM" LabelsWidth="S">
                    </px:PXLayoutRule>
                    <px:PXRadioButton ID="gbLocationType_op0" runat="server" GroupName="gbLocationType" Value="CO" />
                    <px:PXRadioButton ID="gbLocationType_op1" runat="server" GroupName="gbLocationType" Value="CU" />
                    <px:PXSelector ID="edBranchID" runat="server" DataField="BranchID"
                    AllowEdit="True" AutoRefresh="True" CommitChanges="true">
                    </px:PXSelector>
                    <px:PXSelector ID="edBranchLocationID" runat="server" AllowEdit="True" AutoRefresh="True"
                        DataField="BranchLocationID" CommitChanges="true">
                    </px:PXSelector>
                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID"
                        AllowEdit="True" AutoRefresh="True" CommitChanges="true">
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edCustomerLocationID" runat="server" AllowEdit="True" AutoRefresh="True"
                        DataField="CustomerLocationID" CommitChanges="true">
                    </px:PXSegmentMask>
                </Template>
            </px:PXGroupBox>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" 
        DataMember="EquipmentSelected" Style="z-index: 100" MarkRequired="Dynamic">
        <Items>
            <px:PXTabItem Text="General Info">
                <Template>
                    <px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="S" ControlSize="M">
                    </px:PXLayoutRule>
                    <px:PXDateTimeEdit ID="edRegisteredDate" runat="server" 
                        DataField="RegisteredDate">
                    </px:PXDateTimeEdit>
                    <px:PXTextEdit ID="edRegistrationNbr" runat="server" 
                        DataField="RegistrationNbr">
                    </px:PXTextEdit>
                    <px:PXTextEdit ID="edBarcode" runat="server" DataField="Barcode">
                    </px:PXTextEdit>
                    <px:PXTextEdit ID="edTagNbr" runat="server" DataField="TagNbr">
                    </px:PXTextEdit>
                    <px:PXDateTimeEdit ID="edSalesDate" runat="server"  DataField="SalesDate" CommitChanges="True">
                    </px:PXDateTimeEdit>
                    <px:PXDropDown ID="edColor" runat="server" DataField="Color">
                    </px:PXDropDown>
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Installation Info" LabelsWidth="S">
                    </px:PXLayoutRule>
                    <px:PXDateTimeEdit ID="edDateInstalled" runat="server" 
                        DataField="DateInstalled" CommitChanges="True">
                    </px:PXDateTimeEdit>
                    <px:PXSelector ID="edInstServiceOrderID" runat="server" DataField="InstServiceOrderID" AllowEdit="True" Enabled="False">
                    </px:PXSelector>
                    <px:PXSelector ID="edInstAppointmentID" runat="server" DataField="InstAppointmentID" AllowEdit="True" Enabled="False">
                    </px:PXSelector>
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Disposal Info" LabelsWidth="S">
                    </px:PXLayoutRule>
                    <px:PXDateTimeEdit ID="edDisposalDate" runat="server" 
                        DataField="DisposalDate">
                    </px:PXDateTimeEdit>
                    <px:PXSelector ID="edReplaceEquipmentID" runat="server" DataField="ReplaceEquipmentID" AllowEdit="True" AutoRefresh="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edDispServiceOrderID" runat="server" DataField="DispServiceOrderID" AllowEdit="True" Enabled="False">
                    </px:PXSelector>
                    <px:PXSelector ID="edDispAppointmentID" runat="server" DataField="DispAppointmentID" AllowEdit="True" Enabled="False">
                    </px:PXSelector>
                    <px:PXLayoutRule runat="server" StartColumn="True" StartGroup="True" GroupCaption="Manufacturer Info" LabelsWidth="S">
                    </px:PXLayoutRule>
                    <px:PXSelector ID="edManufacturerID" runat="server" AllowEdit="True" 
                        DataField="ManufacturerID" CommitChanges="True" AutoRefresh="true">
                    </px:PXSelector>
                    <px:PXSelector ID="edManufacturerModelID" runat="server"
                        DataField="ManufacturerModelID" AllowEdit="True" AutoRefresh="true" CommitChanges="True">
                    </px:PXSelector>
                    <px:PXTextEdit ID="edManufacturingYear" runat="server" 
                        DataField="ManufacturingYear">
                    </px:PXTextEdit>
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Inventory Info">
                    </px:PXLayoutRule>
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID"
                        AllowEdit="True" AutoRefresh="True" CommitChanges="true">
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edSiteID2" runat="server" DataField="SiteID" AllowEdit="True" AutoRefresh="True" CommitChanges="true">
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edLocationID" runat="server" AllowEdit="True" 
                        AutoRefresh="True" CommitChanges="True" DataField="LocationID">
                    </px:PXSegmentMask>
                    <px:PXTextEdit ID="edINSerialNumber" runat="server" DataField="INSerialNumber">
                    </px:PXTextEdit>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Vehicle Info" BindingContext="form" VisibleExp="DataControls[&quot;edIsVehicle&quot;].Value == true">
                <Template>
                    <px:PXLayoutRule runat="server" StartRow="True" 
                        StartColumn="True">
                    </px:PXLayoutRule>
                    <px:PXLayoutRule runat="server" GroupCaption="Vehicle Info" StartGroup="True" 
                        StartRow="True">
                    </px:PXLayoutRule>
                    <px:PXSelector ID="edVehicleTypeID" runat="server" DataField="VehicleTypeID">
                    </px:PXSelector>  
                    <px:PXTextEdit ID="edEngineNo" runat="server" DataField="EngineNo">
                    </px:PXTextEdit>
                    <px:PXNumberEdit ID="edAxles" runat="server" DataField="Axles">
                    </px:PXNumberEdit>
                    <px:PXNumberEdit ID="edMaxMiles" runat="server" DataField="MaxMiles">
                    </px:PXNumberEdit>
                    <px:PXNumberEdit ID="edTareWeight" runat="server" DataField="TareWeight">
                    </px:PXNumberEdit>
                    <px:PXNumberEdit ID="edWeightCapacity" runat="server" 
                        DataField="WeightCapacity">
                    </px:PXNumberEdit>
                    <px:PXNumberEdit ID="edGrossVehicleWeight" runat="server" 
                        DataField="GrossVehicleWeight">
                    </px:PXNumberEdit>        
                    <px:PXSelector ID="edColorID2" runat="server" DataField="ColorID">
                    </px:PXSelector>                    
                    <px:PXLayoutRule runat="server" GroupCaption="Fuel Info" StartGroup="True" 
                        StartColumn="True">
                    </px:PXLayoutRule>
                    <px:PXDropDown ID="edFuelType" runat="server" DataField="FuelType">
                    </px:PXDropDown>
                    <px:PXNumberEdit ID="edFuelTank1" runat="server" DataField="FuelTank1">
                    </px:PXNumberEdit>
                    <px:PXNumberEdit ID="edFuelTank2" runat="server" DataField="FuelTank2">
                    </px:PXNumberEdit>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Purchase Info">
                <Template>
                    <px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="S" ControlSize="M">
                    </px:PXLayoutRule>
                    <px:PXDropDown ID="edPropertyType" runat="server" DataField="PropertyType">
                    </px:PXDropDown>
                    <px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID">
                    </px:PXSegmentMask>
                    <px:PXDateTimeEdit ID="edPurchDate" runat="server" DataField="PurchDate">
                    </px:PXDateTimeEdit>
                    <px:PXTextEdit ID="edPurchPONumber" runat="server" DataField="PurchPONumber">
                    </px:PXTextEdit>
                    <px:PXNumberEdit ID="edPurchAmount" runat="server" DataField="PurchAmount">
                    </px:PXNumberEdit>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Components and Warranties">
                <Template>
                    <px:PXFormView runat="server" ID="formCompWarr" DataMember="EquipmentSelected" Style="z-index: 100" Width="100%">
                        <ContentStyle BackColor="Transparent" BorderStyle="None" >
                        </ContentStyle>
                        <Template>
                            <px:PXLayoutRule runat="server" GroupCaption="Company General Warranty" StartGroup="True" StartColumn="true" LabelsWidth="S" ControlSize="S">
                            </px:PXLayoutRule>
                            <px:PXLayoutRule runat="server" Merge="True">
                            </px:PXLayoutRule>
                            <px:PXNumberEdit ID="edCpnyWarrantyValue" runat="server" DataField="CpnyWarrantyValue" CommitChanges="True">
                            </px:PXNumberEdit>
                            <px:PXDropDown ID="edCpnyWarrantyType" runat="server" DataField="CpnyWarrantyType" SuppressLabel="True" CommitChanges="True">
                            </px:PXDropDown>
                            <px:PXLayoutRule runat="server" Merge="False">
                            </px:PXLayoutRule>
                            <px:PXDateTimeEdit ID="edCpnyWarrantyEndDate" runat="server" DataField="CpnyWarrantyEndDate" CommitChanges="True">
                            </px:PXDateTimeEdit>
                            <px:PXLayoutRule runat="server" GroupCaption="Vendor General Warranty" StartGroup="True" StartColumn="true" LabelsWidth="S" ControlSize="S">
                            </px:PXLayoutRule>
                            <px:PXLayoutRule runat="server" Merge="True">
                            </px:PXLayoutRule>
                            <px:PXNumberEdit ID="edVendorWarrantyValue" runat="server" DataField="VendorWarrantyValue" CommitChanges="True">
                            </px:PXNumberEdit>
                            <px:PXDropDown ID="edVendorWarrantyType" runat="server" DataField="VendorWarrantyType" SuppressLabel="True" CommitChanges="True">
                            </px:PXDropDown>
                            <px:PXLayoutRule runat="server" Merge="False">
                            </px:PXLayoutRule>
                            <px:PXDateTimeEdit ID="edVendorWarrantyEndDate" runat="server" DataField="VendorWarrantyEndDate">
                            </px:PXDateTimeEdit>
                            <px:PXLayoutRule runat="server" StartRow="true" ColumnWidth="100%">
                            </px:PXLayoutRule>
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid ID="PXGridWarranty" runat="server" DataSourceID="ds" 
                        SkinID="Details" TabIndex="4200" Width="100%" SyncPosition="True" KeepPosition="true">
                        <Levels>
                            <px:PXGridLevel 
                                DataMember="EquipmentWarranties">
                                <RowTemplate>
                                    <px:PXSelector runat="server" ID="edItemClassID" DataField="ItemClassID" AutoRefresh="true" AllowEdit="true"> 
                                    </px:PXSelector>
                                    <px:PXSelector runat="server" ID="edInventoryID" DataField="InventoryID" AutoRefresh="true" AllowEdit="true"> 
                                    </px:PXSelector>
                                    <px:PXSelector runat="server" ID="edInstServiceOrderID" DataField="InstServiceOrderID" AllowEdit="true"> 
                                    </px:PXSelector>
                                    <px:PXSelector runat="server" ID="edInstAppointmentID" DataField="InstAppointmentID" AllowEdit="true"> 
                                    </px:PXSelector>
                                    <px:PXSelector runat="server" ID="edInvoiceRefNbr" DataField="InvoiceRefNbr" AllowEdit="true"> 
                                    </px:PXSelector>
                                    <px:PXSelector runat="server" ID="edSalesOrderNbr" DataField="SalesOrderNbr" AllowEdit="true"> 
                                    </px:PXSelector>
                                    <px:PXSelector runat="server" ID="edComponentReplaced" DataField="ComponentReplaced" AutoRefresh="true" CommitChanges="true"> 
                                    </px:PXSelector>
                                    <px:PXSegmentMask runat="server" ID="edVendorID" DataField="VendorID" AllowEdit="true"> 
                                    </px:PXSegmentMask>
                                    <px:PXSelector runat="server" ID="edComponentID" DataField="ComponentID" AutoRefresh="true" AllowEdit="true"> 
                                    </px:PXSelector>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="LineRef">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ComponentID" CommitChanges="true">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Status" Type="DropDownList" CommitChanges="true">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="LongDescr">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ItemClassID" CommitChanges="true">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="InventoryID" CommitChanges="true">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SerialNumber">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CpnyWarrantyDuration" TextAlign="Right" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CpnyWarrantyType" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CpnyWarrantyEndDate">
                                    </px:PXGridColumn>  
                                    <px:PXGridColumn DataField="VendorWarrantyDuration" TextAlign="Right" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="VendorWarrantyType" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="VendorWarrantyEndDate">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="VendorID">
                                    </px:PXGridColumn>          
                                    <px:PXGridColumn DataField="Comment">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="InstallationDate" CommitChanges="True">
                                    </px:PXGridColumn>    
                                    <px:PXGridColumn DataField="SalesDate" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="InstServiceOrderID">
                                    </px:PXGridColumn> 
                                    <px:PXGridColumn DataField="InstAppointmentID">
                                    </px:PXGridColumn>        
                                    <px:PXGridColumn DataField="InvoiceRefNbr">
                                    </px:PXGridColumn>    
                                    <px:PXGridColumn DataField="SalesOrderNbr">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ComponentReplaced">
                                    </px:PXGridColumn>                           
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="300" />
                        <ActionBar ActionsText="False" PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdReplaceComponent">
                                    <AutoCallBack Target="ds" Command="ReplaceComponent" ></AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                    <px:PXGrid ID="PXGridAnswers" runat="server" Caption="Attributes" DataSourceID="ds" Height="150px" MatrixMode="True" Width="420px" SkinID="Attributes">
                        <Levels>
                            <px:PXGridLevel DataKeyNames="AttributeID,EntityType,EntityID" DataMember="Answers">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="M" StartColumn="True" />
                                    <px:PXTextEdit ID="edParameterID" runat="server" DataField="AttributeID" Enabled="False" />
                                    <px:PXTextEdit ID="edAnswerValue" runat="server" DataField="Value" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn AllowShowHide="False" DataField="AttributeID" TextField="AttributeID_description" TextAlign="Left" />
                                    <px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Value" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                    <px:PXImageUploader Height="320px" Width="430px" ID="imgUploader" runat="server" DataField="ImageUrl" AllowUpload="true" ShowComment="true" DataMember="EquipmentRecords"/>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Source Info">
                <Template>
                    <px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="S">
                    </px:PXLayoutRule>
                    <px:PXDropDown ID="edSourceType" runat="server" DataField="SourceType">
                    </px:PXDropDown>
                    <px:PXTextEdit ID="edSourceRefNbr" runat="server" DataField="SourceRefNbr" Enabled="false">
                        <LinkCommand Target="ds" Command="OpenSource"></LinkCommand>
                    </px:PXTextEdit>
                    <px:PXSelector ID="edSalesOrderNbr2" runat="server" DataField="SalesOrderNbr" AllowEdit="True" Enabled="False">
                    </px:PXSelector>
                    <px:PXSelector ID="edEquipmentReplacedID" runat="server" DataField="EquipmentReplacedID" AllowEdit="True" Enabled="False">
                    </px:PXSelector>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" />
    </px:PXTab>
</asp:Content>